# BachatGat — Rotating Savings Group Manager

A full-stack web application for managing **Bachat Gat** (बचत गट) — rotating savings and credit associations (ROSCA) common in Maharashtra, India. Digitizes an Excel-based system where members pool monthly contributions and can take loans repaid via monthly EMIs.

---

## Quick Start

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB ships with Visual Studio)
- Node.js 20+ / npm

### Run Backend
```bash
cd src/BachatGat.Api
dotnet run
# → Swagger UI at https://localhost:5001/swagger
# → Auto-migrates database on first run
# → OTP codes are printed to the console (SMS stub)
```

### Run Frontend
```bash
cd src/bachat-gat-ui
npm install
ng serve
# → http://localhost:4200
```

### First-Time Setup
1. Start backend — it auto-creates the database and runs all migrations
2. Open `http://localhost:4200` → you'll see the OTP login screen
3. Enter any phone number → check the backend console for the OTP code
4. After login → create a group → add members → start recording contributions

---

## Configuration

| File | Setting | Default |
|---|---|---|
| `src/BachatGat.Api/appsettings.json` | `ConnectionStrings:DefaultConnection` | LocalDB |
| `src/BachatGat.Api/appsettings.json` | `Jwt:Key` | Change for production! |
| `src/bachat-gat-ui/src/environments/environment.ts` | `apiUrl` | `https://localhost:5001/api` |

---

## Project Structure

```
BachatGat/
├── src/
│   ├── BachatGat.Core/           # Domain: entities, enums, interfaces
│   ├── BachatGat.Infrastructure/ # EF Core DbContext, SMS stub, loan calculator
│   ├── BachatGat.Application/    # Business logic: services, DTOs, exceptions
│   ├── BachatGat.Api/            # ASP.NET Core Web API, controllers
│   └── bachat-gat-ui/            # Angular 19 frontend
├── docs/
│   ├── ARCHITECTURE.md           # Detailed architecture guide
│   ├── API.md                    # All API endpoints reference
│   └── FEATURES.md               # Feature descriptions and business rules
└── README.md
```

---

## Key Features

- **Phone OTP login** — 6-digit OTP, 5-min expiry, JWT tokens
- **Multiple groups** — each group is independent with its own members and funds
- **4 user roles** — Admin, Treasurer, Member, Auditor
- **Monthly contribution tracker** — Excel-style grid (members × months), with pending/approved states
- **Contribution approval** — Treasurer records → Pending; Admin approves → counts in totals
- **Loan lifecycle** — Request → Member vote → Admin approval → Disburse → Monthly EMI repayment
- **Next EMI column** — Shows each member's upcoming payment = Saving + Loan installment
- **Reports** — Fund summary (total collected, disbursed, balance, interest earned) + Loan ledger
- **Reducing balance EMI** — Standard `P × r × (1+r)^n / ((1+r)^n - 1)` formula

---

## Pending / Future Work

- Marathi i18n translations (`@angular/localize`)
- Real SMS provider (Twilio / MSG91) — swap `ConsoleSmsService` in DI
- Member statement report UI
- Excel export for tracker grid
- Push notifications for payment reminders

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 19, Angular Material (M3), standalone components |
| Backend | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 10, code-first migrations |
| Database | SQL Server (LocalDB for dev) |
| Auth | Phone OTP + JWT Bearer (access + refresh tokens) |
| Architecture | Clean Architecture — Core → Infrastructure ← Application ← Api |
