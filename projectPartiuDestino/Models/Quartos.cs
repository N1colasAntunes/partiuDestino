namespace projectPartiuDestino.Models
{
    public class Quarto
    {
        public int Id { get; set; }
        public int HospedagemId { get; set; }
        public string TipoQuarto { get; set; } = string.Empty;
        public int CapacidadeAdultos { get; set; }
        public int CapacidadeCriancas { get; set; }
        public decimal PrecoAdicional { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public string? Comodidades { get; set; }
        public string? ImagemUrl { get; set; }
    }
}