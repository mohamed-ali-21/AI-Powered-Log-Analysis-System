import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/dashboard/dashboard.page').then(m => m.DashboardPage),
    title: 'Dashboard · LogMind'
  },
  {
    path: 'logs',
    loadComponent: () => import('./features/logs/logs.page').then(m => m.LogsPage),
    title: 'Logs · LogMind'
  },
  {
    path: 'issues',
    loadComponent: () => import('./features/issues/issues.page').then(m => m.IssuesPage),
    title: 'Issues · LogMind'
  },
  {
    path: 'issues/:id',
    loadComponent: () => import('./features/issues/issue-detail.page').then(m => m.IssueDetailPage),
    title: 'Issue · LogMind'
  },
  {
    path: 'analysis',
    loadComponent: () => import('./features/analysis/analysis.page').then(m => m.AnalysisPage),
    title: 'AI Analysis · LogMind'
  },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings.page').then(m => m.SettingsPage),
    title: 'Settings · LogMind'
  },
  { path: '**', redirectTo: '' }
];
