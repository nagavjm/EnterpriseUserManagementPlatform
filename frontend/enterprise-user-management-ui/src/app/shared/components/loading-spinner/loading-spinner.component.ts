import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="isLoading" class="d-flex flex-column align-items-center justify-content-center p-4">
      <div class="spinner-border text-primary" role="status" style="width: 2.5rem; height: 2.5rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p *ngIf="message" class="mt-2 text-muted small">{{ message }}</p>
    </div>
  `
})
export class LoadingSpinnerComponent {
  @Input() isLoading = false;
  @Input() message = 'Loading data...';
}

