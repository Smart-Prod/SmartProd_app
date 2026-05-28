import React, { createContext, useContext, useState, useEffect, useCallback, useMemo } from 'react';
import API from '../services/api';
import type { Product, BOM, Movimentacao, ProductionOrder, Invoice, ProductType, MovementType } from '../models/index';

export type { Product, BOM, Movimentacao, ProductionOrder, Invoice };

interface AppContextType {
  products: Product[];
  boms: BOM[];
  stockMovements: Movimentacao[];
  productionOrders: ProductionOrder[];
  invoices: Invoice[];
  addProduct: (product: Omit<Product, 'id'>) => Promise<Product>;
  updateProduct: (id: number, product: Partial<Product>) => Promise<void>;
  addProductionOrder: (order: Omit<ProductionOrder, 'id' | 'createdAt' | 'finishedAt' | 'produced'>) => Promise<ProductionOrder>;
  updateProductionOrder: (id: number, order: Partial<ProductionOrder>) => Promise<void>;
  addStockMovement: (movement: Omit<Movimentacao, 'id' | 'createdAt'>) => Promise<Movimentacao>;
  addInvoice: (invoice: Omit<Invoice, 'id' | 'date'>) => Promise<Invoice>;
  addBOM: (bom: Omit<BOM, 'id'>) => Promise<BOM>;
  refreshAll: () => Promise<void>;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

export const useApp = () => {
  const context = useContext(AppContext);
  if (!context) throw new Error('useApp must be used within an AppProvider');
  return context;
};

export const AppProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [products, setProducts] = useState<Product[]>([]);
  const [boms, setBoms] = useState<BOM[]>([]);
  const [movimentacoes, setMovimentacoes] = useState<Movimentacao[]>([]);
  const [productionOrders, setProductionOrders] = useState<ProductionOrder[]>([]);
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  const mapProduct = (s: Record<string, unknown>): Product => ({
    id: s.id as number,
    code: s.code as string,
    name: s.name as string,
    type: (s.tipo ?? s.type) as ProductType,
    unit: s.unit as string,
    currentStock: (s.estoqueAtual ?? s.currentStock) as number,
    reservedStock: (s.estoqueReservado ?? s.reservedStock) as number,
    minStock: (s.estoqueMínimo ?? s.minStock) as number,
    usuarioId: s.usuarioId as number,
    createdAt: s.createdAt as string,
    updatedAt: s.updatedAt as string,
  });

  const mapMovimentacao = (s: Record<string, unknown>): Movimentacao => ({
    id: s.id as number,
    productId: s.productId as number,
    orderId: s.orderId as number | undefined,
    type: (s.tipo ?? s.type) as MovementType,
    quantity: s.quantity as number,
    createdAt: s.createdAt as string,
    produto: s.produto ? mapProduct(s.produto as Record<string, unknown>) : undefined as unknown as Product,
  });

  const mapProducts = (arr: unknown[]): Product[] => arr.map((p) => mapProduct(p as Record<string, unknown>));
  const mapMovimentacoes = (arr: unknown[]): Movimentacao[] => arr.map((m) => mapMovimentacao(m as Record<string, unknown>));

  const mapBOM = (s: Record<string, unknown>): BOM => ({
    id: s.id as number,
    productId: s.produtoId ?? s.productId as number,
    product: s.produto ? mapProduct(s.produto as Record<string, unknown>) : undefined as unknown as Product,
    materials: ((s.materials ?? []) as Record<string, unknown>[]).map((m: Record<string, unknown>) => ({
      id: m.id as number,
      bomId: m.materiasId ?? m.bomId as number,
      materialId: m.materialId as number,
      bom: undefined as unknown as BOM,
      material: m.produtos ? mapProduct(m.produtos as Record<string, unknown>) : undefined as unknown as Product,
      quantity: m.quantidade ?? m.quantity as number,
    })),
    createdAt: s.createdAt as string,
    updatedAt: s.updatedAt as string,
  });

  const mapBOMs = (arr: unknown[]): BOM[] => arr.map((b) => mapBOM(b as Record<string, unknown>));

  const toServerProduct = (p: Partial<Product>): Record<string, unknown> => {
    const out: Record<string, unknown> = {};
    if (p.code !== undefined) out.code = p.code;
    if (p.name !== undefined) out.name = p.name;
    if (p.type !== undefined) out.tipo = p.type;
    if (p.unit !== undefined) out.unit = p.unit;
    if (p.currentStock !== undefined) out.estoqueAtual = p.currentStock;
    if (p.reservedStock !== undefined) out.estoqueReservado = p.reservedStock;
    if (p.minStock !== undefined) out.estoqueMínimo = p.minStock;
    return out;
  };

  const extractArray = (data: unknown): unknown[] => {
    if (Array.isArray(data)) return data;
    if (data && typeof data === "object") {
      const obj = data as Record<string, unknown>;
      if (Array.isArray(obj.movements)) return obj.movements;
      if (Array.isArray(obj.orders)) return obj.orders;
      if (Array.isArray(obj.users)) return obj.users;
    }
    return [];
  };

  const fetchProducts = useCallback(async () => {
    try {
      const res = await API.get('/api/Produto');
      setProducts(mapProducts(extractArray(res.data)));
    } catch (err) {
      console.error('fetchProducts error:', err);
    }
  }, []);

