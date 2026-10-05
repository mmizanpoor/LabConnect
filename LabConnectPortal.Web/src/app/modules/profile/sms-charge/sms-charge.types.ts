export interface InitPayResult {
  code?: string | null;
  key: string;
  redirectUrl?: string | null;
  isSuccess: boolean;
  message?: string | null;
  paymentGatewayType: PaymentGatewayType;
}

export enum PaymentGatewayType {
  Zarinpal = 1,
  BehPardakht = 2,
}
