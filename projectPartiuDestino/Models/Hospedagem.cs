namespace projectPartiuDestino.Models
{
    public class Hospedagem
    {
        public int Id { get; set; }
        public int PacoteId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public string? Descricao { get; set; }
        public string? Endereco { get; set; }
        public string? ImagemUrl { get; set; }

        // Não vem do banco diretamente — populado manualmente nos controllers
        public List<Quarto> Quartos { get; set; } = new();
    }
}