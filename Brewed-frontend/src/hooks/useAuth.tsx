import { useContext } from "react";
import { AuthContext } from "../context/AuthContext";
import { emailKeyName, roleKeyName, tokenKeyName, userIdKeyName, roleTokenKey, emailTokenKey, userIdTokenKey } from "../constants/constants";
import api from "../api/api";
import { jwtDecode } from "jwt-decode";
import { clearGuestSession, getGuestSessionId, hasGuestSession } from "../utils/guestSession";

const useAuth = () => {
  const { token, setToken, email, setEmail, role, setRole, userId, setUserId } = useContext(AuthContext);
  const isLoggedIn = !!token;

  const login = async (email: string, password: string) => {
    try {
      const res = await api.Auth.login(email, password);
      const token = res.data.token;
      
      console.log("Login successful, token received:", token);
      
      // Token mentése
      setToken(token);
      localStorage.setItem(tokenKeyName, token);
      
      // Token dekódolása
      const decodedToken: any = jwtDecode(token);
      console.log("Decoded token:", decodedToken);
      
      // Role
      const userRole = decodedToken[roleTokenKey];
      console.log("User role:", userRole);
      setRole(userRole);
      localStorage.setItem(roleKeyName, userRole);
      
      // Email
      const userEmail = decodedToken[emailTokenKey];
      console.log("User email:", userEmail);
      setEmail(userEmail);
      localStorage.setItem(emailKeyName, userEmail);
      
      // UserId
      const id = decodedToken[userIdTokenKey];
      console.log("User ID:", id);
      setUserId(id);
      localStorage.setItem(userIdKeyName, id);
      
      console.log("All data saved to localStorage");

      // Merge guest cart into user cart before clearing guest session
      if (hasGuestSession()) {
        try {
          const guestSessionId = getGuestSessionId();
          await api.Cart.mergeGuestCart(guestSessionId);
          console.log("Guest cart merged into user cart");
        } catch (mergeError) {
          console.error("Failed to merge guest cart:", mergeError);
        }
      }

      // Clear guest session after successful login
      clearGuestSession();
      console.log("Guest session cleared after login");

      return true;
    } catch (error) {
      console.error("Login failed:", error);
      return false;
    }
  };

  const logout = () => {
    console.log("Logging out, clearing localStorage");
    localStorage.clear();
    setToken(null);
    setEmail(null);
    setRole(null);
    setUserId(null);
  };

  return { 
    login, 
    logout, 
    token, 
    email: email || 'User', 
    isLoggedIn, 
    role, 
    userId 
  };
};

export default useAuth;