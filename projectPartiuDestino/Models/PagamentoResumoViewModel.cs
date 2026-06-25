namespace projectPartiuDestino.Models
{
    public class PagamentoResumoViewModel
    {
      
            public List<CarrinhoItem> Itens { get; set; } = new();

            public decimal Total { get; set; }

            public string CodigoReserva { get; set; } = string.Empty;

            public string FormaPagamento { get; set; } = "Cartao";

            public int Parcelas { get; set; } = 1;

            public string? NomeTitular { get; set; }

            public string? DocumentoTitular { get; set; }

            public string? NumeroCartao { get; set; }

            public string? Vencimento { get; set; }

            public string? Cvv { get; set; }
        }
    }

