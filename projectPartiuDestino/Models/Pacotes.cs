namespace projectPartiuDestino.Models
{
    public class Pacotes
    {
        public int Id { get; set; }
        public int DestinoId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string TipoViagem { get; set; }
        public int DuracaoDias { get; set; }
        public DateTime DataPartida { get; set; }
        public DateTime DataRetorno { get; set; }
        public decimal PrecoPorPessoa { get; set; }
        public int VagasDisponiveis { get; set; }
        public string? ImagemUrl { get; set; }
        public Destinos Destino { get; set; }

        // ADICIONADO — usado apenas na tela de Detalhes do Pacote
        public List<Hospedagem> Hospedagens { get; set; } = new();
        public string? VooCompanhiaAerea { get; set; }

        public string? VooTitulo { get; set; }

        public string? VooDescricao { get; set; }

        public string? VooAeroportoOrigem { get; set; }

        public string? VooAeroportoDestino { get; set; }

        public string? VooHorarioIda { get; set; }

        public string? VooHorarioVolta { get; set; }

        public string? VooDuracaoMedia { get; set; }

        public string? VooBagagemInclusa { get; set; }

        public string? VooTipoTarifa { get; set; }

        public string? VooEscala { get; set; }

        public decimal VooPrecoAdicionalPorPessoa { get; set; }
    }
}