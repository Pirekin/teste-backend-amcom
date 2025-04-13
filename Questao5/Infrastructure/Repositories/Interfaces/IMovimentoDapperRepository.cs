using Questao5.Domain.Entities;

namespace Questao5.Infrastructure.Repositories.Interfaces
{
    public interface IMovimentoDapperRepository
    {
        Task<string> Movimentar(MovimentacaoRequest movimentacaoRequest);
    }
}
