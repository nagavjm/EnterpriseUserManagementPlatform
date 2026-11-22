import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

export interface ConfirmDialogOptions {
  title?: string;
  message: string;
  confirmButtonText?: string;
  cancelButtonText?: string;
  confirmButtonClass?: string;
  iconClass?: string;
}

export interface ConfirmDialogState {
  isOpen: boolean;
  options: ConfirmDialogOptions;
}

@Injectable({
  providedIn: 'root'
})
export class ConfirmationDialogService {
  private dialogStateSubject = new BehaviorSubject<ConfirmDialogState>({
    isOpen: false,
    options: { message: '' }
  });
  public dialogState$: Observable<ConfirmDialogState> = this.dialogStateSubject.asObservable();

  private resultSubject: Subject<boolean> | null = null;

  confirm(options: ConfirmDialogOptions): Observable<boolean> {
    // If a dialog is already open, resolve previous one as false
    if (this.resultSubject) {
      this.resultSubject.next(false);
      this.resultSubject.complete();
    }

    this.resultSubject = new Subject<boolean>();

    this.dialogStateSubject.next({
      isOpen: true,
      options: {
        title: options.title ?? 'Confirm Action',
        message: options.message,
        confirmButtonText: options.confirmButtonText ?? 'Confirm',
        cancelButtonText: options.cancelButtonText ?? 'Cancel',
        confirmButtonClass: options.confirmButtonClass ?? 'btn-warning',
        iconClass: options.iconClass ?? 'bi-exclamation-triangle-fill text-warning'
      }
    });

    return this.resultSubject.asObservable();
  }

  confirmAction(): void {
    if (this.resultSubject) {
      this.resultSubject.next(true);
      this.resultSubject.complete();
      this.resultSubject = null;
    }
    this.close();
  }

  cancelAction(): void {
    if (this.resultSubject) {
      this.resultSubject.next(false);
      this.resultSubject.complete();
      this.resultSubject = null;
    }
    this.close();
  }

  private close(): void {
    this.dialogStateSubject.next({
      isOpen: false,
      options: { message: '' }
    });
  }
}
