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
}

export interface ContributionCell {
  contributionId?: number;
  period: string;
  amountPaid: number;
  cumulativeTotal: number;
  isPaid: boolean;
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

export interface LoanRepayment {
  id: number;
  period: string;
  emiAmount: number;
  principalAmount: number;
  interestAmount: number;
  isPaid: boolean;
  paidAt?: string;
}

export interface FundSummary {
  totalContributionsCollected: number;
  totalLoansDisbursed: number;
  totalLoanOutstanding: number;
  totalInterestCollected: number;
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
