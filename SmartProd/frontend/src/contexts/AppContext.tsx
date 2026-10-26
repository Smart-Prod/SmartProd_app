import React, { createContext, useContext, useEffect } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import API from "../services/api";
import type {
  BOM,
  Invoice,
  Movimentacao,
  MovementType,
  Product,
  ProductType,
  ProductionOrder,
  InvoiceType,
} from "../models";

export type { Product, BOM, Movimentacao, ProductionOrder, Invoice };

interface AppContextType {
  products: Product[];
  boms: BOM[];
  movimentacoes: Movimentacao[];
  productionOrders: ProductionOrder[];
  invoices: Invoice[];
  addProduct: (product: Pick<Product, "code" | "name" | "type" | "unit" | "currentStock" | "reservedStock" | "minStock">) => Promise<Product>;
  updateProduct: (id: number, product: Partial<Product>) => Promise<void>;
  addProductionOrder: (order: Pick<ProductionOrder, "productId" | "usuarioId" | "quantity">) => Promise<ProductionOrder>;
  updateProductionOrder: (id: number, order: Partial<ProductionOrder>) => Promise<void>;
  addMovimentacao: (movement: Omit<Movimentacao, "id" | "createdAt">) => Promise<Movimentacao>;
  addStockMovement: (movement: Omit<Movimentacao, "id" | "createdAt">) => Promise<Movimentacao>;
  addInvoice: (invoice: {
    type: InvoiceType;
    number: string;
    supplier?: string;
    customer?: string;
    items: Array<Pick<Invoice["items"][number], "productId" | "quantity" | "value">>;
  }) => Promise<Invoice>;
  addBOM: (bom: { productId: number; materials: Array<{ materialId: number; quantity: number }> }) => Promise<BOM>;
}

type ApiObject = Record<string, unknown>;

const asObject = (value: unknown): ApiObject => {
  if (typeof value !== "object" || value === null || Array.isArray(value)) {
    throw new Error("Resposta da API em formato inválido.");
  }
  return value as ApiObject;
};

const valueOf = (source: ApiObject, ...keys: string[]): unknown => {
  for (const key of keys) {
    if (key in source) return source[key];
  }
  return undefined;
};

const numberOf = (source: ApiObject, ...keys: string[]): number => {
  const value = valueOf(source, ...keys);
  if (typeof value !== "number" || !Number.isFinite(value)) {
    throw new Error(`A resposta da API não contém um valor numérico válido para ${keys[0]}.`);
  }
  return value;
};

const stringOf = (source: ApiObject, ...keys: string[]): string => {
  const value = valueOf(source, ...keys);
  if (typeof value !== "string") {
    throw new Error(`A resposta da API não contém um valor textual válido para ${keys[0]}.`);
  }
  return value;
};

const asArray = (data: unknown, ...keys: string[]): unknown[] => {
  if (Array.isArray(data)) return data;
  const source = asObject(data);
  for (const key of keys) {
    if (Array.isArray(source[key])) return source[key];
  }
  throw new Error("Resposta da API não contém a lista esperada.");
};

const mapProduct = (value: unknown): Product => {
  const source = asObject(value);
  const type = stringOf(source, "tipo", "type") as ProductType;
  return {
    id: numberOf(source, "id", "Id"),
    code: stringOf(source, "code", "Code"),
    name: stringOf(source, "name", "Name"),
    type,
    unit: stringOf(source, "unit", "Unit"),
    currentStock: numberOf(source, "estoqueAtual", "currentStock", "EstoqueAtual"),
    reservedStock: numberOf(source, "estoqueReservado", "reservedStock", "EstoqueReservado"),
    minStock: numberOf(source, "estoqueMínimo", "estoqueMinimo", "minStock", "EstoqueMínimo"),
    usuarioId: numberOf(source, "usuarioId", "UsuarioId"),
    createdAt: stringOf(source, "createdAt", "CreatedAt"),
    updatedAt: stringOf(source, "updatedAt", "UpdatedAt"),
  };
};

const mapMovement = (value: unknown): Movimentacao => {
  const source = asObject(value);
  const movementType = stringOf(source, "tipo", "type") as MovementType;
  const product = valueOf(source, "produto", "product");
  return {
    id: numberOf(source, "id", "Id"),
    productId: numberOf(source, "productId", "ProductId"),
    orderId: typeof valueOf(source, "orderId", "OrderId") === "number"
      ? numberOf(source, "orderId", "OrderId")
      : undefined,
    type: movementType,
    quantity: numberOf(source, "quantity", "Quantity"),
    createdAt: stringOf(source, "createdAt", "CreatedAt"),
    produto: product ? mapProduct(product) : undefined,
  };
};

