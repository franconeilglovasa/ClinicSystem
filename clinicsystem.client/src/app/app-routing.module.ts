import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { AppShellComponent } from './shared/layout/app-shell.component';
import { AuthGuard, RoleGuard } from './core/guards/auth.guard';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { PatientListComponent } from './features/patients/patient-list.component';
import { PatientDetailComponent } from './features/patients/patient-detail.component';
import { VisitQueueComponent } from './features/visits/visit-queue.component';
import { VitalsFormComponent } from './features/vitals/vitals-form.component';
import { VisitDetailComponent } from './features/visits/visit-detail.component';
import { LabQueueComponent } from './features/laboratory/lab-queue.component';
import { BillingComponent } from './features/billing/billing.component';
import { BillDetailComponent } from './features/billing/bill-detail.component';
import { UsersAdminComponent } from './features/admin/users.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  {
    path: 'app',
    component: AppShellComponent,
    canActivate: [AuthGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'patients', component: PatientListComponent },
      { path: 'patients/:id', component: PatientDetailComponent },
      {
        path: 'queue',
        component: VisitQueueComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'Nurse', 'Doctor'] }
      },
      {
        path: 'vitals/:visitId',
        component: VitalsFormComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'Nurse'] }
      },
      {
        path: 'visits/:visitId',
        component: VisitDetailComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'Doctor'] }
      },
      {
        path: 'lab-queue',
        component: LabQueueComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'Laboratory'] }
      },
      {
        path: 'billing',
        component: BillingComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'Cashier'] }
      },
      {
        path: 'billing/:visitId',
        component: BillDetailComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'Cashier'] }
      },
      {
        path: 'admin/users',
        component: UsersAdminComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin'] }
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
