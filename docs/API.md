# BachatGat — API Reference

Base URL: `https://localhost:5001/api`

All endpoints except `/auth/*` require `Authorization: Bearer <accessToken>` header.

Error responses follow `{ message: string }` shape. HTTP status codes:
- `400` BadRequest, `401` Unauthorized, `403` Forbidden, `404` NotFound, `409` Conflict

---

## Auth

### POST `/auth/send-otp`
Send OTP to phone number. OTP is printed to console (SMS stub).

**Request**
```json
{ "phoneNumber": "9876543210" }
```
**Response** `200`
```json
{ "message": "OTP sent" }
```

---

### POST `/auth/verify-otp`
Verify OTP and receive JWT tokens. Creates user if first login.

**Request**
```json
{ "phoneNumber": "9876543210", "otp": "123456", "fullName": "Priya Patil" }
```
**Response** `200`
```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<guid>",
  "userId": 1,
  "fullName": "Priya Patil",
  "phoneNumber": "9876543210"
}
```

---

### POST `/auth/refresh`
Refresh access token using a valid refresh token.

**Request**
```json
{ "refreshToken": "<guid>" }
```
**Response** `200` — same shape as verify-otp response.

---

## Groups

### GET `/groups`
List all groups the caller belongs to.

**Response** `200`
```json
[{
  "id": 1,
  "name": "Laxmi Bachat Gat",
  "description": "Women's savings group",
  "monthlyAmount": 2000.00,
  "interestRatePercent": 2.00,
  "createdAt": "2026-01-01T00:00:00Z",
  "memberCount": 10
}]
```

---

### POST `/groups`
Create a new group. Caller becomes Admin.

**Request**
```json
{
  "name": "Laxmi Bachat Gat",
  "description": "optional",
  "monthlyAmount": 2000,
  "interestRatePercent": 2.0
}
```
**Response** `201` — group object (same as GET item).

---

### GET `/groups/{id}`
Get group detail with members list.

**Response** `200`
```json
{
  "id": 1,
  "name": "Laxmi Bachat Gat",
  "monthlyAmount": 2000.00,
  "interestRatePercent": 2.00,
  "members": [{
    "id": 1,
    "userId": 5,
    "fullName": "Priya Patil",
    "phoneNumber": "9876543210",
    "role": 1,
    "joinedAt": "2026-01-01T00:00:00Z",
    "isActive": true
  }]
}
```
Roles: `1=Admin, 2=Treasurer, 3=Member, 4=Auditor`

---

### PUT `/groups/{id}`
Update group name/description/interest rate. **Admin or Treasurer only.**

**Request**
```json
{ "name": "New Name", "description": "optional", "interestRatePercent": 2.5 }
```
**Response** `204`

---

### POST `/groups/{id}/members`
Add a member to the group. **Admin only.**

**Request**
```json
{ "phoneNumber": "9876543211", "role": 3 }
```
**Response** `200 { "message": "Member added" }`

---

### DELETE `/groups/{id}/members/{memberId}`
Remove (deactivate) a member. **Admin only.**

**Response** `204`

---

## Contributions

### GET `/groups/{groupId}/contributions`
List all contributions. Optional query param: `?period=2026-03`

**Response** `200`
```json
[{
  "id": 1,
  "groupMemberId": 1,
  "memberName": "Priya Patil",
  "period": "2026-03",
  "amountPaid": 2000.00,
  "paidAt": "2026-03-05T10:00:00Z",
  "isApproved": true,
  "approvedAt": "2026-03-06T09:00:00Z"
}]
```

---

### POST `/groups/{groupId}/contributions`
Record a contribution. **Treasurer or Admin only.**
- Admin → auto-approved
- Treasurer → status Pending (needs Admin approval)

**Request**
```json
{ "groupMemberId": 1, "period": "2026-03", "amountPaid": 2000 }
```
**Response** `200 { "message": "Contribution recorded" }`

---

### PUT `/groups/{groupId}/contributions/{contributionId}`
Update amount. **Treasurer or Admin only.** Resets `IsApproved` to false (unless editor is Admin).

**Request**
```json
{ "amountPaid": 1500 }
```
**Response** `200 { "message": "Contribution updated" }`

---

### POST `/groups/{groupId}/contributions/{contributionId}/approve`
Approve a pending contribution. **Admin only.**

**Response** `200 { "message": "Contribution approved." }`

---

### GET `/groups/{groupId}/contributions/tracker`
Excel-style grid: members × months. Only **approved** contributions count in totals.

