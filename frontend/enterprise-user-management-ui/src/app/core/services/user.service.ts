import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import {
  User,
  CreateUserRequest,
  UpdateUserRequest,
  UserQueryParameters
} from '../../shared/models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/users`;

  getUsers(params?: UserQueryParameters): Observable<ApiResponse<PagedResult<User>>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.searchTerm) {
        httpParams = httpParams.set('searchTerm', params.searchTerm.trim());
      }
      if (params.department) {
        httpParams = httpParams.set('department', params.department);
      }
      if (params.role) {
        httpParams = httpParams.set('role', params.role);
      }
      if (params.isActive !== undefined && params.isActive !== '') {
        httpParams = httpParams.set('isActive', params.isActive.toString());
      }
      if (params.pageNumber) {
        httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      }
      if (params.pageSize) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
      if (params.sortBy) {
        httpParams = httpParams.set('sortBy', params.sortBy);
      }
      if (params.sortDescending !== undefined) {
        httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
      }
    }

    return this.http.get<ApiResponse<PagedResult<User>>>(this.apiUrl, { params: httpParams });
  }

  getUserById(id: string): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(this.apiUrl, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/${id}`, request);
  }

  updateUserStatus(id: string, isActive: boolean): Observable<ApiResponse<User>> {
    return this.http.patch<ApiResponse<User>>(`${this.apiUrl}/${id}/status`, { isActive });
  }

  deleteUser(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }
}

