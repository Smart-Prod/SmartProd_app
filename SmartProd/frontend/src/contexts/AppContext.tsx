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
  addMovimentacao: (movement: Omit<Movimentacao, 'id' | 'createdAt'>) => Promise<Movimentacao>;
  addStockMovement: (movement: Omit<Movimentacao, 'id' | 'createdAt'>) => Promise<Movimentacao>;
  addInvoice: (invoice: Omit<Invoice, 'id' | 'date'>) => Promise<Invoice>;
  addBOM: (bom: Omit<BOM, 'id'>) => Promise<BOM>;
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
  const [stockMovements, setStockMovements] = useState<Movimentacao[]>([]);
  const [productionOrders, setProductionOrders] = useState<ProductionOrder[]>([]);
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  // ── Mapping: server JSON → frontend model ──────────────────────
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
  const mapStockMovements = (arr: unknown[]): Movimentacao[] => arr.map((m) => mapMovimentacao(m as Record<string, unknown>));

  // ── Mapping: frontend model → server JSON (for POST/PUT) ──────
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

  // Helper to extract array data from API responses
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

  useEffect(() => {
    let mounted = true;

    const fetchData = async () => {
      try {
        const productsRes = await API.get('/api/Produto');
        if (mounted) setProducts(mapProducts(extractArray(productsRes.data)));
      } catch { /* silent */ }

      try {
        const stockRes = await API.get('/api/Movimentacao');
        if (mounted) setStockMovements(mapStockMovements(extractArray(stockRes.data)));
      } catch { /* silent */ }

      try {
        const ordersRes = await API.get('/api/ProductionOrder');
        if (mounted) setProductionOrders(extractArray(ordersRes.data) as ProductionOrder[]);
      } catch { /* silent */ }

      try {
        const invoicesRes = await API.get('/api/NotaFiscal');
        if (mounted) setInvoices(extractArray(invoicesRes.data) as Invoice[]);
      } catch { /* silent */ }
    };

    fetchData();
    return () => { mounted = false; };
  }, []);

  const genId = () => Date.now();

  const addProduct = useCallback(async (product: Omit<Product, 'id'>): Promise<Product> => {
    try {
      const res = await API.post('/api/Produto', toServerProduct(product));
      await fetchProducts();
      return res.data as Product;
    } catch (err: any) {
      console.error('addProduct: API failed', err.response?.data || err.message);
      throw err;
    }
  }, [fetchProducts]);

  const updateProduct = useCallback(async (id: number, product: Partial<Product>): Promise<void> => {
    try {
      await API.put(`/api/Produto/${id}`, toServerProduct(product));
    } catch (err) {
      console.warn('updateProduct: API failed', err);
    }
    setProducts(prev => prev.map(p => (p.id === id ? { ...p, ...product } : p)));
  }, []);

  const addProductionOrder = useCallback(async (
    order: Omit<ProductionOrder, 'id' | 'createdAt' | 'finishedAt' | 'produced'>
  ): Promise<ProductionOrder> => {
    const payload = { ...order, createdAt: new Date().toISOString(), produced: 0 };
    try {
      const res = await API.post<ProductionOrder>('/api/ProductionOrder', payload);
      setProductionOrders(prev => [...prev, res.data]);
      return res.data;
    } catch (err) {
      console.warn('addProductionOrder: API failed, using local fallback', err);
      const newOrder: ProductionOrder = { ...payload, id: genId() };
      setProductionOrders(prev => [...prev, newOrder]);
      return newOrder;
    }
  }, []);

  const updateProductionOrder = useCallback(async (id: number, order: Partial<ProductionOrder>): Promise<void> => {
    try {
      await API.put(`/api/ProductionOrder/${id}`, order);
      setProductionOrders(prev => prev.map(o => (o.id === id ? { ...o, ...order } : o)));
    } catch (err) {
      console.warn('updateProductionOrder: API failed, applied local update', err);
      setProductionOrders(prev => prev.map(o => (o.id === id ? { ...o, ...order } : o)));
    }
  }, []);

  const addStockMovement = useCallback(async (movement: Omit<Movimentacao, 'id' | 'createdAt'>): Promise<Movimentacao> => {
    const { type, ...rest } = movement;
    const payload = { ...rest, tipo: type, createdAt: new Date().toISOString() };
    try {
      const res = await API.post<Movimentacao>('/api/Movimentacao', payload);
      setStockMovements(prev => [res.data, ...prev]);
      return res.data;
    } catch (err) {
      console.warn('addMovimentacao: API failed, using local fallback', err);
      const newMovement: Movimentacao = { ...movement, id: genId(), createdAt: new Date().toISOString() };
      setStockMovements(prev => [newMovement, ...prev]);
      return newMovement;
    }
  }, []);

  const addInvoice = useCallback(async (invoice: Omit<Invoice, 'id' | 'date'>): Promise<Invoice> => {
    const payload = { ...invoice, date: new Date().toISOString() };
    try {
      const res = await API.post<Invoice>('/api/NotaFiscal', payload);
      setInvoices(prev => [res.data, ...prev]);
      return res.data;
    } catch (err) {
      console.warn('addInvoice: API failed, using local fallback', err);
      const newInvoice: Invoice = { ...payload, id: genId() };
      setInvoices(prev => [newInvoice, ...prev]);
      return newInvoice;
    }
  }, []);

  const addBOM = useCallback(async (bom: Omit<BOM, 'id'>): Promise<BOM> => {
    try {
      const res = await API.post<BOM>('/api/BOM', bom);
      setBoms(prev => [...prev, res.data]);
      return res.data;
    } catch (err) {
      console.warn('addBOM: API failed, using local fallback', err);
      const newBOM: BOM = { ...bom, id: genId() };
      setBoms(prev => [...prev, newBOM]);
      return newBOM;
    }
  }, []);

  const value = useMemo(() => ({
    products,
    boms,
    stockMovements,
    productionOrders,
    invoices,
    addProduct,
    updateProduct,
    addProductionOrder,
    updateProductionOrder,
    addMovimentacao: addStockMovement,
    addStockMovement,
    addInvoice,
    addBOM,
  }), [
    products,
    boms,
    stockMovements,
    productionOrders,
    invoices,
    addProduct,
    updateProduct,
    addProductionOrder,
    updateProductionOrder,
    addStockMovement,
    addInvoice,
    addBOM,
  ]);

  return (
    <AppContext.Provider value={value}>
      {children}
    </AppContext.Provider>
  );
};
