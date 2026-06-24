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

        public int? NumeroCamas { get; set; }

        public string? TipoCamas { get; set; }

        public bool CafeIncluso { get; set; }

        public decimal? AreaM2 { get; set; }

        public string? Descricao { get; set; }

        public string? PoliticaCancelamento { get; set; }
    }
}