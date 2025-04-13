using MediatR;

using Questao5.Application.Exceptions;
using Questao5.Application.Queries.Responses;
using Questao5.Domain.Enumerators;
using Questao5.Infrastructure.Repositories.Interfaces;

namespace Questao5.Application.Handlers;

public class ObterSaldoQueryHandler : IRequestHandler<ObterSaldoQuery, ObterSaldoResponse>
{
    private readonly IContaCorrenteDapperRepository _contaCorrenteDapperRepository;

    public ObterSaldoQueryHandler(IContaCorrenteDapperRepository contaCorrenteDapperRepository)
    {
        _contaCorrenteDapperRepository = contaCorrenteDapperRepository;
    }

    public async Task<ObterSaldoResponse> Handle(ObterSaldoQuery request, CancellationToken cancellationToken)
    {
        var conta = await _contaCorrenteDapperRepository.ObterPorID(request.IdContaCorrente);

        if (conta == null)
            throw new BusinessException(BusinessErrorType.INVALID_ACCOUNT, "Conta corrente inválida.");

        if (conta.ativo != 1)
            throw new BusinessException(BusinessErrorType.INACTIVE_ACCOUNT, "Conta corrente inativa.");

        var saldo = await _contaCorrenteDapperRepository.ObterSaldo(request.IdContaCorrente);

        return new ObterSaldoResponse() { NomeCorrentista = conta.nome, NumeroConta = conta.numero, Saldo = saldo, DataHora = DateTime.Now };
    }

}