const mapMovementsResponse = (data: unknown): Movimentacao[] =>
  asArray(data, "movements", "movimentacoes").map(mapMovement);

const mapBom = (value: unknown): BOM => {
  const source = asObject(value);
  const materials = asArray(valueOf(source, "materials", "Materials"), "items");
  return {
    id: numberOf(source, "id", "Id"),
    productId: numberOf(source, "productId", "produtoId", "ProdutoId"),
    product: valueOf(source, "produto", "product") ? mapProduct(valueOf(source, "produto", "product")) : undefined,
    materials: materials.map(itemValue => {
      const item = asObject(itemValue);
      const material = valueOf(item, "produtos", "material", "Produtos");
      return {
        id: numberOf(item, "id", "Id"),
        bomId: numberOf(item, "materiaisId", "MateriaisId", "bomId"),
        materialId: numberOf(item, "produtosId", "ProdutosId", "materialId"),
        quantity: numberOf(item, "quantidade", "Quantidade", "quantity"),
        material: material ? mapProduct(material) : undefined,
      };
    }),
    createdAt: stringOf(source, "createdAt", "CreatedAt"),
    updatedAt: stringOf(source, "updatedAt", "UpdatedAt"),
  };
};

const toServerProduct = (product: Partial<Product>): ApiObject => ({
  ...(product.code !== undefined ? { code: product.code } : {}),
  ...(product.name !== undefined ? { name: product.name } : {}),
  ...(product.type !== undefined ? { tipo: product.type } : {}),
  ...(product.unit !== undefined ? { unit: product.unit } : {}),
  ...(product.currentStock !== undefined ? { estoqueAtual: product.currentStock } : {}),
  ...(product.reservedStock !== undefined ? { estoqueReservado: product.reservedStock } : {}),
  ...(product.minStock !== undefined ? { "estoqueMínimo": product.minStock } : {}),
});

const AppContext = createContext<AppContextType | undefined>(undefined);

export const useApp = (): AppContextType => {
  const context = useContext(AppContext);
  if (!context) throw new Error("useApp must be used within an AppProvider");
  return context;
};

