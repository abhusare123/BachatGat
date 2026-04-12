# BachatGat — Features & Business Rules

## Implemented Features

### 1. Phone OTP Authentication
- Enter phone number → 6-digit OTP delivered (printed to console in dev)
- OTP valid for 5 minutes, single-use
- On first login, user provides their full name (stored)
- Returns JWT access token (60 min) + refresh token (7 days)
- Refresh token can issue new access tokens without re-OTP

---

### 2. Group Management
- Any logged-in user can create a group → becomes Admin automatically
- Group has: `name`, `description`, `monthlyAmount` (fixed saving per member), `interestRatePercent`
- **`monthlyAmount` cannot be changed after group creation** — it is the standard contribution amount
- Admin can add members by phone number; new users are created if needed
- Admin can remove (deactivate) members; their historical data is preserved
- If a user belongs to only one group, the app auto-navigates to that group's contributions page

---

### 3. Monthly Contribution Tracker

**Recording contributions**
- Treasurer or Admin records a contribution for any member
- One contribution per member per period (YYYY-MM); duplicate throws `409`
- Editing a contribution resets approval to Pending (unless editor is Admin)

**Approval workflow**
| Recorder | Auto-approved? |
|---|---|
| Admin | ✓ Yes — counts immediately |
| Treasurer | ✗ No — Pending (excluded from totals until Admin approves) |

**Tracker grid**
- Excel-style layout: members as rows, months as columns
- Pending cells: amber/orange background with ⏳ icon
- Approved cells: green background
- Admin sees an approve button (✓) on pending cells on hover
- **Totals and GrandTotal only count approved contributions**

**Next EMI column (blue)**
- Shows each member's total payment due next month
- Formula: `MonthlyAmount + nextUnpaidLoan.PrincipalAmount + nextUnpaidLoan.InterestAmount`
- Members with no active loan: shows only `MonthlyAmount` (e.g. ₹2,000)
- Members with active loan: shows combined amount with "(बचत + कर्ज)" label and tooltip breakdown
- "Next unpaid" = the first `LoanRepayment` with `IsPaid = false` ordered by period

---

### 4. Loan Lifecycle

```
[Member] Request Loan
         ↓
[Members + Admin] Vote (Approve / Reject)
         ↓
[Admin / Treasurer] Formally Approve or Reject
         ↓
[Admin / Treasurer] Disburse → generates repayment schedule
         ↓
[Admin / Treasurer] Mark monthly repayments as Paid
         ↓
[System] Last repayment paid → auto-closes loan
```

**Voting rules**
- Only Members and Admins can vote (Treasurer and Auditor cannot)
- Cannot vote on your own loan request
- Vote can be changed after initial submission
- Voting is advisory — Admin/Treasurer makes the final approval decision

**Loan interest**
- Rate is fixed at time of loan request (copied from `Group.InterestRatePercent`)
- Reducing balance method: `interest = outstandingBalance × (rate/100)` per month
- EMI formula: `P × r × (1+r)^n / ((1+r)^n - 1)`

**Repayment schedule**
- Generated automatically when loan is disbursed
- Each `LoanRepayment` row: `Period (YYYY-MM)`, `EMIAmount`, `PrincipalAmount`, `InterestAmount`
- Treasurer/Admin marks each installment as paid
- Outstanding balance = sum of unpaid principal portions
- When last row is paid → loan status auto-transitions to `Closed`

---

### 5. Reports

**Fund Summary** (`/reports/fund-summary`)
- Total Contributions Collected (approved only)
- Total Loans Disbursed
- Total Loan Outstanding (disbursed − principal repaid)
- Total Interest Collected (from paid repayments)
- Available Balance = Contributions + Interest − Disbursed + Principal Repaid

**Loan Ledger** (`/reports/loan-ledger`)
- All non-pending, non-rejected loans
- Per loan: original amount, outstanding balance, interest paid, status

**Member Statement** (`/users/me/reports/statement?groupId=`)
- Individual's contribution history
- Individual's loans with vote counts

---

## Role Permission Matrix

| Action | Admin | Treasurer | Member | Auditor |
|---|:---:|:---:|:---:|:---:|
| Create group | ✓ | — | — | — |
| Add/remove members | ✓ | — | — | — |
| Record contribution | ✓ | ✓ | — | — |
| Approve contribution | ✓ | — | — | — |
| Request loan | ✓ | ✓ | ✓ | — |
| Vote on loan | ✓ | — | ✓ | — |
| Approve/reject/disburse loan | ✓ | ✓ | — | — |
| Mark repayment paid | ✓ | ✓ | — | — |
| View tracker | ✓ | ✓ | ✓ | ✓ |
| View reports | ✓ | ✓ | ✓ | ✓ |

---

## Navigation Structure (Angular)

```
/login                              → OTP login
/groups                             → Group list (auto-redirects if 1 group)
/groups/:id/contributions           → Monthly Contribution Tracker ← primary screen
/groups/:id/loans                   → Loan list + vote + admin actions
/groups/:id/loans/request           → Submit loan request
/groups/:id/loans/:loanId/repayments → EMI schedule + mark paid
/groups/:id/reports                 → Fund summary + loan ledger
/groups/:id/members                 → Member list + add/remove
```

Sidebar navigation priority: बचत (Contributions) → कर्ज (Loans) → अहवाल (Reports) → सदस्य (Members)

---

## Pending / Future Features

| Feature | Notes |
|---|---|
| Marathi i18n | Use `@angular/localize`; locale switcher in navbar |
| Real SMS provider | Replace `ConsoleSmsService` with Twilio/MSG91 — change only DI registration |
| Member statement UI | Backend exists (`/users/me/reports/statement`); needs Angular component |
| Excel export for tracker | Backend: add `/contributions/tracker/export` using `ClosedXML` or `EPPlus` |
| Payment reminders | Push notifications or SMS when next EMI is due |
| Group-level interest rate change | Currently locked; would need historical rate tracking |
| Audit log | Track who recorded/approved what and when |

---

## Known Design Decisions

| Decision | Rationale |
|---|---|
| `paramsInheritanceStrategy: 'always'` | Required for `:id` param to be accessible in lazy-loaded child modules |
| Route tree traversal in child components | `snapshot.paramMap.get('id')` returns null in lazy children; must walk `r.parent` |
| Approval resets on edit | Treasurer edit after Admin approval would be misleading; re-approval ensures Admin signs off on final amount |
| Admin auto-approves own records | Admin has authority and adding a self-approval step would add friction without value |
| No loan amount limit in system | Admin rejects at disbursal if funds are insufficient — keeps the system flexible |
