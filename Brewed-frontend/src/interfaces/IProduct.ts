import { ICategory } from "./ICategory";
import { IProductImage } from "./IProductImage";

export interface IProduct {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  roastLevel: string;
  origin: string;
  isCaffeineFree: boolean;
  isOrganic: boolean;
  imageUrl: string;
  categoryId: number;
  categoryName: string;
  averageRating: number;
  reviewCount: number;
  productImages?: IProductImage[];
}

export interface IProductImage {
  id: number;
  imageUrl: string;
  displayOrder: number;
}