export interface IReview {
  id: number;
  rating: number;
  title: string;
  comment: string;
  createdAt: string;
  userName: string;
  userId: number;
  productId?: number;
  productName?: string;
}