export const AppProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const queryClient = useQueryClient();
  const productsQuery = useQuery({
    queryKey: ["products"],
    queryFn: async () => (await API.get<unknown>("/api/Produto")).data,
    select: data => asArray(data, "products").map(mapProduct),
  });
  const bomsQuery = useQuery({
    queryKey: ["boms"],
    queryFn: async () => (await API.get<unknown>("/api/BOM")).data,
    select: data => asArray(data, "boms").map(mapBom),
  });
  const movementsQuery = useQuery({
    queryKey: ["movimentacoes"],
    queryFn: async () => (await API.get<unknown>("/api/Movimentacao")).data,
    select: mapMovementsResponse,
  });
  const ordersQuery = useQuery({
    queryKey: ["productionOrders"],
    queryFn: async () => (await API.get<unknown>("/api/ProductionOrder")).data,
    select: data => asArray(data, "orders").map(value => asObject(value) as unknown as ProductionOrder),
  });
  const invoicesQuery = useQuery({
    queryKey: ["invoices"],
    queryFn: async () => (await API.get<unknown>("/api/NotaFiscal")).data,
    select: data => asArray(data, "invoices").map(value => asObject(value) as unknown as Invoice),
  });

  useEffect(() => {
    for (const [name, error] of [
      ["products", productsQuery.error],
      ["boms", bomsQuery.error],
      ["movements", movementsQuery.error],
      ["production orders", ordersQuery.error],
      ["invoices", invoicesQuery.error],
    ] as const) {
      if (error) console.error(`Failed to load ${name}:`, error);
    }
  }, [productsQuery.error, bomsQuery.error, movementsQuery.error, ordersQuery.error, invoicesQuery.error]);

  const createProduct = useMutation({
    mutationFn: async (product: Pick<Product, "code" | "name" | "type" | "unit" | "currentStock" | "reservedStock" | "minStock">) =>
      mapProduct((await API.post<unknown>("/api/Produto", toServerProduct(product))).data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["products"] }),
  });
  const editProduct = useMutation({
    mutationFn: async ({ id, product }: { id: number; product: Partial<Product> }) => {
      const existing = productsQuery.data?.find(item => item.id === id);
      if (!existing) throw new Error("Produto não encontrado no estado carregado.");
      await API.put(`/api/Produto/${id}`, toServerProduct({ ...existing, ...product }));
    },
    onSuccess: () => Promise.all([
      queryClient.invalidateQueries({ queryKey: ["products"] }),
      queryClient.invalidateQueries({ queryKey: ["movimentacoes"] }),
    ]),
  });
  const createOrder = useMutation({
    mutationFn: async (order: Pick<ProductionOrder, "productId" | "usuarioId" | "quantity">) =>
      (await API.post<ProductionOrder>("/api/ProductionOrder", {
        productId: order.productId,
        usuarioId: order.usuarioId,
        quantity: order.quantity,
      })).data,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["productionOrders"] }),
  });
  const editOrder = useMutation({
    mutationFn: async ({ id, order }: { id: number; order: Partial<ProductionOrder> }): Promise<void> => {
      await API.put(`/api/ProductionOrder/${id}`, { status: order.status, produced: order.produced });
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["productionOrders"] }),
  });
  const createMovement = useMutation({
    mutationFn: async (movement: Omit<Movimentacao, "id" | "createdAt">) =>
      mapMovement((await API.post<unknown>("/api/Movimentacao", {
        productId: movement.productId,
        tipo: movement.type,
        quantity: movement.quantity,
        orderId: movement.orderId,
      })).data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["movimentacoes"] }),
  });
  const createInvoice = useMutation({
    mutationFn: async (invoice: {
      type: InvoiceType;
      number: string;
      supplier?: string;
      customer?: string;
      items: Array<Pick<Invoice["items"][number], "productId" | "quantity" | "value">>;
    }) =>
      (await API.post<Invoice>("/api/NotaFiscal", {
        type: invoice.type,
        number: invoice.number,
        date: new Date().toISOString(),
        supplier: invoice.supplier,
        customer: invoice.customer,
        items: invoice.items.map(item => ({ productId: item.productId, quantity: item.quantity, value: item.value })),
      })).data,
    onSuccess: () => Promise.all([
      queryClient.invalidateQueries({ queryKey: ["invoices"] }),
      queryClient.invalidateQueries({ queryKey: ["products"] }),
      queryClient.invalidateQueries({ queryKey: ["movimentacoes"] }),
    ]),
  });
  const createBom = useMutation({
    mutationFn: async (bom: { productId: number; materials: Array<{ materialId: number; quantity: number }> }) =>
      (await API.post<BOM>("/api/BOM", {
        productId: bom.productId,
        materials: bom.materials.map(item => ({ materialId: item.materialId, quantidade: item.quantity })),
      })).data,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["boms"] }),
  });

  const addProduct = (product: Pick<Product, "code" | "name" | "type" | "unit" | "currentStock" | "reservedStock" | "minStock">) =>
    createProduct.mutateAsync(product);
  const updateProduct = async (id: number, product: Partial<Product>) => editProduct.mutateAsync({ id, product });
  const addProductionOrder = (order: Pick<ProductionOrder, "productId" | "usuarioId" | "quantity">) =>
    createOrder.mutateAsync(order);
  const updateProductionOrder = async (id: number, order: Partial<ProductionOrder>) => editOrder.mutateAsync({ id, order });
  const addStockMovement = (movement: Omit<Movimentacao, "id" | "createdAt">) => createMovement.mutateAsync(movement);
  const addInvoice = (invoice: Parameters<AppContextType["addInvoice"]>[0]) => createInvoice.mutateAsync(invoice);
  const addBOM = (bom: Parameters<AppContextType["addBOM"]>[0]) => createBom.mutateAsync(bom);

  const contextValue: AppContextType = {
    products: productsQuery.data ?? [],
    boms: bomsQuery.data ?? [],
    movimentacoes: movementsQuery.data ?? [],
    productionOrders: ordersQuery.data ?? [],
    invoices: invoicesQuery.data ?? [],
    addProduct,
    updateProduct,
    addProductionOrder,
    updateProductionOrder,
    addMovimentacao: addStockMovement,
    addStockMovement,
    addInvoice,
    addBOM,
  };

  return <AppContext.Provider value={contextValue}>{children}</AppContext.Provider>;
};
