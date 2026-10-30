import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-alert-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 1080;">
      <div 
        *ngFor="let notification of notificationService.notifications$ | async"
        class="toast show align-items-center text-white border-0 mb-2 shadow"
        [ngClass]="'bg-' + notification.type"
        role="alert" 
        aria-live="assertive" 
        aria-atomic="true">
        <div class="d-flex">
          <div class="toast-body">
            <i class="bi me-2" [ngClass]="getIcon(notification.type)"></i>
            {{ notification.message }}
          </div>
          <button 
            type="button" 
            class="btn-close btn-close-white me-2 m-auto" 
            aria-label="Close"
            (click)="notificationService.dismiss(notification.id)">
          </button>
        </div>
      </div>
    </div>
  `
})
export class AlertToastComponent {
  public notificationService = inject(NotificationService);

  getIcon(type: string): string {
    switch (type) {
      case 'success': return 'bi-check-circle-fill';
      case 'danger': return 'bi-exclamation-triangle-fill';
      case 'warning': return 'bi-exclamation-circle-fill';
      default: return 'bi-info-circle-fill';
    }
  }
}

