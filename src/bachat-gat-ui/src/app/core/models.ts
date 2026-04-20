export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  userId: number;
  fullName: string;
  phoneNumber: string;
}

export interface Group {
  id: number;
  name: string;
  description?: string;
  monthlyAmount: number;
  interestRatePercent: number;
  createdAt: string;
  memberCount: number;
}

export interface GroupDetail extends Omit<Group, 'memberCount'> {
  members: GroupMember[];
}

export interface GroupMember {
  id: number;
  userId: number;
  fullName: string;
  phoneNumber: string;
  role: GroupMemberRole;
  joinedAt: string;
  isActive: boolean;
}

export enum GroupMemberRole {
  Admin = 1,
  Treasurer = 2,
  Member = 3,
  Auditor = 4
}

export interface ContributionTracker {
  periods: string[];
  rows: MemberTrackerRow[];
  periodTotals: PeriodTotal[];
  grandTotal: number;
}

export interface MemberTrackerRow {
  groupMemberId: number;
  memberName: string;
  cells: ContributionCell[];
  runningTotal: number;
  nextEmi: number;
  nextEmiSaving: number;
  nextEmiLoanPrincipal: number;
  nextEmiLoanInterest: number;
}

export interface ContributionCell {
  contributionId?: number;
  period: string;
  amountPaid: number;
  cumulativeTotal: number;
  isPaid: boolean;
  isApproved: boolean;
}

export interface PeriodTotal {
  period: string;
  total: number;
  outstanding: number;
}

export interface Loan {
  id: number;
  groupId: number;
  requestedByUserId: number;
  requestedByName: string;
  amount: number;
  tenureMonths: number;
  interestRatePercent: number;
  purpose?: string;
  status: LoanStatus;
  requestedAt: string;
  approvedAt?: string;
  closedAt?: string;
  approveVotes: number;
  rejectVotes: number;
  totalEligibleVoters: number;
  currentUserVote?: number; // 1 = Approve, 2 = Reject
}

export enum LoanStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Active = 4,
  Closed = 5
}

export interface ForeclosureSummary {
  outstandingPrincipal: number;
  foreclosureInterest: number;
  totalAmount: number;
}

export interface LoanRepayment {
  id: number;
  period: string;
  emiAmount: number;
  principalAmount: number;
  interestAmount: number;
  isPaid: boolean;
  isForeclosed: boolean;
  paidAt?: string;
}

export interface FundSummary {
  totalContributionsCollected: number;
  totalLoansDisbursed: number;
  totalLoanOutstanding: number;
  totalInterestCollected: number;
  totalExpenses: number;
  availableBalance: number;
}

export interface LoanLedgerItem {
  loanId: number;
  memberName: string;
  originalAmount: number;
  outstandingBalance: number;
  totalInterestPaid: number;
  status: string;
  requestedAt: string;
}

export interface ExpenseDto {
  id: number;
  description: string;
  amount: number;
  category: string;
  date: string;
  recordedAt: string;
  recordedByName: string;
}

export interface AddExpenseRequest {
  description: string;
  amount: number;
  category: number;
  date: string;
}

export interface UserProfile {
  id: number;
  fullName: string;
  phoneNumber: string;
  email: string | null;
  address: string | null;
  createdAt: string;
}

export interface UpdateUserProfileRequest {
  fullName: string;
  email: string | null;
  address: string | null;
}

export interface ConfigurableRule {
  key: string;
  label: string;
  labelMr: string;
  value: string;
  unit: string;
  description: string;
  descriptionMr: string;
}

export interface GroupRulesResponse {
  configurableRules: ConfigurableRule[];
  interestRatePercent: number;
}

export interface UpdateRuleRequest {
  value: string;
}
