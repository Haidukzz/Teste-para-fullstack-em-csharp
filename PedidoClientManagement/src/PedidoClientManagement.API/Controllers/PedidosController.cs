using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PedidoClientManagement.API.Data;
using PedidoClientManagement.API.Models;

namespace PedidoClientManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PedidosController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .AsNoTracking()
                .ToListAsync();
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
                return NotFound(new { message = "Pedido não encontrado." });
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Pedido pedido)
        {
            // valida cliente
            if (!await _context.Clientes.AnyAsync(c => c.Id == pedido.ClienteId))
                return NotFound(new { message = "Cliente não encontrado." });

            // valida itens
            if (pedido.Itens == null || !pedido.Itens.Any())
                return BadRequest(new { message = "O pedido deve ter ao menos um item." });

            foreach (var item in pedido.Itens)
            {
                if (string.IsNullOrWhiteSpace(item.Descricao))
                    return BadRequest(new { message = "Descrição de cada item é obrigatória." });
                if (item.Quantidade < 1)
                    return BadRequest(new { message = "Quantidade deve ser pelo menos 1." });
                if (item.PrecoUnitario < 0)
                    return BadRequest(new { message = "Preço unitário não pode ser negativo." });
            }

            // Garantir que DataPedido seja UTC agora
            pedido.DataPedido = DateTime.UtcNow;

            // Calcular total e salvar
            pedido.CalcularTotal();
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Pedido update)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
                return NotFound(new { message = "Pedido não encontrado." });

            // Verificar janela de 24h usando UTC
            if ((DateTime.UtcNow - pedido.DataPedido).TotalHours > 24)
                return BadRequest(new { message = "Pedido não pode ser alterado após 24 horas." });

            // valida itens no update
            if (update.Itens == null || !update.Itens.Any())
                return BadRequest(new { message = "O pedido deve ter ao menos um item." });

            foreach (var item in update.Itens)
            {
                if (string.IsNullOrWhiteSpace(item.Descricao))
                    return BadRequest(new { message = "Descrição de cada item é obrigatória." });
                if (item.Quantidade < 1)
                    return BadRequest(new { message = "Quantidade deve ser pelo menos 1." });
                if (item.PrecoUnitario < 0)
                    return BadRequest(new { message = "Preço unitário não pode ser negativo." });
            }

            // Atualizar itens e recalcular total
            pedido.Itens = update.Itens;
            pedido.CalcularTotal();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null)
                return NotFound(new { message = "Pedido não encontrado." });

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("filtro")]
        public async Task<IActionResult> GetByFiltro(
            [FromQuery] string? nome,
            [FromQuery] DateTime? inicio,
            [FromQuery] DateTime? fim)
        {
            var query = _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => p.Cliente!.Nome.Contains(nome));

            if (inicio.HasValue)
            {
                // Interpretar inicio como UTC
                var dtInicio = inicio.Value.Kind == DateTimeKind.Utc
                    ? inicio.Value
                    : DateTime.SpecifyKind(inicio.Value, DateTimeKind.Utc);
                query = query.Where(p => p.DataPedido >= dtInicio);
            }

            if (fim.HasValue)
            {
                // Interpretar fim como UTC
                var dtFim = fim.Value.Kind == DateTimeKind.Utc
                    ? fim.Value
                    : DateTime.SpecifyKind(fim.Value, DateTimeKind.Utc);
                query = query.Where(p => p.DataPedido <= dtFim);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }
    }
}
