import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Loan, LoanRepayment } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LoanService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getLoans(groupId: number) {
    return this.http.get<Loan[]>(`${this.API}/groups/${groupId}/loans`);
  }

  requestLoan(groupId: number, amount: number, tenureMonths: number, purpose?: string) {
    return this.http.post(`${this.API}/groups/${groupId}/loans`, { amount, tenureMonths, purpose });
  }

  vote(loanId: number, vote: number, comment?: string) {
    return this.http.post(`${this.API}/loans/${loanId}/vote`, { vote, comment });
  }

  approveLoan(loanId: number) {
    return this.http.post(`${this.API}/loans/${loanId}/approve`, {});
  }

  rejectLoan(loanId: number) {
    return this.http.post(`${this.API}/loans/${loanId}/reject`, {});
  }

  disburse(loanId: number) {
    return this.http.post(`${this.API}/loans/${loanId}/disburse`, {});
  }

  getRepayments(loanId: number) {
    return this.http.get<LoanRepayment[]>(`${this.API}/loans/${loanId}/repayments`);
  }

  markRepaymentPaid(loanId: number, repaymentId: number) {
    return this.http.post(`${this.API}/loans/${loanId}/repayments/${repaymentId}/pay`, {});
  }
}
