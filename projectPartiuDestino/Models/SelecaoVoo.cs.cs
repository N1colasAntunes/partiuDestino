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
    }
}