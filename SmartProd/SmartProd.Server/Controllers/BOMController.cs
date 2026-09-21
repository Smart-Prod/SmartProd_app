using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/BOM")]
    [Authorize]
    public class BOMController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BOMController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var boms = await _context.Materiais
                    .Include(b => b.Produto)
                    .Include(b => b.Materials)
                        .ThenInclude(bm => bm.Produtos)
                    .ToListAsync();
                return Ok(boms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var bom = await _context.Materiais
                    .Include(b => b.Produto)
                    .Include(b => b.Materials)
                        .ThenInclude(bm => bm.Produtos)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (bom == null)
                    return NotFound(new { error = "BOM não encontrada." });

                return Ok(bom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBomDto dto)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(dto.ProductId);
                if (produto == null)
                    return BadRequest(new { error = "Produto não encontrado." });

                var bom = new Materiais
                {
                    ProdutoId = dto.ProductId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Materiais.Add(bom);
                await _context.SaveChangesAsync();

                if (dto.Materials != null)
                {
                    foreach (var item in dto.Materials)
                    {
                        var materialProduto = await _context.Produtos.FindAsync(item.MaterialId);
                        if (materialProduto == null)
                            continue;

                        bom.Materials.Add(new MateriaisItems
                        {
                            MateriaisId = bom.Id,
                            ProdutosId = item.MaterialId,
                            Quantidade = item.Quantidade
                        });
                    }
                }

                await _context.SaveChangesAsync();

                await _context.Entry(bom).Reference(b => b.Produto).LoadAsync();
                await _context.Entry(bom).Collection(b => b.Materials).LoadAsync();
                foreach (var m in bom.Materials)
                    await _context.Entry(m).Reference(bm => bm.Produtos).LoadAsync();

                return StatusCode(201, bom);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public class CreateBomDto
        {
            public int ProductId { get; set; }
            public List<BomItemDto>? Materials { get; set; }
        }

        public class BomItemDto
        {
            public int MaterialId { get; set; }
            public double Quantidade { get; set; }
        }
    }
}