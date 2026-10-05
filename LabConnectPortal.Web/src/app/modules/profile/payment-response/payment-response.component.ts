import { NgClass, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { VerifyResult } from './payment-response.types';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';

@Component({
  selector: 'app-payment-response',
  standalone: true,
  imports: [MatIconModule, NgClass, NgIf, BaseButtonComponent, RouterLink],
  templateUrl: './payment-response.component.html',
  styleUrl: './payment-response.component.scss',
})
export class PaymentResponseComponent implements OnInit {
  constructor(private activatedRoute: ActivatedRoute) {}
  verifyResult: VerifyResult | null = null;

  ngOnInit(): void {
    this.activatedRoute.queryParamMap.subscribe((params) => {
      console.log(params);
      this.verifyResult = {
        isSuccess: params.get('isSuccess') === 'true',
        refId: params.get('refId') || '',
        message: params.get('message') || '',
      };
    });
  }
}
