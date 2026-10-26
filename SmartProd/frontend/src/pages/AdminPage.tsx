import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, Controller } from "react-hook-form";
import { z } from "zod";
import { Edit, Plus, Shield, Trash2, Users } from "lucide-react";
import { toast } from "sonner";
import API from "../services/api";
import { useAuth } from "../contexts/AuthContext";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

type Role = "admin" | "operator" | "owner";
interface AdminUser {
  id: number;
  name: string;
  email: string;
  role: Role;
  active: boolean;
  createdAt: string;
}

const userSchema = z.object({
  name: z.string().trim().min(2, "Informe ao menos 2 caracteres.").max(100),
  email: z.string().trim().email("Informe um e-mail válido.").max(254),
  password: z.string().max(128).refine(value => value.length === 0 || value.length >= 8, "A senha deve ter ao menos 8 caracteres."),
  role: z.enum(["admin", "operator", "owner"]),
  active: z.boolean(),
});
type UserForm = z.infer<typeof userSchema>;

const roleLabel: Record<Role, string> = {
  admin: "Administrador",
  operator: "Operador",
  owner: "Gestor",
};

const normalizeRole = (role: string): Role => {
  const normalized = role.toLowerCase();
  if (normalized === "admin" || normalized === "owner" || normalized === "operator") return normalized;
  throw new Error(`Papel de usuário desconhecido: ${role}`);
};

const mapUser = (user: Omit<AdminUser, "role"> & { role: string }): AdminUser => ({
  ...user,
  role: normalizeRole(user.role),
});

