namespace projectPartiuDestino.Models
{
    public class CarrinhoItem
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        /// <summary>"pacote" | "destino" | "viagem_personalizada"</summary>
        public string TipoItem { get; set; } = string.Empty;

        public int ItemId { get; set; }
        public string NomeItem { get; set; } = string.Empty;
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; } = 1;
        public DateTime DataAdicionado { get; set; }

        // Propriedade calculada — não vem do banco
        public decimal Subtotal => PrecoUnitario * Quantidade;
    }
}