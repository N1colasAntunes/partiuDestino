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
        public Destino Destino { get; set; }
    }
}