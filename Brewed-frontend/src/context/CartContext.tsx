import { createContext } from "react";

export interface CartContextType {
  cartItemCount: number;
  refreshCartCount: () => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export default CartContext;