import { Component, OnInit, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Department, UserRole } from '../../../shared/models/user.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ComponentCanDeactivate } from '../../../core/guards/unsaved-changes.guard';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, LoadingSpinnerComponent],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.css']
})
export class UserFormComponent implements OnInit, ComponentCanDeactivate {
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

  private initialFormValue: any = null;

  departments = Object.values(Department);
  roles = Object.values(UserRole);

  /**
   * Browser unload listener: warns the user when attempting to close tab,
   * reload (F5), or navigate away to an external URL with unsaved changes.
   */
  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.hasUnsavedChanges()) {
      event.preventDefault();
      // Modern browsers require returnValue to be set to trigger prompt
      event.returnValue = true;
    }
  }

  /**
   * Returns true if there are pending unsaved changes on the form.
   * Implements ComponentCanDeactivate for Angular route transitions.
   */
  hasUnsavedChanges(): boolean {
    return !this.isSubmitting && !!this.userForm && this.userForm.dirty;
  }

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
    this.initialFormValue = this.userForm.getRawValue();
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
          this.userForm.markAsPristine();
          this.initialFormValue = this.userForm.getRawValue();
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.router.navigate(['/users']);
      }
    });
  }

  /**
   * Reverts all changes back to initial state and marks form as pristine.
   */
  resetForm(): void {
    if (this.initialFormValue) {
      this.userForm.reset(this.initialFormValue);
      if (this.isEditMode) {
        this.userForm.get('email')?.disable();
      }
    } else {
      this.userForm.reset({
        firstName: '',
        lastName: '',
        email: '',
        department: '',
        role: ''
      });
    }
    this.userForm.markAsPristine();
    this.notificationService.showInfo('Form changes have been reverted.');
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
            this.userForm.markAsPristine();
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
            this.userForm.markAsPristine();
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

