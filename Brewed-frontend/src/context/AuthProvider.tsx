import { useState, ReactNode } from "react";
import { AuthContext } from "./AuthContext";
import { tokenKeyName, emailKeyName, roleKeyName, userIdKeyName } from "../constants/constants";

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [token, setTokenState] = useState<string | null>(
    localStorage.getItem(tokenKeyName)
  );
  const [email, setEmailState] = useState<string | null>(
    localStorage.getItem(emailKeyName)
  );
  const [role, setRoleState] = useState<string | null>(
    localStorage.getItem(roleKeyName)
  );
  const [userId, setUserIdState] = useState<string | null>(
    localStorage.getItem(userIdKeyName)
  );

  const setToken = (newToken: string | null) => {
    setTokenState(newToken);
    if (newToken) {
      localStorage.setItem(tokenKeyName, newToken);
    } else {
      localStorage.removeItem(tokenKeyName);
    }
  };

  const setEmail = (newEmail: string | null) => {
    setEmailState(newEmail);
    if (newEmail) {
      localStorage.setItem(emailKeyName, newEmail);
    } else {
      localStorage.removeItem(emailKeyName);
    }
  };

  const setRole = (newRole: string | null) => {
    setRoleState(newRole);
    if (newRole) {
      localStorage.setItem(roleKeyName, newRole);
    } else {
      localStorage.removeItem(roleKeyName);
    }
  };

  const setUserId = (newUserId: string | null) => {
    setUserIdState(newUserId);
    if (newUserId) {
      localStorage.setItem(userIdKeyName, newUserId);
    } else {
      localStorage.removeItem(userIdKeyName);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        token,
        setToken,
        email,
        setEmail,
        role,
        setRole,
        userId,
        setUserId,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};