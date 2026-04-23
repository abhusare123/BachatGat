import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FundSummary, LoanLedgerItem, MonthlyReport } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getFundSummary(groupId: number) {
    return this.http.get<FundSummary>(`${this.API}/groups/${groupId}/reports/fund-summary`);
  }

  getLoanLedger(groupId: number) {
    return this.http.get<LoanLedgerItem[]>(`${this.API}/groups/${groupId}/reports/loan-ledger`);
  }

  getMemberStatement(groupId: number) {
    return this.http.get<any>(`${this.API}/users/me/reports/statement?groupId=${groupId}`);
  }

  getMonthlyReport(groupId: number, period?: string) {
    const qs = period ? `?period=${period}` : '';
    return this.http.get<MonthlyReport>(`${this.API}/groups/${groupId}/reports/monthly${qs}`);
  }
}
