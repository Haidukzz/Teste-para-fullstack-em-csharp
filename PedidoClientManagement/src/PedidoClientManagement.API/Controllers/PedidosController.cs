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
        if (pedido == null) return NotFound(new { message = "Pedido não encontrado." });
        return Ok(pedido);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Pedido pedido)
    {
        if (!await _context.Clientes.AnyAsync(c => c.Id == pedido.ClienteId))
            return NotFound(new { message = "Cliente não encontrado." });

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
        if (pedido == null) return NotFound(new { message = "Pedido não encontrado." });

        if ((DateTime.Now - pedido.DataPedido).TotalHours > 24)
            return BadRequest(new { message = "Pedido não pode ser alterado após 24 horas." });

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
        if (pedido == null) return NotFound(new { message = "Pedido não encontrado." });

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("filtro")]
    public async Task<IActionResult> GetByFiltro(string? nome, DateTime? inicio, DateTime? fim)
    {
        var query = _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(p => p.Cliente!.Nome.Contains(nome));
        if (inicio.HasValue)
            query = query.Where(p => p.DataPedido >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(p => p.DataPedido <= fim.Value);

        var result = await query.ToListAsync();
        return Ok(result);
    }
}
   }