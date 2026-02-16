import { useState, useEffect, ReactNode } from "react";
import CartContext from "./CartContext";
import api from "../api/api";
import useAuth from "../hooks/useAuth";
import { getGuestSessionId } from "../utils/guestSession";

interface CartProviderProps {
  children: ReactNode;
}

const CartProvider = ({ children }: CartProviderProps) => {
  const { isLoggedIn } = useAuth();
  const [cartItemCount, setCartItemCount] = useState(0);

  const refreshCartCount = async () => {
    try {
      const sessionId = isLoggedIn ? undefined : getGuestSessionId();
      const response = await api.Cart.getCart(sessionId);
      setCartItemCount(response.data.totalItems);
    } catch (error) {
      console.error("Failed to load cart count:", error);
      setCartItemCount(0);
    }
  };

  useEffect(() => {
    refreshCartCount();
    // Poll every 30 seconds as backup
    const interval = setInterval(refreshCartCount, 30000);
    return () => clearInterval(interval);
  }, [isLoggedIn]);

  useEffect(() => {
    const handleCartUpdated = () => refreshCartCount();
    window.addEventListener('cart-updated', handleCartUpdated);
    return () => window.removeEventListener('cart-updated', handleCartUpdated);
  }, []);

  return (
    <CartContext.Provider value={{ cartItemCount, refreshCartCount }}>
      {children}
    </CartContext.Provider>
  );
};

export default CartProvider;