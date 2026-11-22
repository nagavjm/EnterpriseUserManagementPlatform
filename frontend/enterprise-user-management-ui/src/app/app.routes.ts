import { Routes } from '@angular/router';
import { unsavedChangesGuard } from './core/guards/unsaved-changes.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
    title: 'Dashboard - Enterprise User Management'
  },
  {
    path: 'users',
    loadComponent: () => import('./features/users/user-list/user-list.component').then(m => m.UserListComponent),
    title: 'User Management - Enterprise User Management'
  },
  {
    path: 'users/new',
    loadComponent: () => import('./features/users/user-form/user-form.component').then(m => m.UserFormComponent),
    canDeactivate: [unsavedChangesGuard],
    title: 'Add New User - Enterprise User Management'
  },
  {
    path: 'users/:id/edit',
    loadComponent: () => import('./features/users/user-form/user-form.component').then(m => m.UserFormComponent),
    canDeactivate: [unsavedChangesGuard],
    title: 'Edit User - Enterprise User Management'
  },
  {
    path: 'users/:id',
    loadComponent: () => import('./features/users/user-details/user-details.component').then(m => m.UserDetailsComponent),
    title: 'User Details - Enterprise User Management'
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
