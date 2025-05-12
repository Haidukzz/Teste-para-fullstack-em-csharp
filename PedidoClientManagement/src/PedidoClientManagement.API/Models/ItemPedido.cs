using System.ComponentModel.DataAnnotations;

namespace PedidoClientManagement.API.Models
{
    public class ItemPedido
    {
        public int Id { get; set; }

        // FK para Pedido — não marcamos como [Required] para que o binder não exija o campo
        public int PedidoId { get; set; }

        // Propriedade de navegação opcional
        public Pedido? Pedido { get; set; }

        [Required]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        public int Quantidade { get; set; }

        [Required]
        public decimal PrecoUnitario { get; set; }
    }
}
