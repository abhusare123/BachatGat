import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Group, GroupDetail } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GroupService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getGroups() {
    return this.http.get<Group[]>(`${this.API}/groups`);
  }

  getGroup(id: number) {
    return this.http.get<GroupDetail>(`${this.API}/groups/${id}`);
  }

  createGroup(payload: { name: string; description?: string; monthlyAmount: number; interestRatePercent: number }) {
    return this.http.post<Group>(`${this.API}/groups`, payload);
  }

  updateGroup(id: number, payload: { name: string; description?: string; monthlyAmount: number; interestRatePercent: number }) {
    return this.http.put(`${this.API}/groups/${id}`, payload);
  }

  addMember(groupId: number, phoneNumber: string | undefined, email: string | undefined, role: number, fullName?: string) {
    const body: any = { role };
    if (phoneNumber) body.phoneNumber = phoneNumber;
    if (email) body.email = email;
    if (fullName) body.fullName = fullName;
    return this.http.post(`${this.API}/groups/${groupId}/members`, body);
  }

  removeMember(groupId: number, memberId: number) {
    return this.http.delete(`${this.API}/groups/${groupId}/members/${memberId}`);
  }
}
