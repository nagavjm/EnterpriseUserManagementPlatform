import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  User,
  Department,
  UserRole,
  UserQueryParameters
} from '../../../shared/models/user.model';
import { PagedResult } from '../../../shared/models/api-response.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    LoadingSpinnerComponent,
    ConfirmModalComponent
  ],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  private notificationService = inject(NotificationService);

  pagedUsers: PagedResult<User> | null = null;
  isLoading = false;

  // Filter & Search states
  searchTerm = '';
  selectedDepartment: Department | '' = '';
  selectedRole: UserRole | '' = '';
  selectedStatus: string = ''; // '', 'true', 'false'
  pageSize = 10;
  currentPage = 1;
  sortBy = 'CreatedDate';
  sortDescending = true;

  // Enums for dropdowns
  departments = Object.values(Department);
  roles = Object.values(UserRole);

  // Delete modal state
  isDeleteModalOpen = false;
  userToDelete: User | null = null;

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;

    const params: UserQueryParameters = {
      searchTerm: this.searchTerm,
      department: this.selectedDepartment || undefined,
      role: this.selectedRole || undefined,
      isActive: this.selectedStatus === '' ? undefined : this.selectedStatus === 'true',
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending
    };

    this.userService.getUsers(params).subscribe({
      next: (response) => {
        if (response.success) {
          this.pagedUsers = response.data;
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onResetFilters(): void {
    this.searchTerm = '';
    this.selectedDepartment = '';
    this.selectedRole = '';
    this.selectedStatus = '';
    this.currentPage = 1;
    this.sortBy = 'CreatedDate';
    this.sortDescending = true;
    this.loadUsers();
  }

  onSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDescending = !this.sortDescending;
    } else {
      this.sortBy = column;
      this.sortDescending = false;
    }
    this.loadUsers();
  }

  goToPage(page: number): void {
    if (page >= 1 && (!this.pagedUsers || page <= this.pagedUsers.totalPages)) {
      this.currentPage = page;
      this.loadUsers();
    }
  }

  toggleStatus(user: User): void {
    const newStatus = !user.isActive;
    this.userService.updateUserStatus(user.id, newStatus).subscribe({
      next: (res) => {
        if (res.success) {
          user.isActive = newStatus;
          user.modifiedDate = res.data.modifiedDate;
          this.notificationService.showSuccess(
            `User '${user.fullName}' has been ${newStatus ? 'activated' : 'deactivated'}.`
          );
        }
      }
    });
  }

  openDeleteModal(user: User): void {
    this.userToDelete = user;
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.userToDelete = null;
    this.isDeleteModalOpen = false;
  }

  confirmDelete(): void {
    if (!this.userToDelete) return;

    const id = this.userToDelete.id;
    const name = this.userToDelete.fullName;

    this.userService.deleteUser(id).subscribe({
      next: () => {
        this.notificationService.showSuccess(`User '${name}' deleted successfully.`);
        this.closeDeleteModal();
        this.loadUsers();
      },
      error: () => {
        this.closeDeleteModal();
      }
    });
  }

  getPageNumbers(): number[] {
    if (!this.pagedUsers) return [];
    const total = this.pagedUsers.totalPages;
    const current = this.currentPage;
    const delta = 2;
    const range: number[] = [];

    for (let i = Math.max(1, current - delta); i <= Math.min(total, current + delta); i++) {
      range.push(i);
    }
    return range;
  }
}
