export interface ICoupon {
  id: number;
  code: string;
  description: string;
  discountType: string;
  discountValue: number;
  minimumOrderAmount?: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
}