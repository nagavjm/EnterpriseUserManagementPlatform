import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { Observable } from 'rxjs';
import { ConfirmationDialogService } from '../services/confirmation-dialog.service';

/**
 * Interface that components with forms or editable data should implement
 * to support unsaved changes detection.
 */
export interface ComponentCanDeactivate {
  /**
   * Optional custom deactivation handler returning boolean, Observable or Promise.
   */
  canDeactivate?: () => boolean | Observable<boolean> | Promise<boolean>;

  /**
   * Returns true if there are dirty/unsaved changes on the form.
   */
  hasUnsavedChanges?: () => boolean;
}

/**
 * Industry-standard CanDeactivate route guard that checks for unsaved changes
 * across all forms and routes. If pending changes exist, prompts the user with
 * a styled enterprise confirmation dialog before navigating away.
 */
export const unsavedChangesGuard: CanDeactivateFn<ComponentCanDeactivate> = (
  component: ComponentCanDeactivate
): boolean | Observable<boolean> | Promise<boolean> => {
  // 1. If component implements custom canDeactivate, delegate to it
  if (component && typeof component.canDeactivate === 'function') {
    return component.canDeactivate();
  }

  // 2. Check if component implements hasUnsavedChanges
  let hasChanges = false;
  if (component && typeof component.hasUnsavedChanges === 'function') {
    hasChanges = component.hasUnsavedChanges();
  } else if (component) {
    // Defensive check: automatically inspect reactive forms if present on component
    const formCandidate = (component as any).userForm || (component as any).form;
    const isSubmitting = Boolean((component as any).isSubmitting);
    if (formCandidate && typeof formCandidate.dirty === 'boolean') {
      hasChanges = formCandidate.dirty && !isSubmitting;
    }
  }

  // If no unsaved changes, allow immediate navigation
  if (!hasChanges) {
    return true;
  }

  // 3. Unsaved changes detected: prompt user via the confirmation dialog
  const dialogService = inject(ConfirmationDialogService);
  return dialogService.confirm({
    title: 'Unsaved Changes',
    message: 'You have unsaved changes that will be lost if you leave this page. Are you sure you want to discard your changes?',
    confirmButtonText: 'Discard Changes',
    cancelButtonText: 'Stay on Page',
    confirmButtonClass: 'btn-warning text-dark fw-semibold',
    iconClass: 'bi-exclamation-triangle-fill text-warning'
  });
};
