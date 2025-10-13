import { createContext } from "react";
import { emailKeyName, roleKeyName, tokenKeyName, userIdKeyName } from "../constants/constants";

interface AuthContext {
  token: string | null;
  setToken: (token: string | null) => void;
  email: string | null;
  setEmail: (email: string | null) => void;
  role: string | null;
  setRole: (role: string | null) => void;
  userId: string | null;
  setUserId: (userId: string | null) => void;
}

export const AuthContext = createContext<AuthContext>({
  token: localStorage.getItem(tokenKeyName),
  setToken: () => {},
  email: localStorage.getItem(emailKeyName),
  setEmail: () => {},
  role: localStorage.getItem(roleKeyName),
  setRole: () => {},
  userId: localStorage.getItem(userIdKeyName),
  setUserId: () => {},
});