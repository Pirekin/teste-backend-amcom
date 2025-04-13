using Questao5.Domain.Entities;
using Questao5.Infrastructure.Repositories.Interfaces;

namespace Questao5.Infrastructure.Services
{
    public class ContaCorrenteService : IContaCorrenteService
    {
        private readonly IContaCorrenteDapperRepository _contaCorrenteDapperRepository;
        public ContaCorrenteService(IContaCorrenteDapperRepository contaCorrenteDapperRepository)
        {
            _contaCorrenteDapperRepository = contaCorrenteDapperRepository;
        }

        public async Task<string> MovimentarConta(MovimentacaoRequest movimentacaoRequest)
        {
           var corrente =  await _contaCorrenteDapperRepository.ObterPorID(movimentacaoRequest.IdContaCorrente);
            return corrente.idcontacorrente;
        }
    }
}
