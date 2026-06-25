namespace projectPartiuDestino.Models
{
    public class ResumoPedidoItem
    {
        public string NomeItem { get; set; } = "";
        public string TipoItem { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}