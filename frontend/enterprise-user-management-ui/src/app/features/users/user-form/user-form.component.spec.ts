import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserFormComponent } from './user-form.component';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('UserFormComponent', () => {
  let component: UserFormComponent;
  let fixture: ComponentFixture<UserFormComponent>;
  let userServiceSpy: jasmine.SpyObj<UserService>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  beforeEach(async () => {
    userServiceSpy = jasmine.createSpyObj('UserService', ['getUserById', 'createUser', 'updateUser']);
    notificationServiceSpy = jasmine.createSpyObj('NotificationService', ['showSuccess', 'showError', 'showInfo']);

    await TestBed.configureTestingModule({
      imports: [UserFormComponent],
      providers: [
        provideRouter([]),
        { provide: UserService, useValue: userServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and initialize pristine form', () => {
    expect(component).toBeTruthy();
    expect(component.userForm).toBeDefined();
    expect(component.userForm.dirty).toBeFalse();
    expect(component.hasUnsavedChanges()).toBeFalse();
  });

  it('should report hasUnsavedChanges as true when user modifies a form field', () => {
    component.userForm.controls['firstName'].setValue('Alice');
    component.userForm.controls['firstName'].markAsDirty();

    expect(component.userForm.dirty).toBeTrue();
    expect(component.hasUnsavedChanges()).toBeTrue();
  });

  it('should report hasUnsavedChanges as false during submission', () => {
    component.userForm.controls['firstName'].setValue('Alice');
    component.userForm.controls['firstName'].markAsDirty();

    component.isSubmitting = true;
    expect(component.hasUnsavedChanges()).toBeFalse();
  });

  it('should revert changes and mark form as pristine on resetForm()', () => {
    component.userForm.controls['firstName'].setValue('Alice');
    component.userForm.controls['firstName'].markAsDirty();
    expect(component.hasUnsavedChanges()).toBeTrue();

    component.resetForm();

    expect(component.userForm.dirty).toBeFalse();
    expect(component.hasUnsavedChanges()).toBeFalse();
    expect(notificationServiceSpy.showInfo).toHaveBeenCalledWith('Form changes have been reverted.');
  });
});
