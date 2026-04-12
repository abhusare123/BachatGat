# BachatGat — Architecture Guide

## Overview

Clean Architecture with four layers. Dependencies flow inward only:

```
BachatGat.Api  →  BachatGat.Application  →  BachatGat.Core
                          ↑
              BachatGat.Infrastructure
```

- **Core** — domain entities, enums, interfaces. No external dependencies.
- **Infrastructure** — EF Core DbContext, SMS stub, loan calculator. Depends on Core.
- **Application** — business logic services, DTOs, custom exceptions. Depends on Core. Infrastructure registers implementations via DI.
- **Api** — ASP.NET Core controllers, JWT auth, Swagger. Depends on Application.

---

## Backend

### BachatGat.Core

**Entities** (`src/BachatGat.Core/Entities/`)

| Entity | Key Fields | Notes |
|---|---|---|
| `User` | `Id, PhoneNumber, FullName, CreatedAt` | One user can belong to many groups |
| `OtpCode` | `PhoneNumber, Code, ExpiresAt, IsUsed` | 6-digit, 5-min TTL |
| `Group` | `Name, MonthlyAmount, InterestRatePercent, CreatedByUserId` | `MonthlyAmount` = saving per member/month |
| `GroupMember` | `GroupId, UserId, Role, JoinedAt, IsActive` | Unique index on `(GroupId, UserId)` |
| `Contribution` | `GroupMemberId, Period, AmountPaid, IsApproved, ApprovedByUserId` | Period = `"YYYY-MM"`. Unique on `(GroupMemberId, Period)` |
| `Loan` | `GroupId, RequestedByUserId, Amount, TenureMonths, InterestRatePercent, Status` | Status enum: Pending → Approved → Active → Closed |
| `LoanVote` | `LoanId, VotedByUserId, Vote` | Unique on `(LoanId, VotedByUserId)` |
| `LoanRepayment` | `LoanId, Period, EMIAmount, PrincipalAmount, InterestAmount, IsPaid` | Generated on disbursal. Unique on `(LoanId, Period)` |

**Enums**

```csharp
GroupMemberRole:  Admin=1, Treasurer=2, Member=3, Auditor=4
LoanStatus:       Pending=1, Approved=2, Rejected=3, Active=4, Closed=5
VoteChoice:       Approve=1, Reject=2
```

**Role hierarchy** — lower number = higher privilege. Role comparisons use `> GroupMemberRole.Treasurer` to mean "Member or Auditor only".

### BachatGat.Infrastructure

- **`AppDbContext`** (`Data/AppDbContext.cs`) — EF Core DbContext implementing `IAppDbContext`. All fluent API config (precision, unique indexes, delete behaviors) is here.
- **`ConsoleSmsService`** — implements `ISmsService`, logs OTP as `LogWarning("=== [SMS STUB] OTP for {Phone}: {Otp} ===")`
- **`LoanCalculatorService`** — implements `ILoanCalculatorService`:
  ```
  EMI = P × r × (1+r)^n / ((1+r)^n - 1)   where r = monthlyRate/100
  ```
  Generates full amortization schedule as `AmortizationEntry(Period, EMI, Principal, Interest, OutstandingBalance)`.
- **`InfrastructureServiceExtensions`** — registers DbContext (SQL Server), `ISmsService`, `ILoanCalculatorService` via `AddInfrastructure(IConfiguration)`.

### BachatGat.Application

**Services** (`Services/`)

| Service | Responsibility |
|---|---|
| `AuthService` | Send OTP, verify OTP + upsert User, issue JWT + refresh token |
| `GroupService` | CRUD groups, add/remove members |
| `ContributionService` | Record/update/approve contributions, build tracker grid, calculate NextEmi |
| `LoanService` | Request, vote, approve, reject, disburse (generates repayment schedule), mark repayment paid |
| `ReportService` | Fund summary, loan ledger, member statement |

**Authorization pattern** — every service method starts with a membership/role check:
```csharp
var membership = await db.GroupMembers.FirstOrDefaultAsync(...) ?? throw ForbiddenException / NotFoundException;
if (membership.Role > GroupMemberRole.Treasurer) throw ForbiddenException;
```

**Custom exceptions** (`Exceptions/`)
- `NotFoundException` → `404`
- `ForbiddenException` → `403`
- `ConflictException` → `409`
- `BadRequestException` → `400`
Mapped in `Program.cs` via global exception handler.

**DTOs** (`DTOs/`) — All request/response shapes. Records (immutable). See `API.md` for shapes.

### BachatGat.Api

- **`Program.cs`** — wires: `AddInfrastructure`, `AddApplication`, JWT bearer auth, Swagger, CORS (`localhost:4200`), global exception middleware, auto-migrate on startup.
- **Controllers** — thin; extract `CurrentUserId` from JWT claim `ClaimTypes.NameIdentifier`, delegate to service.
- **No custom auth policies** — authorization is enforced inside Application services (membership checks), not at controller level beyond `[Authorize]`.

---

## Frontend (Angular 19)

### Architecture

