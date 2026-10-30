import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Department, UserRole } from '../../../shared/models/user.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, LoadingSpinnerComponent],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.css']
})
export class UserFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  userForm!: FormGroup;
  isEditMode = false;
  userId: string | null = null;
  isLoading = false;
  isSubmitting = false;

  departments = Object.values(Department);
  roles = Object.values(UserRole);

  ngOnInit(): void {
    this.initForm();
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.userId = id;
        this.loadUserForEdit(id);
      }
    });
  }

  private initForm(): void {
    this.userForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      department: ['', [Validators.required]],
      role: ['', [Validators.required]]
    });
  }

  private loadUserForEdit(id: string): void {
    this.isLoading = true;
    this.userService.getUserById(id).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const user = response.data;
          this.userForm.patchValue({
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            department: user.department,
            role: user.role
          });
          // Disable email editing in edit mode to preserve unique identifier
          this.userForm.get('email')?.disable();
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.router.navigate(['/users']);
      }
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.userForm.getRawValue();

    if (this.isEditMode && this.userId) {
      this.userService.updateUser(this.userId, {
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        department: formValue.department,
        role: formValue.role
      }).subscribe({
        next: (res) => {
          this.isSubmitting = false;
          if (res.success) {
            this.notificationService.showSuccess(`User '${res.data.fullName}' updated successfully.`);
            this.router.navigate(['/users']);
          }
        },
        error: () => {
          this.isSubmitting = false;
        }
      });
    } else {
      this.userService.createUser({
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        email: formValue.email,
        department: formValue.department,
        role: formValue.role
      }).subscribe({
        next: (res) => {
          this.isSubmitting = false;
          if (res.success) {
            this.notificationService.showSuccess(`User '${res.data.fullName}' created successfully.`);
            this.router.navigate(['/users']);
          }
        },
        error: (err) => {
          this.isSubmitting = false;
          if (err.status === 409) {
            this.userForm.get('email')?.setErrors({ duplicate: true });
          }
        }
      });
    }
  }

  get f() {
    return this.userForm.controls;
  }
}

