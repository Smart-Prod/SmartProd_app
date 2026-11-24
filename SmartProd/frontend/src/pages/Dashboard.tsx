import React, { useState } from 'react';
import { useApp } from '../contexts/AppContext';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import {
  Package,
  Factory,
  TrendingUp,
  Activity,
  RotateCw,
  ShieldAlert,
  Radio,
} from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line } from 'recharts';
import { ProductionStatus } from '../models';

const statusColors: Record<ProductionStatus, string> = {
  PLANEJADA: 'bg-info',
  EM_PRODUCAO: 'bg-brand-orange',
  PAUSADA: 'bg-gray-500',
  CONCLUIDA: 'bg-success',
  CANCELADA: 'bg-danger'
};

// runtime guard to narrow unknown status values to the ProductionStatus union
function isStatus(value: unknown): value is ProductionStatus {
  return (
    typeof value === 'string' &&
    (value === 'PLANEJADA' ||
      value === 'EM_PRODUCAO' ||
      value === 'PAUSADA' ||
      value === 'CONCLUIDA' ||
      value === 'CANCELADA')
  );
}

export const Dashboard: React.FC = () => {
  const { products, productionOrders, movimentacoes, refreshData } = useApp();
  const [isSyncing, setIsSyncing] = useState(false);

  const handleSync = async () => {
    setIsSyncing(true);
    try {
      await refreshData();
    } finally {
      setTimeout(() => setIsSyncing(false), 500);
    }
  };

  // Calculate metrics
  const mpProducts = products.filter(p => p.type === 'MP');
  const paProducts = products.filter(p => p.type === 'PA');

  const ruptureProducts = products.filter(p => p.currentStock <= p.minStock);

  const activeOrders = productionOrders.filter(o => o.status === 'EM_PRODUCAO');
  const plannedOrders = productionOrders.filter(o => o.status === 'PLANEJADA');
  const completedOrders = productionOrders.filter(o => o.status === 'CONCLUIDA');
  const canceledOrders = productionOrders.filter(o => o.status === 'CANCELADA');

  // OEE (Overall Equipment Effectiveness) Calculation
  const totalOrders = productionOrders.length;
  const totalPlannedUnits = productionOrders.reduce((sum, o) => sum + o.quantity, 0);
  const totalProducedUnits = productionOrders.reduce((sum, o) => sum + o.produced, 0);

  // Disponibilidade (A): Razão entre ordens produtivas (ativas/concluídas) e o total de ordens
  const disponibilidade = totalOrders > 0
    ? (completedOrders.length + activeOrders.length) / totalOrders
    : 1.0;

  // Performance (P): Razão entre o total produzido e o total planejado
  const performance = totalPlannedUnits > 0
    ? Math.min(1.0, totalProducedUnits / totalPlannedUnits)
    : (totalOrders > 0 ? 0.85 : 1.0);

  // Qualidade (Q): Razão de conformidade (concluídas sem cancelamento/perdas)
  const finalizedCount = completedOrders.length + canceledOrders.length;
  const qualidade = finalizedCount > 0
    ? completedOrders.length / finalizedCount
    : 1.0;

  const oeePercent = Math.round(disponibilidade * performance * qualidade * 100);
  const dispPercent = Math.round(disponibilidade * 100);
  const perfPercent = Math.round(performance * 100);
  const qualPercent = Math.round(qualidade * 100);

  // Production chart data
  const productionData = productionOrders.map((order) => {
    const product = products.find(p => p.id === order.productId);
    return {
      produto: product?.name || 'Produto',
      planejado: order.quantity,
      produzido: order.produced,
      progresso: Math.round((order.produced / order.quantity) * 100)
    };
  });

  const stockData = products.map(product => ({
    produto: product.name,
    atual: product.currentStock,
    minimo: product.minStock,
    tipo: product.type
  }));

  const movementData = movimentacoes.slice(0, 7).reverse().map((movement) => {
    return {
      data: new Date(movement.createdAt).toLocaleDateString('pt-BR').slice(0, 5),
      entradas: movement.type === 'ENTRADA' ? movement.quantity : 0,
      saidas: movement.type === 'SAIDA' ? movement.quantity : 0,
      producao: movement.type === 'PRODUCAO' ? movement.quantity : 0
    };
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold">Dashboard Gerencial</h1>
          <p className="text-gray-600">Indicadores industriais OEE, estoque e produção em tempo real</p>
        </div>
        <div className="flex items-center gap-2">
          <Badge variant="outline" className="flex items-center gap-1.5 px-3 py-1 bg-green-50 text-green-700 border-green-200">
            <Radio className="h-3 w-3 animate-pulse text-green-600" />
            <span>Sincronização Ativa</span>
          </Badge>
          <Button
            variant="outline"
            size="sm"
            onClick={handleSync}
            disabled={isSyncing}
            className="flex items-center gap-1.5"
          >
            <RotateCw className={`h-4 w-4 ${isSyncing ? 'animate-spin text-blue-600' : ''}`} />
            <span>Sincronizar</span>
          </Button>
        </div>
      </div>

      {/* Alertas de Ruptura de Estoque */}
      {ruptureProducts.length > 0 && (
        <Alert variant="destructive" className="border-red-300 bg-red-50 text-red-900">
          <ShieldAlert className="h-5 w-5 text-red-600" />
          <AlertDescription>
            <span className="font-semibold">{ruptureProducts.length} produto(s) em estado crítico ou de ruptura:</span>{' '}
            {ruptureProducts.map(p => (
              <span key={p.id} className="inline-block mr-2 font-mono text-xs bg-red-100 px-1.5 py-0.5 rounded border border-red-300">
                {p.name} (Atual: {p.currentStock} / Mín: {p.minStock})
              </span>
            ))}
          </AlertDescription>
        </Alert>
      )}

      {/* OEE INDUSTRIAL WIDGET */}
      <Card className="border-slate-200 shadow-sm overflow-hidden">
        <CardHeader className="bg-gradient-to-r from-slate-900 to-slate-800 text-white pb-4">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2">
            <div>
              <CardTitle className="text-lg flex items-center gap-2 text-white">
                <Activity className="h-5 w-5 text-emerald-400" />
                OEE — Eficiência Global dos Equipamentos
              </CardTitle>
              <CardDescription className="text-slate-300 text-xs">
                Métrica padrão internacional da indústria: Disponibilidade × Performance × Qualidade
              </CardDescription>
            </div>
            <div className="flex items-center gap-3">
              <div className="text-right">
                <span className="text-3xl font-extrabold text-emerald-400">{oeePercent}%</span>
                <p className="text-[10px] text-slate-300 uppercase tracking-wider">Índice Geral</p>
              </div>
              <Badge className={
                oeePercent >= 85 ? 'bg-emerald-600' : oeePercent >= 65 ? 'bg-amber-600' : 'bg-rose-600'
              }>
                {oeePercent >= 85 ? 'Classe Mundial' : oeePercent >= 65 ? 'Operação Típica' : 'Atenção Operacional'}
              </Badge>
            </div>
          </div>
        </CardHeader>
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="space-y-2 bg-slate-50 p-4 rounded-lg border">
              <div className="flex justify-between items-center text-sm">
                <span className="font-medium text-slate-700">Disponibilidade</span>
                <span className="font-bold text-slate-900">{dispPercent}%</span>
              </div>
              <Progress value={dispPercent} className="h-2.5" />
              <p className="text-xs text-muted-foreground">
                {activeOrders.length + completedOrders.length} de {totalOrders} OPs operacionais
              </p>
            </div>

            <div className="space-y-2 bg-slate-50 p-4 rounded-lg border">
              <div className="flex justify-between items-center text-sm">
                <span className="font-medium text-slate-700">Performance</span>
                <span className="font-bold text-slate-900">{perfPercent}%</span>
              </div>
              <Progress value={perfPercent} className="h-2.5" />
              <p className="text-xs text-muted-foreground">
                {totalProducedUnits} de {totalPlannedUnits} peças produzidas
              </p>
            </div>

            <div className="space-y-2 bg-slate-50 p-4 rounded-lg border">
              <div className="flex justify-between items-center text-sm">
                <span className="font-medium text-slate-700">Qualidade</span>
                <span className="font-bold text-slate-900">{qualPercent}%</span>
              </div>
              <Progress value={qualPercent} className="h-2.5" />
              <p className="text-xs text-muted-foreground">
                {completedOrders.length} concluídas com conformidade total
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Matéria-Prima</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl">{mpProducts.length}</div>
            <p className="text-xs text-muted-foreground">
              {mpProducts.filter(p => p.currentStock > p.minStock).length} em níveis adequados
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Produtos Acabados</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl">{paProducts.length}</div>
            <p className="text-xs text-muted-foreground">
              {paProducts.reduce((sum, p) => sum + p.currentStock, 0)} unidades em estoque
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">OPs Ativas</CardTitle>
            <Factory className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl">{activeOrders.length}</div>
            <p className="text-xs text-muted-foreground">
              {plannedOrders.length} planejadas, {completedOrders.length} concluídas
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm">Movimentações</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl">{movimentacoes.length}</div>
            <p className="text-xs text-muted-foreground">
              Total de movimentações registradas
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Progresso das Ordens de Produção</CardTitle>
            <CardDescription>Planejado vs Produzido</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={productionData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="produto" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="planejado" fill="#e2e8f0" name="Planejado" />
                <Bar dataKey="produzido" fill="#3b82f6" name="Produzido" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Níveis de Estoque</CardTitle>
            <CardDescription>Atual vs Mínimo</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={stockData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="produto" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="minimo" fill="#ef4444" name="Mínimo" />
                <Bar dataKey="atual" fill="#10b981" name="Atual" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Production Orders Status */}
      <Card>
        <CardHeader>
          <CardTitle>Status das Ordens de Produção</CardTitle>
          <CardDescription>Situação atual das OPs</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {productionOrders.length === 0 ? (
              <p className="text-gray-500 text-center py-4">Nenhuma ordem de produção registrada</p>
            ) : (
              productionOrders.map((order) => {
                const product = products.find(p => p.id === order.productId);
                const progress = Math.round((order.produced / order.quantity) * 100);

                // isStatus safely narrows an untrusted API status value.
                const badgeClass = isStatus(order.status) ? statusColors[order.status] : 'bg-gray-400';
                const statusLabel = isStatus(order.status) ? order.status.replace('_', ' ') : String(order.status);

                return (
                  <div key={order.id} className="space-y-2">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm">{product?.name}</p>
                        <p className="text-xs text-gray-500">OP: {order.id}</p>
                      </div>
                      <div className="flex items-center space-x-2">
                        <span className="text-sm">{order.produced}/{order.quantity}</span>
                        <Badge
                          variant="outline"
                          className={`text-white ${badgeClass}`}
                        >
                          {statusLabel}
                        </Badge>
                      </div>
                    </div>
                    <Progress value={progress} className="h-2" />
                  </div>
                );
              })
            )}
          </div>
        </CardContent>
      </Card>

      {/* Recent Stock Movements */}
      {movementData.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Movimentações Recentes</CardTitle>
            <CardDescription>Entradas, saídas e produção dos últimos dias</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={movementData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="data" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="entradas" stroke="#10b981" name="Entradas" />
                <Line type="monotone" dataKey="saidas" stroke="#ef4444" name="Saídas" />
                <Line type="monotone" dataKey="producao" stroke="#3b82f6" name="Produção" />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      )}
    </div>
  );
};