using MediatR;

using Questao5.Domain.Enumerators;

namespace Questao5.Application.Commands.Requests
{
    public class MovimentacaoCommand : IRequest<string>
    {
        public string IdRequisicao { get; set; }
        public string IdContaCorrente { get; set; }
        public decimal Valor { get; set; }
        public TipoMovimento TipoMovimento { get; set; }
    }
}
