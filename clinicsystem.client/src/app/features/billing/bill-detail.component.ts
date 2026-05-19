import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { Bill } from '../../core/models/models';

@Component({
  selector: 'app-bill-detail',
  templateUrl: './bill-detail.component.html'
})
export class BillDetailComponent implements OnInit {
  visitId!: string;
  patientId = '';
  bill?: Bill;
  loading = true;
  showItemForm = false;
  showPaymentForm = false;
  today = new Date();

  itemForm = this.fb.group({
    description: ['', Validators.required],
    category: ['Other', Validators.required],
    unitPrice: [0, Validators.required],
    quantity: [1, Validators.required]
  });

  paymentForm = this.fb.group({
    amount: [0, Validators.required],
    notes: ['']
  });

  constructor(private route: ActivatedRoute, private api: ApiService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.visitId = this.route.snapshot.paramMap.get('visitId')!;
    this.api.getVisit(this.visitId).subscribe(v => {
      this.patientId = v.patientId;
      this.loadBillOrCreate();
    });
  }

  loadBillOrCreate(): void {
    this.loading = true;
    this.api.getBillByVisit(this.visitId).subscribe({
      next: b => { this.bill = b; this.loading = false; },
      error: () => {
        this.api.createBill(this.visitId, { patientId: this.patientId, notes: '' }).subscribe({
          next: b => { this.bill = b; this.loading = false; },
          error: () => this.loading = false
        });
      }
    });
  }

  addItem(): void {
    if (!this.bill || this.itemForm.invalid) return;
    this.api.addBillItem(this.bill.billId, this.itemForm.value as any).subscribe(() => {
      this.itemForm.reset({ description: '', category: 'Other', unitPrice: 0, quantity: 1 });
      this.showItemForm = false;
      this.loadBillOrCreate();
    });
  }

  deleteItem(itemId: string): void {
    if (!this.bill) return;
    this.api.deleteBillItem(this.bill.billId, itemId).subscribe(() => this.loadBillOrCreate());
  }

  recordPayment(): void {
    if (!this.bill || this.paymentForm.invalid) return;
    this.api.recordBillPayment(this.bill.billId, this.paymentForm.value as any).subscribe(() => {
      this.paymentForm.reset({ amount: 0, notes: '' });
      this.showPaymentForm = false;
      this.loadBillOrCreate();
    });
  }

  printReceipt(): void { setTimeout(() => window.print(), 100); }
}
