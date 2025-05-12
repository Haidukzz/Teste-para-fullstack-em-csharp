using System;
using System.Collections.Generic;
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
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context) => _context = context;

        // GET api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetAll()
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .ToListAsync();
            return Ok(clientes);
        }

        // GET api/clientes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();
            return Ok(cliente);
        }

        // GET api/clientes/auth?cpf=...&dataNascimento=yyyy-MM-dd
        [HttpGet("auth")]
        public async Task<IActionResult> Authenticate([FromQuery] string cpf, [FromQuery] DateTime dataNascimento)
        {
            // garantir que a data de consulta fique em UTC
            var dataUtc = DateTime.SpecifyKind(dataNascimento, DateTimeKind.Utc);

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.CPF == cpf &&
                    c.DataNascimento.Date == dataUtc.Date);

            if (cliente == null)
                return NotFound("CPF ou data de nascimento inválidos.");

            return Ok(cliente);
        }

        // POST api/clientes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!ValidarCpf(cliente.CPF))
                return BadRequest("CPF inválido.");

            if (await _context.Clientes.AnyAsync(c => c.CPF == cliente.CPF))
                return Conflict("CPF já cadastrado.");

            // converter DataNascimento para UTC antes de qualquer validação de idade
            // assume-se que cliente.DataNascimento possa vir com Kind=Unspecified
            cliente.DataNascimento = cliente.DataNascimento.Kind switch
            {
                DateTimeKind.Utc   => cliente.DataNascimento,
                DateTimeKind.Local => cliente.DataNascimento.ToUniversalTime(),
                _                  => DateTime.SpecifyKind(cliente.DataNascimento, DateTimeKind.Utc)
            };

            if (cliente.DataNascimento > DateTime.UtcNow.AddYears(-18))
                return BadRequest("Cliente deve ter 18 anos ou mais.");

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        // DELETE api/clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ValidarCpf(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "").Trim();
            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
                return false;

            var invalidCpfs = new[]
            {
                "00000000000", "11111111111", "22222222222", "33333333333",
                "44444444444", "55555555555", "66666666666", "77777777777",
                "88888888888", "99999999999"
            };
            if (invalidCpfs.Contains(cpf))
                return false;

            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpf.Substring(0, 9);
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (tempCpf[i] - '0') * mult1[i];

            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;
            tempCpf += firstDigit;
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (tempCpf[i] - '0') * mult2[i];

            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

            return cpf.EndsWith($"{firstDigit}{secondDigit}");
        }
    }
}
