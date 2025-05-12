using System;
using System.ComponentModel.DataAnnotations;

namespace PedidoClientManagement.API.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime DataNascimento { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos.")]
        public string CPF { get; set; } = string.Empty;
    }
}