Standalone components + lazy-loaded feature modules. `paramsInheritanceStrategy: 'always'` in router so child components in lazy modules can read `:id` from parent route.

```
app.routes.ts
├── /login  → AuthModule (lazy)
└── /groups → GroupsModule (lazy)
    └── :id → GroupShellComponent (sidebar layout)
        ├── contributions → ContributionsModule (lazy)
        ├── loans         → LoansModule (lazy)
        ├── reports       → ReportsModule (lazy)
        └── members       → GroupDetailComponent
```

### Route Param Inheritance Pattern

Child components in lazy modules **must** traverse the route tree to read `:id`:
```typescript
// In every child component's ngOnInit():
let r = this.route.snapshot;
while (r && !r.paramMap.has('id')) r = r.parent!;
this.groupId = +(r?.paramMap.get('id') ?? 0);
```
**Do not** use `this.route.snapshot.paramMap.get('id')` directly — returns null in lazy children.

### Key Services (`src/app/core/`)

| Service | Endpoints wrapped |
|---|---|
| `AuthService` | send-otp, verify-otp, refresh. Stores JWT in `localStorage`. Signal `currentUser()`. |
| `GroupService` | groups CRUD, members |
| `ContributionService` | tracker, record, update, **approve** |
| `LoanService` | loans CRUD, vote, approve/reject, disburse, repayments, markPaid |
| `ReportService` | fund-summary, loan-ledger, member-statement |

### Auth Interceptor
`auth.interceptor.ts` — appends `Authorization: Bearer <token>` to every HTTP request automatically.

### Important Components

| Component | Path | Role |
|---|---|---|
| `GroupShellComponent` | `groups/group-shell/` | Sidebar layout; reads group name; nav: Contributions → Loans → Reports → Members |
| `TrackerComponent` | `contributions/tracker/` | Excel-style grid. Shows pending (orange) vs approved (green) cells. Approve button for Admin. Next EMI column (blue). |
| `LoanListComponent` | `loans/loan-list/` | Loan cards with vote panel, admin actions, "Repayments" button for Active/Closed |
| `RepaymentListComponent` | `loans/repayment-list/` | EMI schedule table, live outstanding balance, Mark Paid button |
| `FundSummaryComponent` | `reports/fund-summary/` | Dashboard cards: collected, disbursed, balance, interest |

### Angular Material Note

Project uses Material 3 (M3). **Do not use `color="primary"` on `<mat-toolbar>`** — M3 renders it nearly white. Use `class="app-navbar"` with `background: #1b5e20 !important` override in `styles.scss`.

---

## Database

### Connection
`LocalDB` by default (`(localdb)\mssqllocaldb`). Change `ConnectionStrings:DefaultConnection` in `appsettings.json` for staging/prod.

### Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName> -p src/BachatGat.Infrastructure -s src/BachatGat.Api

# Apply to database
dotnet ef database update -s src/BachatGat.Api

# Rollback
dotnet ef migrations remove -p src/BachatGat.Infrastructure -s src/BachatGat.Api
```

### Applied Migrations
| Migration | What it adds |
|---|---|
| `InitialCreate` | All base tables |
| `AddContributionApproval` | `IsApproved`, `ApprovedAt`, `ApprovedByUserId` on `Contributions` |

---

## Authentication Flow

```
1. POST /api/auth/send-otp   { phoneNumber }
   → generates 6-digit OTP, saves to OtpCodes, calls ISmsService (logs to console)

2. POST /api/auth/verify-otp  { phoneNumber, otp, fullName? }
   → validates OTP (5-min window, single-use)
   → upserts User record
   → returns { accessToken (JWT, 60min), refreshToken (GUID, 7 days), userId, fullName, phoneNumber }

3. POST /api/auth/refresh  { refreshToken }
   → issues new access token

JWT claims: sub = userId, phone = phoneNumber
```

---

## Business Rules

### Contributions
- `MonthlyAmount` (e.g. ₹2,000) is set when group is created; cannot change after
- Treasurer records a contribution → `IsApproved = false` (pending, orange in UI)
- Admin records → auto-approved immediately
- Admin can approve any pending contribution → counts in totals
- Editing a contribution (Treasurer) resets approval to pending
- Only **approved** contributions count toward: `GrandTotal`, `PeriodTotals`, `RunningTotal` per member

### Loans
- Any member (not Auditor) can request a loan
- Voting: only Members and Admins vote; simple majority; cannot vote on own loan
- Vote can be changed after submission
- Admin/Treasurer can approve or reject regardless of votes
- Disbursal generates full amortization schedule (`LoanRepayment` rows), sets status = Active
- Each `LoanRepayment.Period` = month the payment is due (YYYY-MM)
- When all repayments are marked paid → status auto-transitions to Closed
- Interest = reducing balance: `outstanding × (rate/100)` per month

### Next EMI (in Tracker)
```
NextEmi = group.MonthlyAmount + (nextUnpaidRepayment.PrincipalAmount + nextUnpaidRepayment.InterestAmount)
        = Saving + (0 if no active loan)
```
