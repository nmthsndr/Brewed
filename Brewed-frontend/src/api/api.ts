import axiosInstance from "./axios.config";
import { IProduct } from "../interfaces/IProduct";
import { ICategory } from "../interfaces/ICategory";
import { ICart, ICartItem } from "../interfaces/ICart";
import { IOrder, IOrderItem } from "../interfaces/IOrder";
import { IUser } from "../interfaces/IUser";
import { IAddress } from "../interfaces/IAddress";
import { IReview } from "../interfaces/IReview";
import { ICoupon } from "../interfaces/ICoupon";

// DTOs
export interface UserRegisterDto {
  name: string;
  email: string;
  password: string;
}

export interface UserLoginDto {
  email: string;
  password: string;
}

export interface UserUpdateDto {
  name: string;
  email: string;
}

export interface ProductFilterDto {
  categoryId?: number;
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  roastLevel?: string;
  origin?: string;
  isCaffeineFree?: boolean;
  isOrganic?: boolean;
  sortBy?: string;
  page?: number;
  pageSize?: number;
}

export interface ProductCreateDto {
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
}

export interface AddToCartDto {
  productId: number;
  quantity: number;
}

export interface UpdateCartItemDto {
  quantity: number;
}

export interface AddressCreateDto {
  firstName: string;
  lastName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  postalCode: string;
  country: string;
  phoneNumber: string;
  isDefault: boolean;
  addressType: string;
}

export interface OrderCreateDto {
  shippingAddressId: number;
  billingAddressId?: number;
  paymentMethod: string;
  couponCode?: string;
  notes?: string;
}

export interface ReviewCreateDto {
  productId: number;
  rating: number;
  title: string;
  comment: string;
}

