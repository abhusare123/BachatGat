import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AddExpenseRequest, ExpenseDto } from './models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private readonly API = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getExpenses(groupId: number) {
    return this.http.get<ExpenseDto[]>(`${this.API}/groups/${groupId}/expenses`);
  }

  addExpense(groupId: number, request: AddExpenseRequest) {
    return this.http.post<{ id: number }>(`${this.API}/groups/${groupId}/expenses`, request);
  }

  deleteExpense(groupId: number, expenseId: number) {
    return this.http.delete(`${this.API}/groups/${groupId}/expenses/${expenseId}`);
  }
}