**Response** `200`
```json
{
  "periods": ["2026-01", "2026-02", "2026-03"],
  "rows": [{
    "groupMemberId": 1,
    "memberName": "Priya Patil",
    "cells": [{
      "contributionId": 1,
      "period": "2026-01",
      "amountPaid": 2000.00,
      "cumulativeTotal": 2000.00,
      "isPaid": true,
      "isApproved": true
    }],
    "runningTotal": 6000.00,
    "nextEmi": 4234.56,
    "nextEmiSaving": 2000.00,
    "nextEmiLoanPrincipal": 1765.23,
    "nextEmiLoanInterest": 469.33
  }],
  "periodTotals": [{
    "period": "2026-01",
    "total": 20000.00,
    "outstanding": 0.00
  }],
  "grandTotal": 60000.00
}
```

> `nextEmi` = `nextEmiSaving + nextEmiLoanPrincipal + nextEmiLoanInterest`
> `nextEmiLoanPrincipal` and `nextEmiLoanInterest` are 0 if member has no active loan.

---

## Loans

### GET `/groups/{groupId}/loans`
List all loans in the group.

**Response** `200`
```json
[{
  "id": 1,
  "groupId": 1,
  "requestedByUserId": 5,
  "requestedByName": "Priya Patil",
  "amount": 50000.00,
  "tenureMonths": 12,
  "interestRatePercent": 2.00,
  "purpose": "Business",
  "status": 4,
  "requestedAt": "2026-02-01T00:00:00Z",
  "approvedAt": "2026-02-03T00:00:00Z",
  "approveVotes": 7,
  "rejectVotes": 1,
  "totalEligibleVoters": 9,
  "currentUserVote": 1
}]
```
Status: `1=Pending, 2=Approved, 3=Rejected, 4=Active, 5=Closed`
`currentUserVote`: `1=Approve, 2=Reject, null=not voted`

---

### POST `/groups/{groupId}/loans`
Submit a loan request. **Any member except Auditor.**

**Request**
```json
{ "amount": 50000, "tenureMonths": 12, "purpose": "Business" }
```
**Response** `201 { "id": 1 }`

---

### GET `/loans/{id}`
Get single loan detail.

---

### POST `/loans/{id}/vote`
Cast or change a vote. **Members and Admins only; cannot vote on own loan.**

**Request**
```json
{ "vote": 1, "comment": "Looks good" }
```
`vote`: `1=Approve, 2=Reject`

**Response** `200 { "message": "Vote recorded", "status": 1 }`

---

### POST `/loans/{id}/approve`
Admin/Treasurer approves the loan. Status: Pending → Approved.

**Response** `200 { "message": "Loan approved" }`

---

### POST `/loans/{id}/reject`
Admin/Treasurer rejects the loan.

**Response** `200 { "message": "Loan rejected" }`

---

### POST `/loans/{id}/disburse`
Mark loan as disbursed; generates full repayment schedule. Status: Approved → Active. **Admin/Treasurer only.**

**Response** `200 { "message": "Loan disbursed and repayment schedule generated" }`

---

### GET `/loans/{id}/repayments`
Get full repayment schedule.

**Response** `200`
```json
[{
  "id": 1,
  "period": "2026-03",
  "emiAmount": 4704.56,
  "principalAmount": 3704.56,
  "interestAmount": 1000.00,
  "isPaid": true,
  "paidAt": "2026-03-10T00:00:00Z"
}]
```

---

### POST `/loans/{id}/repayments/{repaymentId}/pay`
Mark a repayment installment as paid. **Admin/Treasurer only.**
If this is the last unpaid installment, loan status transitions to Closed automatically.

**Response** `200 { "message": "Repayment recorded", "loanStatus": 4 }`

---

## Reports

### GET `/groups/{groupId}/reports/fund-summary`
Financial overview of the group.

**Response** `200`
```json
{
  "totalContributionsCollected": 120000.00,
  "totalLoansDisbursed": 50000.00,
  "totalLoanOutstanding": 32000.00,
  "totalInterestCollected": 4200.00,
  "availableBalance": 74200.00
}
```
`availableBalance = totalContributions + totalInterestCollected - totalLoansDisbursed + totalPrincipalRepaid`

---

### GET `/groups/{groupId}/reports/loan-ledger`
All active/closed loans with outstanding balances.

**Response** `200`
```json
[{
  "loanId": 1,
  "memberName": "Priya Patil",
  "originalAmount": 50000.00,
  "outstandingBalance": 32000.00,
  "totalInterestPaid": 2100.00,
  "status": "Active",
  "requestedAt": "2026-02-01T00:00:00Z"
}]
```

---

### GET `/users/me/reports/statement?groupId={groupId}`
Individual member's contribution history and loan summary.

**Response** `200`
```json
{
  "memberName": "Priya Patil",
  "groupName": "Laxmi Bachat Gat",
  "contributions": [...],
  "loans": [...],
  "totalContributed": 24000.00
}
```
