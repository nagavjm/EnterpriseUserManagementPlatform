import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { unsavedChangesGuard, ComponentCanDeactivate } from './unsaved-changes.guard';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

describe('unsavedChangesGuard', () => {
  let confirmationDialogServiceSpy: jasmine.SpyObj<ConfirmationDialogService>;
  const mockRoute = {} as ActivatedRouteSnapshot;
  const mockState = {} as RouterStateSnapshot;

  beforeEach(() => {
    confirmationDialogServiceSpy = jasmine.createSpyObj('ConfirmationDialogService', ['confirm']);

    TestBed.configureTestingModule({
      providers: [
        { provide: ConfirmationDialogService, useValue: confirmationDialogServiceSpy }
      ]
    });
  });

  it('should allow navigation if component has no unsaved changes', () => {
    const component: ComponentCanDeactivate = {
      hasUnsavedChanges: () => false
    };

    const result = TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, mockRoute, mockState, mockState)
    );

    expect(result).toBeTrue();
    expect(confirmationDialogServiceSpy.confirm).not.toHaveBeenCalled();
  });

  it('should prompt confirmation dialog if component has unsaved changes', (done) => {
    confirmationDialogServiceSpy.confirm.and.returnValue(of(true));

    const component: ComponentCanDeactivate = {
      hasUnsavedChanges: () => true
    };

    const result$ = TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, mockRoute, mockState, mockState)
    );

    expect(confirmationDialogServiceSpy.confirm).toHaveBeenCalledWith(
      jasmine.objectContaining({
        title: 'Unsaved Changes'
      })
    );

    if (typeof result$ === 'object' && 'subscribe' in result$) {
      result$.subscribe(allowed => {
        expect(allowed).toBeTrue();
        done();
      });
    }
  });

  it('should delegate to canDeactivate method if component implements it', () => {
    const component: ComponentCanDeactivate = {
      canDeactivate: () => false
    };

    const result = TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, mockRoute, mockState, mockState)
    );

    expect(result).toBeFalse();
  });

  it('should detect dirty form when component does not implement hasUnsavedChanges', () => {
    confirmationDialogServiceSpy.confirm.and.returnValue(of(false));

    const component = {
      userForm: { dirty: true },
      isSubmitting: false
    } as any;

    TestBed.runInInjectionContext(() =>
      unsavedChangesGuard(component, mockRoute, mockState, mockState)
    );

    expect(confirmationDialogServiceSpy.confirm).toHaveBeenCalled();
  });
});
