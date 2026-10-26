export type LoginRequest = {
  email: string;
  senha: string;
};

export type User = {
  id: number;
  name: string;
  email: string;
  role: string;
};
