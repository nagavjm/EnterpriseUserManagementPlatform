import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmationDialogService } from '../../../core/services/confirmation-dialog.service';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <ng-container *ngIf="dialogService.dialogState$ | async as state">
      <div *ngIf="state.isOpen" class="modal-backdrop fade show" (click)="onBackdropClick()"></div>
      <div 
        *ngIf="state.isOpen" 
        class="modal d-block fade show" 
        tabindex="-1" 
        role="dialog" 
        aria-modal="true"
        aria-labelledby="confirmDialogTitle"
        style="background: rgba(0, 0, 0, 0.5);">
        <div class="modal-dialog modal-dialog-centered" role="document" (click)="$event.stopPropagation()">
          <div class="modal-content border-0 shadow">
            <div class="modal-header border-bottom-0 pb-0">
              <h5 class="modal-title d-flex align-items-center text-body" id="confirmDialogTitle">
                <i class="bi me-2 fs-4" [ngClass]="state.options.iconClass"></i>
                <span class="fw-semibold">{{ state.options.title }}</span>
              </h5>
              <button 
                type="button" 
                class="btn-close" 
                aria-label="Close" 
                (click)="dialogService.cancelAction()">
              </button>
            </div>
            <div class="modal-body py-3">
              <p class="text-body-secondary mb-0 fs-6">{{ state.options.message }}</p>
            </div>
            <div class="modal-footer border-top-0 pt-0 gap-2">
              <button 
                type="button" 
                class="btn btn-secondary px-3" 
                (click)="dialogService.cancelAction()">
                {{ state.options.cancelButtonText }}
              </button>
              <button 
                type="button" 
                class="btn px-3" 
                [ngClass]="state.options.confirmButtonClass" 
                (click)="dialogService.confirmAction()">
                {{ state.options.confirmButtonText }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </ng-container>
  `
})
export class ConfirmationDialogComponent {
  public dialogService = inject(ConfirmationDialogService);

  @HostListener('window:keydown.escape')
  onEscape(): void {
    this.dialogService.cancelAction();
  }

  onBackdropClick(): void {
    this.dialogService.cancelAction();
  }
}
