import React, { createContext, useContext, useEffect, useState } from 'react';
import API, { login as apiLogin } from '../services/api';

export interface User {
  id: string;
  name: string;
  email: string;
  role?: 'admin' | 'gestor' | 'operador';
  // adapte os campos conforme sua API
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<boolean>;
  register: (name: string, email: string, password: string) => Promise<boolean>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = (): AuthContextType => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(true);

  // Tenta restaurar a sessão ao iniciar (token + fetch /users/me quando possível)
  useEffect(() => {
    const init = async () => {
      const token = sessionStorage.getItem('token');
      const savedUser = sessionStorage.getItem('user');

      if (savedUser) {
        try {
          setUser(JSON.parse(savedUser));
        } catch {
          sessionStorage.removeItem('user');
        }
      }

      if (token) {
        API.defaults.headers.common['Authorization'] = `Bearer ${token}`;
      }

      setLoading(false);
    };

    init();
  }, []);

  const mapRole = (backendRole: string): 'admin' | 'gestor' | 'operador' => {
    switch (backendRole?.toLowerCase()) {
      case 'admin': return 'admin';
      case 'operator': return 'operador';
      case 'gestor': return 'gestor';
      default: return 'operador';
    }
  };

  const login = async (email: string, password: string): Promise<boolean> => {
    setLoading(true);
    try {
      const resp = await apiLogin({
        email,
        senha: password,
      } as any);

      const token = resp?.token ?? null;
      const userFromResp = resp?.user ?? null;

      if (token) {
        sessionStorage.setItem('token', token);
        API.defaults.headers.common['Authorization'] = `Bearer ${token}`;
      }
      if (userFromResp) {
        const mappedUser: User = {
          id: String(userFromResp.id ?? ''),
          name: userFromResp.name ?? '',
          email: userFromResp.email ?? '',
          role: mapRole(userFromResp.role ?? 'operador'),
        };
        sessionStorage.setItem('user', JSON.stringify(mappedUser));
        setUser(mappedUser);
      }

      return true;
    } catch (err) {
      console.error('Login error', err);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const register = async (name: string, email: string, password: string): Promise<boolean> => {
    setLoading(true);
    try {
      const body = { name, email, senha: password };
      await API.post('/api/Usuario/register', body);
      return true;
    } catch (err) {
      console.error('Register error', err);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    delete API.defaults.headers.common['Authorization'];
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};