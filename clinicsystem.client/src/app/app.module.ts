import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './features/auth/login/login.component';
import { AppShellComponent } from './shared/layout/app-shell.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { PatientListComponent } from './features/patients/patient-list.component';
import { PatientDetailComponent } from './features/patients/patient-detail.component';
import { VisitQueueComponent } from './features/visits/visit-queue.component';
import { VisitDetailComponent } from './features/visits/visit-detail.component';
import { VitalsFormComponent } from './features/vitals/vitals-form.component';
import { VitalsViewComponent } from './features/vitals/vitals-view.component';
import { HistoryFormComponent } from './features/medical-history/history-form.component';
import { PrescriptionsComponent } from './features/prescriptions/prescriptions.component';
import { LabQueueComponent } from './features/laboratory/lab-queue.component';
import { LabViewComponent } from './features/laboratory/lab-view.component';
import { BillingComponent } from './features/billing/billing.component';
import { BillDetailComponent } from './features/billing/bill-detail.component';
import { AiSuggestionsComponent } from './features/ai/ai-suggestions.component';
import { UsersAdminComponent } from './features/admin/users.component';
import { JwtInterceptor } from './core/interceptors/jwt.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    AppShellComponent,
    DashboardComponent,
    PatientListComponent,
    PatientDetailComponent,
    VisitQueueComponent,
    VisitDetailComponent,
    VitalsFormComponent,
    VitalsViewComponent,
    HistoryFormComponent,
    PrescriptionsComponent,
    LabQueueComponent,
    LabViewComponent,
    BillingComponent,
    BillDetailComponent,
    AiSuggestionsComponent,
    UsersAdminComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