  const fetchAll = useCallback(async () => {
    const fetchData = async () => {
      try {
        const productsRes = await API.get('/api/Produto');
        setProducts(mapProducts(extractArray(productsRes.data)));
      } catch { /* silent */ }

      try {
        const stockRes = await API.get('/api/Movimentacao');
        setMovimentacoes(mapMovimentacoes(extractArray(stockRes.data)));
      } catch { /* silent */ }

      try {
        const ordersRes = await API.get('/api/ProductionOrder');
        setProductionOrders(extractArray(ordersRes.data) as ProductionOrder[]);
      } catch { /* silent */ }

      try {
        const invoicesRes = await API.get('/api/NotaFiscal');
        setInvoices(extractArray(invoicesRes.data) as Invoice[]);
      } catch { /* silent */ }

      try {
        const bomsRes = await API.get('/api/BOM');
        setBoms(mapBOMs(extractArray(bomsRes.data)));
      } catch { /* silent */ }
    };

    await fetchData();
  }, []);

  useEffect(() => {
    let mounted = true;

    const fetchData = async () => {
      try {
        const productsRes = await API.get('/api/Produto');
        if (mounted) setProducts(mapProducts(extractArray(productsRes.data)));
      } catch { /* silent */ }

      try {
        const stockRes = await API.get('/api/Movimentacao');
        if (mounted) setMovimentacoes(mapMovimentacoes(extractArray(stockRes.data)));
      } catch { /* silent */ }

      try {
        const ordersRes = await API.get('/api/ProductionOrder');
        if (mounted) setProductionOrders(extractArray(ordersRes.data) as ProductionOrder[]);
      } catch { /* silent */ }

      try {
        const invoicesRes = await API.get('/api/NotaFiscal');
        if (mounted) setInvoices(extractArray(invoicesRes.data) as Invoice[]);
      } catch { /* silent */ }

      try {
        const bomsRes = await API.get('/api/BOM');
        if (mounted) setBoms(mapBOMs(extractArray(bomsRes.data)));
      } catch { /* silent */ }
    };

    fetchData();
    return () => { mounted = false; };
  }, []);

  const addProduct = useCallback(async (product: Omit<Product, 'id'>): Promise<Product> => {
    const res = await API.post('/api/Produto', toServerProduct(product));
    await fetchProducts();
    return res.data as Product;
  }, [fetchProducts]);

  const updateProduct = useCallback(async (id: number, product: Partial<Product>): Promise<void> => {
    await API.put(`/api/Produto/${id}`, toServerProduct(product));
    setProducts(prev => prev.map(p => (p.id === id ? { ...p, ...product } : p)));
  }, []);

  const addProductionOrder = useCallback(async (
    order: Omit<ProductionOrder, 'id' | 'createdAt' | 'finishedAt' | 'produced'>
  ): Promise<ProductionOrder> => {
    const payload = { productId: order.productId, usuarioId: order.usuarioId, quantity: order.quantity };
    const res = await API.post<ProductionOrder>('/api/ProductionOrder', payload);
    await fetchAll();
    return res.data;
  }, [fetchAll]);

  const updateProductionOrder = useCallback(async (id: number, order: Partial<ProductionOrder>): Promise<void> => {
    const payload: Record<string, unknown> = {};
    if (order.status) payload.status = order.status;
    if (order.produced !== undefined) payload.produced = order.produced;

    await API.put(`/api/ProductionOrder/${id}`, payload);
    setProductionOrders(prev => prev.map(o => (o.id === id ? { ...o, ...order } : o)));
  }, []);

  const addStockMovement = useCallback(async (movement: Omit<Movimentacao, 'id' | 'createdAt'>): Promise<Movimentacao> => {
    const { type, ...rest } = movement;
    const payload = { ...rest, tipo: type };
    const res = await API.post<Movimentacao>('/api/Movimentacao', payload);
    setMovimentacoes(prev => [res.data, ...prev]);
    return res.data;
  }, []);

  const addInvoice = useCallback(async (invoice: Omit<Invoice, 'id' | 'date'>): Promise<Invoice> => {
    const payload = { ...invoice, date: new Date().toISOString() };
    const res = await API.post<Invoice>('/api/NotaFiscal', payload);
    setInvoices(prev => [res.data, ...prev]);
    return res.data;
  }, []);

  const addBOM = useCallback(async (bom: Omit<BOM, 'id'>): Promise<BOM> => {
    const payload = {
      productId: bom.productId,
      materials: bom.materials.map(m => ({
        materialId: m.materialId,
        quantidade: m.quantity,
      })),
    };
    const res = await API.post<BOM>('/api/BOM', payload);
    await fetchAll();
    return res.data;
  }, [fetchAll]);

  const refreshAll = useCallback(async () => {
    await fetchAll();
  }, [fetchAll]);

  const value = useMemo(() => ({
    products,
    boms,
    stockMovements: movimentacoes,
    productionOrders,
    invoices,
    addProduct,
    updateProduct,
    addProductionOrder,
    updateProductionOrder,
    addStockMovement,
    addInvoice,
    addBOM,
    refreshAll,
  }), [
    products,
    boms,
    movimentacoes,
    productionOrders,
    invoices,
    addProduct,
    updateProduct,
    addProductionOrder,
    updateProductionOrder,
    addStockMovement,
    addInvoice,
    addBOM,
    refreshAll,
  ]);

  return (
    <AppContext.Provider value={value}>
      {children}
    </AppContext.Provider>
  );
};
