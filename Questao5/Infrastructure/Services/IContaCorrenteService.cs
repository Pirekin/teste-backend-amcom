using Questao5.Domain.Entities;

namespace Questao5.Infrastructure.Services
{
    public interface IContaCorrenteService
    {
        Task<string> MovimentarConta(MovimentacaoRequest movimentacaoRequest);
    }
}
