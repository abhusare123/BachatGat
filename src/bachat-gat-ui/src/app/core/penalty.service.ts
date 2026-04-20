import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AddPenaltyRequest, PenaltyDto } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PenaltyService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getPenalties(groupId: number) {
    return this.http.get<PenaltyDto[]>(`${this.API}/groups/${groupId}/penalties`);
  }

  addPenalty(groupId: number, request: AddPenaltyRequest) {
    return this.http.post<{ id: number }>(`${this.API}/groups/${groupId}/penalties`, request);
  }
}