export const AdminPage: React.FC = () => {
  const { user: currentUser } = useAuth();
  const queryClient = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);
  const usersQuery = useQuery({
    queryKey: ["adminUsers"],
    queryFn: async () => {
      const { data } = await API.get<{ users: Array<Omit<AdminUser, "role"> & { role: string }> }>("/api/Usuario");
      return data.users.map(mapUser);
    },
  });
  const form = useForm<UserForm>({
    resolver: zodResolver(userSchema),
    defaultValues: { name: "", email: "", password: "", role: "operator", active: true },
  });

  const saveUser = useMutation({
    mutationFn: async (values: UserForm) => {
      if (editingUser) {
        await API.put(`/api/Usuario/${editingUser.id}`, {
          name: values.name,
          email: values.email,
          role: values.role === "admin" ? "Admin" : values.role === "owner" ? "Owner" : "Operator",
          active: values.active,
        });
        return;
      }

      if (!values.password) throw new Error("A senha é obrigatória ao criar um usuário.");
      const { data } = await API.post<{ user: { id: number } }>("/api/Usuario/register", {
        name: values.name,
        email: values.email,
        senha: values.password,
      });
      await API.put(`/api/Usuario/${data.user.id}`, {
        name: values.name,
        email: values.email,
        role: values.role === "admin" ? "Admin" : values.role === "owner" ? "Owner" : "Operator",
        active: values.active,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["adminUsers"] });
      toast.success(editingUser ? "Usuário atualizado." : "Usuário criado.");
      setDialogOpen(false);
      setEditingUser(null);
      form.reset({ name: "", email: "", password: "", role: "operator", active: true });
    },
    onError: error => toast.error(error instanceof Error ? error.message : "Não foi possível salvar o usuário."),
  });
  const deleteUser = useMutation({
    mutationFn: async (id: number) => API.delete(`/api/Usuario/${id}`),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["adminUsers"] });
      toast.success("Usuário removido.");
    },
    onError: error => toast.error(error instanceof Error ? error.message : "Não foi possível remover o usuário."),
  });

  if (currentUser?.role !== "admin") {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-center">
          <Shield className="mx-auto mb-4 h-12 w-12 text-gray-400" />
          <h2 className="text-xl text-gray-600">Acesso Restrito</h2>
          <p className="text-gray-500">Apenas administradores podem acessar esta área.</p>
        </div>
      </div>
    );
  }

  const startCreate = () => {
    setEditingUser(null);
    form.reset({ name: "", email: "", password: "", role: "operator", active: true });
    setDialogOpen(true);
  };

  const startEdit = (userToEdit: AdminUser) => {
    setEditingUser(userToEdit);
    form.reset({
      name: userToEdit.name,
      email: userToEdit.email,
      password: "",
      role: userToEdit.role,
      active: userToEdit.active,
    });
    setDialogOpen(true);
  };

  const submit = (values: UserForm) => saveUser.mutate(values);

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-3xl">Administração</h1>
        <p className="text-gray-600">Gerenciamento de usuários conectado à API.</p>
      </header>
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="flex items-center gap-2"><Users className="h-5 w-5" />Usuários</CardTitle>
          <Button onClick={startCreate}><Plus className="mr-2 h-4 w-4" />Novo usuário</Button>
        </CardHeader>
        <CardContent>
          {usersQuery.isLoading ? <p>Carregando usuários...</p> : null}
          {usersQuery.isError ? <p role="alert">Falha ao carregar usuários: {usersQuery.error.message}</p> : null}
          {!usersQuery.isLoading && !usersQuery.isError && (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Nome</TableHead><TableHead>E-mail</TableHead><TableHead>Papel</TableHead>
                  <TableHead>Status</TableHead><TableHead>Criado em</TableHead><TableHead>Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {(usersQuery.data ?? []).map(item => (
                  <TableRow key={item.id}>
                    <TableCell>{item.name}</TableCell>
                    <TableCell>{item.email}</TableCell>
                    <TableCell><Badge variant={item.role === "admin" ? "destructive" : "secondary"}>{roleLabel[item.role]}</Badge></TableCell>
                    <TableCell><Badge variant={item.active ? "default" : "secondary"}>{item.active ? "Ativo" : "Inativo"}</Badge></TableCell>
                    <TableCell>{new Date(item.createdAt).toLocaleDateString("pt-BR")}</TableCell>
                    <TableCell>
                      <div className="flex gap-2">
                        <Button size="sm" variant="outline" aria-label={`Editar ${item.name}`} onClick={() => startEdit(item)}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button
                          size="sm"
                          variant="outline"
                          aria-label={`Remover ${item.name}`}
                          disabled={item.id === currentUser.id || deleteUser.isPending}
                          onClick={() => deleteUser.mutate(item.id)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editingUser ? "Editar usuário" : "Novo usuário"}</DialogTitle>
            <DialogDescription>Os dados são salvos na API SmartProd.</DialogDescription>
          </DialogHeader>
          <form className="space-y-4" onSubmit={form.handleSubmit(submit)}>
            <div className="space-y-2">
              <Label htmlFor="user-name">Nome</Label>
              <Input id="user-name" {...form.register("name")} />
              {form.formState.errors.name && <p role="alert">{form.formState.errors.name.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="user-email">E-mail</Label>
              <Input id="user-email" type="email" {...form.register("email")} />
              {form.formState.errors.email && <p role="alert">{form.formState.errors.email.message}</p>}
            </div>
            {!editingUser && (
              <div className="space-y-2">
                <Label htmlFor="user-password">Senha inicial</Label>
                <Input id="user-password" type="password" autoComplete="new-password" {...form.register("password")} />
                {form.formState.errors.password && <p role="alert">{form.formState.errors.password.message}</p>}
              </div>
            )}
            <div className="space-y-2">
              <Label>Papel</Label>
              <Controller control={form.control} name="role" render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="operator">Operador</SelectItem>
                    <SelectItem value="owner">Gestor</SelectItem>
                    <SelectItem value="admin">Administrador</SelectItem>
                  </SelectContent>
                </Select>
              )} />
            </div>
            <div className="flex items-center gap-2">
              <Controller control={form.control} name="active" render={({ field }) => (
                <Switch checked={field.value} onCheckedChange={field.onChange} />
              )} />
              <Label>Usuário ativo</Label>
            </div>
            <div className="flex justify-end gap-2">
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>Cancelar</Button>
              <Button type="submit" disabled={saveUser.isPending}>{saveUser.isPending ? "Salvando..." : "Salvar"}</Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
};
