import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Bill } from '../../core/models/models';

@Component({
  selector: 'app-billing',
  templateUrl: './billing.component.html'
})
export class BillingComponent implements OnInit {
  bills: Bill[] = [];

  constructor(private api: ApiService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.getBills().subscribe(r => this.bills = r);
  }
}
