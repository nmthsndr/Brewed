import { IAddress } from "./IAddress";

export interface IOrder {
  id: number;
  orderNumber: string;
  subTotal: number;
  shippingCost: number;
  discount: number;
  totalAmount: number;
  couponCode?: string;
  orderDate: string;
  status: string;
  paymentMethod: string;
  paymentStatus: string;
  notes?: string;
  shippedAt?: string;
  deliveredAt?: string;
  shippingAddress: IAddress;
  billingAddress?: IAddress;
  items: IOrderItem[];
  invoice?: IInvoice;
  user?: {
    id: number;
    name: string;
    email: string;
  };
  isGuestOrder: boolean;
  guestOrderDetails?: {
    id: number;
    orderId: number;
    email: string;
    firstName: string;
    lastName: string;
    shippingAddressLine1: string;
    shippingAddressLine2?: string;
    shippingCity: string;
    shippingPostalCode: string;
    shippingCountry: string;
    shippingPhoneNumber: string;
    billingAddressLine1: string;
    billingAddressLine2?: string;
    billingCity: string;
    billingPostalCode: string;
    billingCountry: string;
    billingPhoneNumber: string;
  };
}

export interface IOrderItem {
  id: number;
  productId: number;
  productName: string;
  productImageUrl: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface IInvoice {
  id: number;
  invoiceNumber: string;
  issueDate: string;
  totalAmount: number;
  pdfUrl: string;
}