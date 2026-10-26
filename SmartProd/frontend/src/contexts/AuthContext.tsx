import React, { createContext, useContext, useEffect, useState } from "react";
import axios from "axios";
import API, { login as apiLogin, logout as apiLogout } from "../services/api";
import type { User } from "../types/auth";

const getRegistrationErrorMessage = (error: unknown): string => {
  if (!axios.isAxiosError<Record<string, unknown>>(error)) {
    return error instanceof Error ? error.message : "Erro ao registrar. Tente novamente.";
  }

  const responseData = error.response?.data;
  if (responseData) {
    for (const key of ["error", "message", "title"]) {
      const message = responseData[key];
      if (typeof message === "string" && message.trim()) return message;
    }

    const validationErrors = responseData.errors;
    if (typeof validationErrors === "object" && validationErrors !== null) {
      const firstError = Object.values(validationErrors).flatMap(value =>
        Array.isArray(value) ? value : [],
      ).find(value => typeof value === "string" && value.trim());
      if (typeof firstError === "string") return firstError;
    }
  }

  return error.response
    ? "Não foi possível completar o cadastro. Verifique os dados informados."
    : "Não foi possível conectar ao servidor. Verifique se a aplicação está disponível.";
};

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  register: (name: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within an AuthProvider");
  return context;
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem("token");
    const savedUser = localStorage.getItem("user");
    if (savedUser) {
      try {
        setUser(JSON.parse(savedUser) as User);
      } catch {
        localStorage.removeItem("user");
      }
    }
    if (token) API.defaults.headers.common.Authorization = `Bearer ${token}`;
    setLoading(false);
  }, []);

  const login = async (email: string, password: string): Promise<boolean> => {
    setLoading(true);
    try {
      const response = await apiLogin({ email, senha: password });
      const authenticatedUser = { ...response.user, role: response.user.role.toLowerCase() };
      localStorage.setItem("user", JSON.stringify(authenticatedUser));
      setUser(authenticatedUser);
      return true;
    } catch (error) {
      console.error("Login error", error);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const register = async (name: string, email: string, password: string): Promise<void> => {
    setLoading(true);
    try {
      await API.post("/api/Usuario/register", { name, email, senha: password });
    } catch (error) {
      console.error("Register error", error);
      throw new Error(getRegistrationErrorMessage(error));
    } finally {
      setLoading(false);
    }
  };

  const logout = (): void => {
    apiLogout();
    localStorage.removeItem("user");
    setUser(null);
  };

  return <AuthContext.Provider value={{ user, loading, login, register, logout }}>{children}</AuthContext.Provider>;
};
