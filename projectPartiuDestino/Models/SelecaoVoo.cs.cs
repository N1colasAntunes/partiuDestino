namespace projectPartiuDestino.Models
{
    public class SelecaoVoo
    {
        public int ItemId { get; set; }

        public string TipoItem { get; set; } = "pacote";

        public string ClasseViagem { get; set; } = "Econômica";

        public string TipoAssento { get; set; } = "Múltiplos";

        public string? NumeroAssento { get; set; }

        public string? CompanhiaAerea { get; set; }

        public string? HorarioIda { get; set; }

        public string? HorarioVolta { get; set; }

        public decimal PrecoAdicional { get; set; } = 0.00m;

        public int QuantidadeAdultos { get; set; } = 1;

        public int QuantidadeCriancas { get; set; } = 0;

        public int QuantidadeTotal { get; set; } = 1;

        public string? TituloVoo { get; set; }

        public string? DescricaoVoo { get; set; }

        public string? AeroportoOrigem { get; set; }

        public string? AeroportoDestino { get; set; }

        public string? DuracaoMedia { get; set; }

        public string? BagagemInclusa { get; set; }

        public string? TipoTarifa { get; set; }

        public string? Escala { get; set; }
        public string? CidadeOrigem { get; set; }

        public string? CidadeDestino { get; set; }
    }
}