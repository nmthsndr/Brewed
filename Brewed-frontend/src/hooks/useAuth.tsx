import { useContext } from "react";
import { AuthContext } from "../context/AuthContext";
import { emailKeyName, roleKeyName, tokenKeyName, userIdKeyName, roleTokenKey, emailTokenKey, userIdTokenKey } from "../constants/constants";
import api from "../api/api";
import { jwtDecode } from "jwt-decode";

const useAuth = () => {
  const { token, setToken, email, setEmail, role, setRole, userId, setUserId } = useContext(AuthContext);
  const isLoggedIn = !!token;

  const login = async (email: string, password: string) => {
    try {
      const res = await api.Auth.login(email, password);
      const token = res.data.token;
      
      setToken(token);
      localStorage.setItem(tokenKeyName, token);
      
      const decodedToken: any = jwtDecode(token);
      
      const userRole = decodedToken[roleTokenKey];
      const userEmail = decodedToken[emailTokenKey];
      const id = decodedToken[userIdTokenKey];
      
      setRole(userRole);
      localStorage.setItem(roleKeyName, userRole);
      
      setEmail(userEmail);
      localStorage.setItem(emailKeyName, userEmail);
      
      setUserId(id);
      localStorage.setItem(userIdKeyName, id);
      
      return true;
    } catch (error) {
      console.error("Login failed:", error);
      return false;
    }
  };

  const logout = () => {
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