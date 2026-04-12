import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ContributionTracker } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ContributionService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTracker(groupId: number) {
    return this.http.get<ContributionTracker>(`${this.API}/groups/${groupId}/contributions/tracker`);
  }

  recordContribution(groupId: number, groupMemberId: number, period: string, amountPaid: number) {
    return this.http.post(`${this.API}/groups/${groupId}/contributions`, { groupMemberId, period, amountPaid });
  }

  updateContribution(groupId: number, contributionId: number, amountPaid: number) {
    return this.http.put(`${this.API}/groups/${groupId}/contributions/${contributionId}`, { amountPaid });
  }
}
