namespace projectPartiuDestino.Models
{
    public class Destinos
    {
        public int Id { get; set; }
        public string OrigemPais { get; set; }
        public string OrigemEstado { get; set; }
        public string Pais { get; set; }
        public string Estado { get; set; }
        public string ImagemUrl { get; set; }
        public decimal PrecoPorPessoa { get; set; }

        // NOVO — hospedagens próprias da Passagem (opcional)
        public List<Hospedagem> Hospedagens { get; set; } = new();
    }
}