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
  maxUsageCount?: number;
  usageCount: number;
}

export interface IUserCoupon {
  id: number;
  userId: number;
  userName: string;
  userEmail: string;
  couponId: number;
  coupon: ICoupon;
  isUsed: boolean;
  assignedDate: string;
  usedDate?: string;
  orderId?: number;
}