import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { User } from '../../../shared/models/user.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ConfirmModalComponent } from '../../../shared/components/confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-user-details',
  standalone: true,
  imports: [CommonModule, RouterModule, LoadingSpinnerComponent, ConfirmModalComponent],
  templateUrl: './user-details.component.html',
  styleUrls: ['./user-details.component.css']
})
export class UserDetailsComponent implements OnInit {
  private userService = inject(UserService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  user: User | null = null;
  isLoading = true;
  isDeleteModalOpen = false;

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loadUser(id);
      } else {
        this.router.navigate(['/users']);
      }
    });
  }

  loadUser(id: string): void {
    this.isLoading = true;
    this.userService.getUserById(id).subscribe({
      next: (response) => {
        if (response.success) {
          this.user = response.data;
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.router.navigate(['/users']);
      }
    });
  }

  toggleStatus(): void {
    if (!this.user) return;

    const newStatus = !this.user.isActive;
    this.userService.updateUserStatus(this.user.id, newStatus).subscribe({
      next: (res) => {
        if (res.success && this.user) {
          this.user.isActive = newStatus;
          this.user.modifiedDate = res.data.modifiedDate;
          this.notificationService.showSuccess(
            `User '${this.user.fullName}' has been ${newStatus ? 'activated' : 'deactivated'}.`
          );
        }
      }
    });
  }

  openDeleteModal(): void {
    this.isDeleteModalOpen = true;
  }

  closeDeleteModal(): void {
    this.isDeleteModalOpen = false;
  }

  confirmDelete(): void {
    if (!this.user) return;

    const id = this.user.id;
    const name = this.user.fullName;

    this.userService.deleteUser(id).subscribe({
      next: () => {
        this.notificationService.showSuccess(`User '${name}' deleted successfully.`);
        this.closeDeleteModal();
        this.router.navigate(['/users']);
      },
      error: () => {
        this.closeDeleteModal();
      }
    });
  }
}

