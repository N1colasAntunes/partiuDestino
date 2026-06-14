namespace projectPartiuDestino.Models
{
    public class Pedidos
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }
        public string TipoItem { get; set; }
        public string NomeItem { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public DateTime DataPedido { get; set; }

        public decimal Subtotal => PrecoUnitario * Quantidade;
    }
}