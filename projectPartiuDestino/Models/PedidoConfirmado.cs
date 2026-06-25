namespace projectPartiuDestino.Models
{
    public class PedidoConfirmado
    {
        public int Id { get; set; }

        public string? CodigoReserva { get; set; }

        public string NomeItem { get; set; } = string.Empty;

        public DateTime DataPedido { get; set; }

        public decimal PrecoUnitario { get; set; }

        public int Quantidade { get; set; }

        public string TipoItem { get; set; } = string.Empty;

        public string? FormaPagamento { get; set; }

        public string? StatusPagamento { get; set; }

        public decimal ValorTotalPedido { get; set; }

        public int Parcelas { get; set; }

        public string? Comprovante { get; set; }

        public DateTime? DataPagamento { get; set; }

        public decimal Total => PrecoUnitario * Quantidade;
    }
}