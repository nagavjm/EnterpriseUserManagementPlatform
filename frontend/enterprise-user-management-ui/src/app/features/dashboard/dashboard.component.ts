import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardSummary } from '../../shared/models/dashboard.model';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, LoadingSpinnerComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  summary: DashboardSummary | null = null;
  isLoading = true;
  error: string | null = null;

  ngOnInit(): void {
    this.loadSummary();
  }

  loadSummary(): void {
    this.isLoading = true;
    this.error = null;

    this.dashboardService.getSummary().subscribe({
      next: (response) => {
        if (response.success) {
          this.summary = response.data;
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load dashboard metrics from backend.';
        this.isLoading = false;
      }
    });
  }

  getDepartmentEntries(): [string, number][] {
    return this.summary ? Object.entries(this.summary.usersByDepartment) : [];
  }

  getRoleEntries(): [string, number][] {
    return this.summary ? Object.entries(this.summary.usersByRole) : [];
  }
}

