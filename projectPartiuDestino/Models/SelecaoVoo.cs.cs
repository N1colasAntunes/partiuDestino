namespace projectPartiuDestino.Models
{
    /// <summary>
    /// Representa a escolha de voo (classe/assento) feita pelo usuário na etapa
    /// "Passagem", antes de escolher a hospedagem. É guardada temporariamente na
    /// Session (chave Voo_pacote_{id}) e consumida pelo CarrinhoController.
    /// </summary>
    public class SelecaoVoo
    {
        public int ItemId { get; set; }
        public string TipoItem { get; set; } = "pacote"; // "pacote" | "destino" (futuro)
        public string ClasseViagem { get; set; } = "Econômica"; // Econômica | Executiva | Primeira Classe
        public string TipoAssento { get; set; } = "Janela";     // Janela | Corredor | Meio
        public string? NumeroAssento { get; set; }
        public string? CompanhiaAerea { get; set; }
        public string? HorarioIda { get; set; }
        public string? HorarioVolta { get; set; }
        public decimal PrecoAdicional { get; set; } = 0.00m;
    }
}