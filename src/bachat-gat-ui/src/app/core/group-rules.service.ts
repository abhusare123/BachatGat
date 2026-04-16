import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GroupRulesResponse, UpdateRuleRequest } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GroupRulesService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getRules(groupId: number): Observable<GroupRulesResponse> {
    return this.http.get<GroupRulesResponse>(`${this.API}/groups/${groupId}/rules`);
  }

  updateRule(groupId: number, ruleKey: string, request: UpdateRuleRequest): Observable<void> {
    return this.http.put<void>(`${this.API}/groups/${groupId}/rules/${ruleKey}`, request);
  }
}
