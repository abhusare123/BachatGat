import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AddGroupIncomeRequest, GroupIncomeDto } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GroupIncomeService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getEntries(groupId: number) {
    return this.http.get<GroupIncomeDto[]>(`${this.API}/groups/${groupId}/income`);
  }

  addEntry(groupId: number, request: AddGroupIncomeRequest) {
    return this.http.post<{ id: number }>(`${this.API}/groups/${groupId}/income`, request);
  }

  deleteEntry(groupId: number, entryId: number) {
    return this.http.delete(`${this.API}/groups/${groupId}/income/${entryId}`);
  }
}
