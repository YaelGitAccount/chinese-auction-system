import { createContext, useContext, useState } from "react";
import { jwtDecode } from "jwt-decode";

const AuthContext = createContext();
export const useAuth = () => useContext(AuthContext);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem("token"));
  let user = null;
  
  try {
    if (token) {
      user = jwtDecode(token);
    }
  } catch (error) {
    console.log("Invalid token");
    localStorage.removeItem("token");
  }
  
  const login = (t) => { setToken(t); localStorage.setItem("token", t); };
  const logout = () => { setToken(null); localStorage.removeItem("token"); };
  
  // Helper function to get user role from JWT claims
  const getUserRole = () => {
    if (!user) return null;
    // Try different possible claim names for role
    return user.role || 
           user["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
           user["role"] ||
           null;
  };
  
  // Helper function to check if user is manager
  const isManager = () => {
    const role = getUserRole();
    return role === "manager" || role === "Manager";
  };
  
  return (
    <AuthContext.Provider value={{ token, user, login, logout, getUserRole, isManager }}>
      {children}
    </AuthContext.Provider>
  );
}
