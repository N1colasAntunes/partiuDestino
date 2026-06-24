namespace projectPartiuDestino.Models
{
    public class ViagemPersonalizada
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        // Identificação pessoal
        public string NomeCompleto { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Whatsapp { get; set; }

        // Destino & Logística
        public string Origem { get; set; }
        public string Destino { get; set; }
        public string RegiaoInteresse { get; set; }
        public DateTime? DataPartida { get; set; }
        public int? DuracaoDias { get; set; }
        public string Transporte { get; set; }

        // Hospedagem
        public string TipoHospedagem { get; set; }
        public string CategoriaHospedagem { get; set; }
        public string PreferenciasHospedagem { get; set; }

        // Perfil dos viajantes
        public int Adultos { get; set; }
        public int Criancas { get; set; }
        public int Idosos { get; set; }
        public string TipoGrupo { get; set; }

        // Estilo da experiência
        public string ObjetivoViagem { get; set; }
        public string RitmoViagem { get; set; }
        public string ClimaViagem { get; set; }

        // Orçamento
        public string FaixaOrcamento { get; set; }

        // Desejos especiais
        public string DesejosEspeciais { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}