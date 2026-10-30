import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface AlertNotification {
  id: string;
  type: 'success' | 'danger' | 'warning' | 'info';
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<AlertNotification[]>([]);
  public notifications$: Observable<AlertNotification[]> = this.notificationsSubject.asObservable();

  showSuccess(message: string, durationMs: number = 4000): void {
    this.addNotification('success', message, durationMs);
  }

  showError(message: string, durationMs: number = 6000): void {
    this.addNotification('danger', message, durationMs);
  }

  showWarning(message: string, durationMs: number = 5000): void {
    this.addNotification('warning', message, durationMs);
  }

  showInfo(message: string, durationMs: number = 4000): void {
    this.addNotification('info', message, durationMs);
  }

  dismiss(id: string): void {
    const current = this.notificationsSubject.value.filter(n => n.id !== id);
    this.notificationsSubject.next(current);
  }

  private addNotification(type: AlertNotification['type'], message: string, durationMs: number): void {
    const id = Math.random().toString(36).substring(2, 9);
    const notification: AlertNotification = { id, type, message };
    const current = this.notificationsSubject.value;
    this.notificationsSubject.next([...current, notification]);

    if (durationMs > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, durationMs);
    }
  }
}