export interface CouponValidateDto {
  code: string;
  orderAmount: number;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

const Auth = {
  login: (email: string, password: string) =>
    axiosInstance.post<{ token: string }>(`/api/users/login`, { email, password }),
  register: (userData: UserRegisterDto) =>
    axiosInstance.post<IUser>(`/api/users/register`, userData),
  confirmEmail: (code: string) =>
    axiosInstance.post(`/api/users/confirm-email`, { code }),
  forgotPassword: (email: string) =>
    axiosInstance.post(`/api/users/forgot-password`, { email }),
  resetPassword: (code: string, newPassword: string) =>
    axiosInstance.post(`/api/users/reset-password`, { code, newPassword }),
  changePassword: (currentPassword: string, newPassword: string) =>
    axiosInstance.post(`/api/users/change-password`, { currentPassword, newPassword })
};

const Products = {
  getProducts: (filter?: ProductFilterDto) =>
    axiosInstance.get<PaginatedResult<IProduct>>(`/api/products`, { params: filter }),
  getProduct: (id: number) =>
    axiosInstance.get<IProduct>(`/api/products/${id}`),
  hasPurchasedProduct: (productId: number) =>
    axiosInstance.get<{ hasPurchased: boolean }>(`/api/products/${productId}/has-purchased`),
  createProduct: (productData: ProductCreateDto) =>
    axiosInstance.post<IProduct>(`/api/products`, productData),
  updateProduct: (id: number, productData: Partial<ProductCreateDto>) =>
    axiosInstance.put<IProduct>(`/api/products/${id}`, productData),
  deleteProduct: (id: number) =>
    axiosInstance.delete(`/api/products/${id}`)
};

const Categories = {
  getCategories: () =>
    axiosInstance.get<ICategory[]>(`/api/categories`),
  getCategory: (id: number) =>
    axiosInstance.get<ICategory>(`/api/categories/${id}`),
  createCategory: (categoryData: { name: string; description: string }) =>
    axiosInstance.post<ICategory>(`/api/categories`, categoryData),
  updateCategory: (id: number, categoryData: { name: string; description: string }) =>
    axiosInstance.put<ICategory>(`/api/categories/${id}`, categoryData),
  deleteCategory: (id: number) =>
    axiosInstance.delete(`/api/categories/${id}`)
};

const Cart = {
  getCart: (sessionId?: string) =>
    axiosInstance.get<ICart>(`/api/cart`, { params: { sessionId } }),
  addToCart: (data: AddToCartDto, sessionId?: string) =>
    axiosInstance.post<ICart>(`/api/cart/items`, data, { params: { sessionId } }),
  updateCartItem: (cartItemId: number, data: UpdateCartItemDto) =>
    axiosInstance.put<ICart>(`/api/cart/items/${cartItemId}`, data),
  removeFromCart: (cartItemId: number) =>
    axiosInstance.delete(`/api/cart/items/${cartItemId}`),
  clearCart: (sessionId?: string) =>
    axiosInstance.delete(`/api/cart`, { params: { sessionId } })
};

const Orders = {
  getOrders: () =>
    axiosInstance.get<IOrder[]>(`/api/orders`),
  getOrder: (id: number) =>
    axiosInstance.get<IOrder>(`/api/orders/${id}`),
  createOrder: (orderData: OrderCreateDto) =>
    axiosInstance.post<IOrder>(`/api/orders`, orderData),
  cancelOrder: (id: number) =>
    axiosInstance.post<IOrder>(`/api/orders/${id}/cancel`),
  getAllOrders: (status?: string, page?: number, pageSize?: number) =>
    axiosInstance.get<PaginatedResult<IOrder>>(`/api/orders/all`, {
      params: { status, page, pageSize }
    }),
  updateOrderStatus: (id: number, status: string) =>
    axiosInstance.put<IOrder>(`/api/orders/${id}/status`, { status }),
  getInvoice: (orderId: number) =>
    axiosInstance.get(`/api/orders/${orderId}/invoice`),
  generateInvoice: (orderId: number) =>
    axiosInstance.post(`/api/orders/${orderId}/invoice`)
};

const Addresses = {
  getAddresses: () =>
    axiosInstance.get<IAddress[]>(`/api/addresses`),
  getAddress: (id: number) =>
    axiosInstance.get<IAddress>(`/api/addresses/${id}`),
  createAddress: (addressData: AddressCreateDto) =>
    axiosInstance.post<IAddress>(`/api/addresses`, addressData),
  updateAddress: (id: number, addressData: AddressCreateDto) =>
    axiosInstance.put<IAddress>(`/api/addresses/${id}`, addressData),
  deleteAddress: (id: number) =>
    axiosInstance.delete(`/api/addresses/${id}`),
  setDefaultAddress: (id: number) =>
    axiosInstance.put<IAddress>(`/api/addresses/${id}/set-default`)
};

const Reviews = {
  getProductReviews: (productId: number, page?: number, pageSize?: number) =>
    axiosInstance.get<PaginatedResult<IReview>>(`/api/reviews/product/${productId}`, {
      params: { page, pageSize }
    }),
  getAllReviews: (page?: number, pageSize?: number) =>
    axiosInstance.get<PaginatedResult<IReview>>(`/api/reviews`, {
      params: { page, pageSize }
    }),
  getUserReviewForProduct: (productId: number) =>
    axiosInstance.get<{ hasReviewed: boolean; review: IReview | null }>(`/api/reviews/product/${productId}/user-review`),
  createReview: (reviewData: ReviewCreateDto) =>
    axiosInstance.post<IReview>(`/api/reviews`, reviewData),
  deleteReview: (id: number) =>
    axiosInstance.delete(`/api/reviews/${id}`)
};

const Coupons = {
  getCoupons: () =>
    axiosInstance.get<ICoupon[]>(`/api/coupons`),
  getCoupon: (id: number) =>
    axiosInstance.get<ICoupon>(`/api/coupons/${id}`),
  createCoupon: (couponData: any) =>
    axiosInstance.post<ICoupon>(`/api/coupons`, couponData),
  updateCoupon: (id: number, couponData: any) =>
    axiosInstance.put<ICoupon>(`/api/coupons/${id}`, couponData),
  deleteCoupon: (id: number) =>
    axiosInstance.delete(`/api/coupons/${id}`),
  validateCoupon: (data: CouponValidateDto) =>
    axiosInstance.post(`/api/coupons/validate`, data)
};

const Users = {
  getProfile: () =>
    axiosInstance.get<IUser>(`/api/users/profile`),
  updateProfile: (userData: UserUpdateDto) =>
    axiosInstance.put<IUser>(`/api/users/profile`, userData),
  deleteProfile: () =>
    axiosInstance.delete(`/api/users/profile`),
  // ADMIN ENDPOINTS
  getAllUsers: () =>
    axiosInstance.get<IUser[]>(`/api/users`),
  getUser: (userId: number) =>
    axiosInstance.get<IUser>(`/api/users/${userId}`),
  createUser: (userData: UserRegisterDto) =>
    axiosInstance.post<IUser>(`/api/users`, userData),
  updateUser: (userId: number, userData: UserUpdateDto) =>
    axiosInstance.put<IUser>(`/api/users/${userId}`, userData),
  deleteUser: (userId: number) =>
    axiosInstance.delete(`/api/users/${userId}`)
};

const Dashboard = {
  getStats: () =>
    axiosInstance.get(`/api/dashboard/stats`),
  getLowStockProducts: (threshold?: number) =>
    axiosInstance.get(`/api/dashboard/low-stock`, { params: { threshold } }),
  getTopCustomers: (count?: number) =>
    axiosInstance.get(`/api/dashboard/top-customers`, { params: { count } })
};

const Files = {
  uploadImage: (file: File, folder: string = 'products') => {
    const formData = new FormData();
    formData.append('file', file);
    return axiosInstance.post<{ url: string }>(`/api/files/upload?folder=${folder}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
  },
  uploadMultipleImages: (files: File[], folder: string = 'products') => {
    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file);
    });
    return axiosInstance.post<{ urls: string[] }>(`/api/files/upload-multiple?folder=${folder}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
  },
  deleteImage: (imageUrl: string) =>
    axiosInstance.delete(`/api/files?imageUrl=${encodeURIComponent(imageUrl)}`)
};

const api = {
  Auth,
  Products,
  Categories,
  Cart,
  Orders,
  Addresses,
  Reviews,
  Coupons,
  Users,
  Dashboard,
  Files
};

export default api;