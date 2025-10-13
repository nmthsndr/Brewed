export interface ICart {
  id: number;
  items: ICartItem[];
  subTotal: number;
  totalItems: number;
}

export interface ICartItem {
  id: number;
  productId: number;
  productName: string;
  productImageUrl: string;
  price: number;
  quantity: number;
  totalPrice: number;
  stockQuantity: number;
}