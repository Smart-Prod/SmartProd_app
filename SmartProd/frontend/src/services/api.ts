import axios, { type InternalAxiosRequestConfig } from "axios";
import type { LoginRequest, User } from "../types/auth";
import type { ApiResponse } from "../types/api";

const API = axios.create({
  baseURL: import.meta.env.VITE_API_URL || "",
  headers: { "Content-Type": "application/json" },
});

API.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem("token");
  if (config.url?.startsWith("/api/") && !config.url.startsWith("/api/v1/")) {
    config.url = config.url.replace(/^\/api\//, "/api/v1/");
  }
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export interface AuthResult {
  user: User;
  token: string;
  refreshToken: string;
}

interface ApiError {
  error?: string;
  message?: string;
}

const unwrapResponse = (value: unknown): unknown => {
  if (typeof value !== "object" || value === null || !("success" in value) || !("data" in value)) {
    return value;
  }
  const envelope = value as ApiResponse<unknown>;
  return envelope.data;
};

export async function login(payload: LoginRequest): Promise<AuthResult> {
  const { data } = await API.post<AuthResult>("/api/Usuario/login", payload);
  localStorage.setItem("token", data.token);
  localStorage.setItem("refreshToken", data.refreshToken);
  API.defaults.headers.common.Authorization = `Bearer ${data.token}`;
  return data;
}

export function logout(): void {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
  delete API.defaults.headers.common.Authorization;
}

API.interceptors.response.use(
  response => {
    response.data = unwrapResponse(response.data);
    return response;
  },
  (error: unknown) => {
    if (axios.isAxiosError<ApiResponse<unknown> | ApiError>(error) && error.response) {
      const responseBody = error.response.data;
      if (typeof responseBody === "object" && responseBody !== null && "success" in responseBody) {
        const envelope = responseBody as ApiResponse<unknown>;
        error.response.data = envelope.message ? { error: envelope.message } : { error: "Request failed." };
      }
    }
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      const requestUrl = String(error.config?.url ?? "");
      if (!requestUrl.includes("/Usuario/login") && !requestUrl.includes("/Usuario/register")) {
        logout();
        localStorage.removeItem("user");
        if (window.location.pathname !== "/login") window.location.assign("/login");
      }
    }
    return Promise.reject(error);
  },
);

export default API;
