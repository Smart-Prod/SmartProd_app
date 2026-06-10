using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Services
{
    public class SeedService
    {
        private readonly AppDbContext _context;

        public SeedService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> SeedKaixoteAsync()
        {
            _context.Estoque.RemoveRange(_context.Estoque);
            _context.Movimentacoes.RemoveRange(_context.Movimentacoes);
            _context.OrdensProducao.RemoveRange(_context.OrdensProducao);
            _context.MateriaisItems.RemoveRange(_context.MateriaisItems);
            _context.Materiais.RemoveRange(_context.Materiais);
            _context.Produtos.RemoveRange(_context.Produtos);
            await _context.SaveChangesAsync();

            var admin = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@smartprod.com")
                ?? await _context.Usuarios.FirstAsync();

            var operatorUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == "operador@smartprod.com")
                ?? admin;

            var mpPP = new Produto
            {
                Code = "MP-GRANULADO-PP-001", Name = "Grânulos de Polipropileno (PP)", Tipo = TipoProduto.MP, Unit = "KG",
                EstoqueAtual = 2000, EstoqueReservado = 200, EstoqueMínimo = 500,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpPE = new Produto
            {
                Code = "MP-GRANULADO-PE-002", Name = "Grânulos de Polietileno (PE)", Tipo = TipoProduto.MP, Unit = "KG",
                EstoqueAtual = 1500, EstoqueReservado = 150, EstoqueMínimo = 300,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpCorAzul = new Produto
            {
                Code = "MP-CORANTE-AZUL-003", Name = "Corante Azul", Tipo = TipoProduto.MP, Unit = "KG",
                EstoqueAtual = 50, EstoqueReservado = 5, EstoqueMínimo = 10,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpCorVermelho = new Produto
            {
                Code = "MP-CORANTE-VERMELHO-004", Name = "Corante Vermelho", Tipo = TipoProduto.MP, Unit = "KG",
                EstoqueAtual = 50, EstoqueReservado = 5, EstoqueMínimo = 10,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpCorVerde = new Produto
            {
                Code = "MP-CORANTE-VERDE-005", Name = "Corante Verde", Tipo = TipoProduto.MP, Unit = "KG",
                EstoqueAtual = 50, EstoqueReservado = 5, EstoqueMínimo = 10,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpCorBranco = new Produto
            {
                Code = "MP-CORANTE-BRANCO-006", Name = "Corante Branco (TiO2)", Tipo = TipoProduto.MP, Unit = "KG",
                EstoqueAtual = 80, EstoqueReservado = 8, EstoqueMínimo = 15,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpPapelKraft = new Produto
            {
                Code = "MP-PAPEL-KRAFT-007", Name = "Papel Kraft para Embalagem", Tipo = TipoProduto.MP, Unit = "UN",
                EstoqueAtual = 2000, EstoqueReservado = 200, EstoqueMínimo = 400,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpFilmeStrech = new Produto
            {
                Code = "MP-FILME-STRECH-008", Name = "Filme Strech Plástico", Tipo = TipoProduto.MP, Unit = "UN",
                EstoqueAtual = 500, EstoqueReservado = 50, EstoqueMínimo = 100,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpAdesivo = new Produto
            {
                Code = "MP-ADESIVO-ROTULO-009", Name = "Adesivo para Rótulos Personalizados", Tipo = TipoProduto.MP, Unit = "UN",
                EstoqueAtual = 2000, EstoqueReservado = 200, EstoqueMínimo = 400,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var mpCaixa = new Produto
            {
                Code = "MP-CAIXA-PAPELAO-010", Name = "Caixa de Papelão Ondulado", Tipo = TipoProduto.MP, Unit = "UN",
                EstoqueAtual = 2000, EstoqueReservado = 200, EstoqueMínimo = 300,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };

            _context.Produtos.AddRange(mpPP, mpPE, mpCorAzul, mpCorVermelho, mpCorVerde, mpCorBranco, mpPapelKraft, mpFilmeStrech, mpAdesivo, mpCaixa);

            var paCopo200 = new Produto
            {
                Code = "PA-COPO-200ML-001", Name = "Copo Descartável 200ml (pacote c/ 100)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 500, EstoqueReservado = 50, EstoqueMínimo = 100,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paCopo300 = new Produto
            {
                Code = "PA-COPO-300ML-002", Name = "Copo Descartável 300ml (pacote c/ 100)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 300, EstoqueReservado = 30, EstoqueMínimo = 80,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paPrato18 = new Produto
            {
                Code = "PA-PRATO-18CM-003", Name = "Prato Descartável 18cm (pacote c/ 50)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 200, EstoqueReservado = 20, EstoqueMínimo = 50,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paPrato22 = new Produto
            {
                Code = "PA-PRATO-22CM-004", Name = "Prato Descartável 22cm (pacote c/ 50)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 150, EstoqueReservado = 15, EstoqueMínimo = 40,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paGarfo = new Produto
            {
                Code = "PA-TALHER-GARFO-005", Name = "Garfo Descartável (pacote c/ 50)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 400, EstoqueReservado = 40, EstoqueMínimo = 100,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paFaca = new Produto
            {
                Code = "PA-TALHER-FACA-006", Name = "Faca Descartável (pacote c/ 50)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 350, EstoqueReservado = 35, EstoqueMínimo = 80,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paColher = new Produto
            {
                Code = "PA-TALHER-COLHER-007", Name = "Colher Descartável (pacote c/ 50)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 450, EstoqueReservado = 45, EstoqueMínimo = 100,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paCopoCafe = new Produto
            {
                Code = "PA-COPO-CAFE-50ML-008", Name = "Copo para Café 50ml (pacote c/ 100)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 600, EstoqueReservado = 60, EstoqueMínimo = 150,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paBandeja = new Produto
            {
                Code = "PA-BANDEJA-ISOPOR-009", Name = "Bandeja de Isopor (pacote c/ 30)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 100, EstoqueReservado = 10, EstoqueMínimo = 30,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paGuardanapo = new Produto
            {
                Code = "PA-GUARDANAPO-010", Name = "Guardanapo Folha Dupla (pacote c/ 100)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 250, EstoqueReservado = 25, EstoqueMínimo = 50,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paToalha = new Produto
            {
                Code = "PA-TOALHA-MESA-011", Name = "Toalha de Mesa Descartável (pacote c/ 10)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 80, EstoqueReservado = 8, EstoqueMínimo = 20,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };
            var paCanudo = new Produto
            {
                Code = "PA-CANUDO-012", Name = "Canudo Descartável (pacote c/ 100)", Tipo = TipoProduto.PA, Unit = "UN",
                EstoqueAtual = 300, EstoqueReservado = 30, EstoqueMínimo = 80,
                UsuarioId = admin.Id, CreatedAt = DateTime.UtcNow
            };

            _context.Produtos.AddRange(paCopo200, paCopo300, paPrato18, paPrato22, paGarfo, paFaca, paColher, paCopoCafe, paBandeja, paGuardanapo, paToalha, paCanudo);
            await _context.SaveChangesAsync();

            var bomCopo200 = new Materiais { ProdutoId = paCopo200.Id, CreatedAt = DateTime.UtcNow };
            var bomCopo300 = new Materiais { ProdutoId = paCopo300.Id, CreatedAt = DateTime.UtcNow };
            var bomPrato18 = new Materiais { ProdutoId = paPrato18.Id, CreatedAt = DateTime.UtcNow };
            var bomPrato22 = new Materiais { ProdutoId = paPrato22.Id, CreatedAt = DateTime.UtcNow };
            var bomGarfo = new Materiais { ProdutoId = paGarfo.Id, CreatedAt = DateTime.UtcNow };
            var bomFaca = new Materiais { ProdutoId = paFaca.Id, CreatedAt = DateTime.UtcNow };
            var bomColher = new Materiais { ProdutoId = paColher.Id, CreatedAt = DateTime.UtcNow };
            var bomCopoCafe = new Materiais { ProdutoId = paCopoCafe.Id, CreatedAt = DateTime.UtcNow };
            var bomBandeja = new Materiais { ProdutoId = paBandeja.Id, CreatedAt = DateTime.UtcNow };
            var bomGuardanapo = new Materiais { ProdutoId = paGuardanapo.Id, CreatedAt = DateTime.UtcNow };
            var bomToalha = new Materiais { ProdutoId = paToalha.Id, CreatedAt = DateTime.UtcNow };
            var bomCanudo = new Materiais { ProdutoId = paCanudo.Id, CreatedAt = DateTime.UtcNow };
            _context.Materiais.AddRange(bomCopo200, bomCopo300, bomPrato18, bomPrato22, bomGarfo, bomFaca, bomColher, bomCopoCafe, bomBandeja, bomGuardanapo, bomToalha, bomCanudo);
            await _context.SaveChangesAsync();

            _context.MateriaisItems.AddRange(
                new MateriaisItems { MateriaisId = bomCopo200.Id, ProdutosId = mpPP.Id, Quantidade = 0.5 },
                new MateriaisItems { MateriaisId = bomCopo200.Id, ProdutosId = mpCorAzul.Id, Quantidade = 0.01 },
                new MateriaisItems { MateriaisId = bomCopo200.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomCopo200.Id, ProdutosId = mpAdesivo.Id, Quantidade = 5 },
                new MateriaisItems { MateriaisId = bomCopo300.Id, ProdutosId = mpPP.Id, Quantidade = 0.7 },
                new MateriaisItems { MateriaisId = bomCopo300.Id, ProdutosId = mpCorVerde.Id, Quantidade = 0.015 },
                new MateriaisItems { MateriaisId = bomCopo300.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomCopo300.Id, ProdutosId = mpAdesivo.Id, Quantidade = 5 },
                new MateriaisItems { MateriaisId = bomPrato18.Id, ProdutosId = mpPE.Id, Quantidade = 0.4 },
                new MateriaisItems { MateriaisId = bomPrato18.Id, ProdutosId = mpCorBranco.Id, Quantidade = 0.008 },
                new MateriaisItems { MateriaisId = bomPrato18.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomPrato22.Id, ProdutosId = mpPE.Id, Quantidade = 0.6 },
                new MateriaisItems { MateriaisId = bomPrato22.Id, ProdutosId = mpCorBranco.Id, Quantidade = 0.012 },
                new MateriaisItems { MateriaisId = bomPrato22.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomGarfo.Id, ProdutosId = mpPP.Id, Quantidade = 0.3 },
                new MateriaisItems { MateriaisId = bomGarfo.Id, ProdutosId = mpCorVermelho.Id, Quantidade = 0.005 },
                new MateriaisItems { MateriaisId = bomGarfo.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomFaca.Id, ProdutosId = mpPP.Id, Quantidade = 0.35 },
                new MateriaisItems { MateriaisId = bomFaca.Id, ProdutosId = mpCorVermelho.Id, Quantidade = 0.005 },
                new MateriaisItems { MateriaisId = bomFaca.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomColher.Id, ProdutosId = mpPP.Id, Quantidade = 0.25 },
                new MateriaisItems { MateriaisId = bomColher.Id, ProdutosId = mpCorVermelho.Id, Quantidade = 0.005 },
                new MateriaisItems { MateriaisId = bomColher.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomCopoCafe.Id, ProdutosId = mpPP.Id, Quantidade = 0.3 },
                new MateriaisItems { MateriaisId = bomCopoCafe.Id, ProdutosId = mpCorBranco.Id, Quantidade = 0.008 },
                new MateriaisItems { MateriaisId = bomCopoCafe.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomCopoCafe.Id, ProdutosId = mpAdesivo.Id, Quantidade = 5 },
                new MateriaisItems { MateriaisId = bomBandeja.Id, ProdutosId = mpPE.Id, Quantidade = 0.2 },
                new MateriaisItems { MateriaisId = bomBandeja.Id, ProdutosId = mpPapelKraft.Id, Quantidade = 0.5 },
                new MateriaisItems { MateriaisId = bomBandeja.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomGuardanapo.Id, ProdutosId = mpPapelKraft.Id, Quantidade = 0.2 },
                new MateriaisItems { MateriaisId = bomGuardanapo.Id, ProdutosId = mpFilmeStrech.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomGuardanapo.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomToalha.Id, ProdutosId = mpPE.Id, Quantidade = 0.8 },
                new MateriaisItems { MateriaisId = bomToalha.Id, ProdutosId = mpCorVerde.Id, Quantidade = 0.02 },
                new MateriaisItems { MateriaisId = bomToalha.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 },
                new MateriaisItems { MateriaisId = bomCanudo.Id, ProdutosId = mpPP.Id, Quantidade = 0.15 },
                new MateriaisItems { MateriaisId = bomCanudo.Id, ProdutosId = mpCorVermelho.Id, Quantidade = 0.003 },
                new MateriaisItems { MateriaisId = bomCanudo.Id, ProdutosId = mpCaixa.Id, Quantidade = 1 }
            );
            await _context.SaveChangesAsync();

            var ordens = new List<OrdemProducao>
            {
                new() { ProductId = paCopo200.Id, UsuarioId = operatorUser.Id, Quantity = 100, Produced = 100, Status = OrdemProducaoStatus.CONCLUIDA, CreatedAt = DateTime.UtcNow.AddDays(-6), FinishedAt = DateTime.UtcNow.AddDays(-5) },
                new() { ProductId = paGarfo.Id, UsuarioId = operatorUser.Id, Quantity = 80, Produced = 80, Status = OrdemProducaoStatus.CONCLUIDA, CreatedAt = DateTime.UtcNow.AddDays(-5), FinishedAt = DateTime.UtcNow.AddDays(-4) },
                new() { ProductId = paColher.Id, UsuarioId = operatorUser.Id, Quantity = 90, Produced = 90, Status = OrdemProducaoStatus.CONCLUIDA, CreatedAt = DateTime.UtcNow.AddDays(-4), FinishedAt = DateTime.UtcNow.AddDays(-3) },
                new() { ProductId = paCopo300.Id, UsuarioId = operatorUser.Id, Quantity = 50, Produced = 50, Status = OrdemProducaoStatus.CONCLUIDA, CreatedAt = DateTime.UtcNow.AddDays(-3), FinishedAt = DateTime.UtcNow.AddDays(-2) },
                new() { ProductId = paCopo200.Id, UsuarioId = operatorUser.Id, Quantity = 60, Produced = 25, Status = OrdemProducaoStatus.EM_PRODUCAO, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { ProductId = paPrato18.Id, UsuarioId = operatorUser.Id, Quantity = 40, Produced = 0, Status = OrdemProducaoStatus.EM_PRODUCAO, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { ProductId = paGuardanapo.Id, UsuarioId = operatorUser.Id, Quantity = 50, Produced = 0, Status = OrdemProducaoStatus.PLANEJADA, CreatedAt = DateTime.UtcNow },
                new() { ProductId = paCanudo.Id, UsuarioId = operatorUser.Id, Quantity = 100, Produced = 0, Status = OrdemProducaoStatus.PLANEJADA, CreatedAt = DateTime.UtcNow },
                new() { ProductId = paBandeja.Id, UsuarioId = operatorUser.Id, Quantity = 30, Produced = 0, Status = OrdemProducaoStatus.PLANEJADA, CreatedAt = DateTime.UtcNow },
                new() { ProductId = paToalha.Id, UsuarioId = operatorUser.Id, Quantity = 20, Produced = 0, Status = OrdemProducaoStatus.PLANEJADA, CreatedAt = DateTime.UtcNow },
                new() { ProductId = paCopoCafe.Id, UsuarioId = operatorUser.Id, Quantity = 120, Produced = 0, Status = OrdemProducaoStatus.EM_PRODUCAO, CreatedAt = DateTime.UtcNow },
            };
            _context.OrdensProducao.AddRange(ordens);
            await _context.SaveChangesAsync();

            var movs = new List<Movimentacao>
            {
                new() { ProductId = mpPP.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 3000, CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new() { ProductId = mpPE.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 2000, CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new() { ProductId = mpCorAzul.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 100, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new() { ProductId = mpCorVerde.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 80, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new() { ProductId = mpCorVermelho.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 80, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new() { ProductId = mpCorBranco.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 120, CreatedAt = DateTime.UtcNow.AddDays(-12) },
                new() { ProductId = mpCaixa.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 3000, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new() { ProductId = mpAdesivo.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 3000, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new() { ProductId = mpPapelKraft.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 3000, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new() { ProductId = mpFilmeStrech.Id, Tipo = TipoMovimentacao.ENTRADA, Quantity = 800, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new() { ProductId = mpPP.Id, Tipo = TipoMovimentacao.CONSUMO, Quantity = 50, CreatedAt = DateTime.UtcNow.AddDays(-6), OrderId = 1 },
                new() { ProductId = mpCorAzul.Id, Tipo = TipoMovimentacao.CONSUMO, Quantity = 1, CreatedAt = DateTime.UtcNow.AddDays(-6), OrderId = 1 },
                new() { ProductId = paCopo200.Id, Tipo = TipoMovimentacao.PRODUCAO, Quantity = 100, CreatedAt = DateTime.UtcNow.AddDays(-5), OrderId = 1 },
                new() { ProductId = paGarfo.Id, Tipo = TipoMovimentacao.PRODUCAO, Quantity = 80, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new() { ProductId = paColher.Id, Tipo = TipoMovimentacao.PRODUCAO, Quantity = 90, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { ProductId = paCopo300.Id, Tipo = TipoMovimentacao.PRODUCAO, Quantity = 50, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            };
            _context.Movimentacoes.AddRange(movs);
            await _context.SaveChangesAsync();

            _context.Estoque.AddRange(
                new Estoque { ProductId = mpPP.Id, Quantity = 2000, Unit = "KG" },
                new Estoque { ProductId = mpPE.Id, Quantity = 1500, Unit = "KG" },
                new Estoque { ProductId = mpCorAzul.Id, Quantity = 50, Unit = "KG" },
                new Estoque { ProductId = mpCorVermelho.Id, Quantity = 50, Unit = "KG" },
                new Estoque { ProductId = mpCorVerde.Id, Quantity = 50, Unit = "KG" },
                new Estoque { ProductId = mpCorBranco.Id, Quantity = 80, Unit = "KG" },
                new Estoque { ProductId = mpPapelKraft.Id, Quantity = 2000, Unit = "UN" },
                new Estoque { ProductId = mpFilmeStrech.Id, Quantity = 500, Unit = "UN" },
                new Estoque { ProductId = mpAdesivo.Id, Quantity = 2000, Unit = "UN" },
                new Estoque { ProductId = mpCaixa.Id, Quantity = 2000, Unit = "UN" },
                new Estoque { ProductId = paCopo200.Id, Quantity = 500, Unit = "UN" },
                new Estoque { ProductId = paCopo300.Id, Quantity = 300, Unit = "UN" },
                new Estoque { ProductId = paPrato18.Id, Quantity = 200, Unit = "UN" },
                new Estoque { ProductId = paPrato22.Id, Quantity = 150, Unit = "UN" },
                new Estoque { ProductId = paGarfo.Id, Quantity = 400, Unit = "UN" },
                new Estoque { ProductId = paFaca.Id, Quantity = 350, Unit = "UN" },
                new Estoque { ProductId = paColher.Id, Quantity = 450, Unit = "UN" },
                new Estoque { ProductId = paCopoCafe.Id, Quantity = 600, Unit = "UN" },
                new Estoque { ProductId = paBandeja.Id, Quantity = 100, Unit = "UN" },
                new Estoque { ProductId = paGuardanapo.Id, Quantity = 250, Unit = "UN" },
                new Estoque { ProductId = paToalha.Id, Quantity = 80, Unit = "UN" },
                new Estoque { ProductId = paCanudo.Id, Quantity = 300, Unit = "UN" }
            );
            await _context.SaveChangesAsync();

            return "Dados inseridos: 22 produtos (10 MP, 12 PA), 12 fichas técnicas, 11 ordens de produção, 16 movimentações, 22 registros de estoque.";
        }
    }
}
