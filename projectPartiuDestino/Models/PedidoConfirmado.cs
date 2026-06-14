namespace projectPartiuDestino.Models
{
    public class PedidoConfirmado
    {
        public int Id { get; set; }
        public string NomeItem { get; set; }
        public DateTime DataPedido { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }

        public decimal Total => PrecoUnitario * Quantidade;
    }
}