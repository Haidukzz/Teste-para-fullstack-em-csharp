// Models/Pedido.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PedidoClientManagement.API.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public DateTime DataPedido { get; set; }

        [Required]
        public decimal ValorTotal { get; set; }

        public List<ItemPedido>? Itens { get; set; }

        public void CalcularTotal()
        {
            DataPedido = DateTime.Now;
            if (Itens != null && Itens.Any())
                ValorTotal = Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
            else
                ValorTotal = 0m;
        }
    }
}
