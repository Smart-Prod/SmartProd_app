import { useApp } from "../contexts/AppContext";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

export const BomList: React.FC = () => {
  const { boms } = useApp();

  return (
    <div className="space-y-6">
      <header>
        <h1 className="text-3xl">Estruturas de Produtos</h1>
        <p className="text-gray-600">Receitas cadastradas para produção.</p>
      </header>
      <Card>
        <CardHeader><CardTitle>Lista de BOMs</CardTitle></CardHeader>
        <CardContent>
          {boms.length === 0 ? (
            <p>Nenhuma estrutura de produto cadastrada.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow><TableHead>Produto acabado</TableHead><TableHead>Matérias-primas</TableHead></TableRow>
              </TableHeader>
              <TableBody>
                {boms.map(bom => (
                  <TableRow key={bom.id}>
                    <TableCell>{bom.product?.name ?? `Produto #${bom.productId}`}</TableCell>
                    <TableCell>
                      <ul className="space-y-1">
                        {bom.materials.map(item => (
                          <li key={item.id}>
                            {item.material?.name ?? `Material #${item.materialId}`} — {item.quantity} {item.material?.unit ?? ""}
                          </li>
                        ))}
                      </ul>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
};
