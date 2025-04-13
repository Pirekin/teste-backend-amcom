using Questao5.Domain.Entities;

namespace Questao5.Infrastructure.Repositories.Interfaces
{
    public interface IContaCorrenteDapperRepository
    {
        Task<ContaCorrente> ObterPorID(string id);
        Task<decimal> ObterSaldo(string id);
    }
}
