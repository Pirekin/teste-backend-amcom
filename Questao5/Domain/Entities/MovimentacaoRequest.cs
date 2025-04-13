using Questao5.Domain.Enumerators;

namespace Questao5.Domain.Entities
{
    public class MovimentacaoRequest
    {
        public string Idmovimento { get; set; }
        public string IdRequisicao { get; set; }
        public string IdContaCorrente { get; set; }
        public decimal Valor { get; set; }
        public TipoMovimento TipoMovimento { get; set; }

    }
}
