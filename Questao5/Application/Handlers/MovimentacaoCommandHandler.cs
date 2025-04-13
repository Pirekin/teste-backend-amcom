using MediatR;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Exceptions;
using Questao5.Domain.Entities;
using Questao5.Domain.Enumerators;
using Questao5.Infrastructure.Repositories.Interfaces;
using System.Data;
using System.Text.Json;

namespace Questao5.Application.Handlers;

public class MovimentacaoCommandHandler : IRequestHandler<MovimentacaoCommand, string>
{
    private readonly IMovimentoDapperRepository _movimentoDapperRepository;
    private readonly IContaCorrenteDapperRepository _contaCorrenteDapperRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;
    private readonly IDbConnection _dbConnection;
    public MovimentacaoCommandHandler(
        IMovimentoDapperRepository movimentoDapperRepository,
        IContaCorrenteDapperRepository contaCorrenteDapperRepository,
        IIdempotenciaRepository idempotenciaRepository,
        IDbConnection dbConnection)
    {
        _movimentoDapperRepository = movimentoDapperRepository;
        _contaCorrenteDapperRepository = contaCorrenteDapperRepository;
        _idempotenciaRepository = idempotenciaRepository;
        _dbConnection = dbConnection;
    }
    public async Task<string> Handle(MovimentacaoCommand request, CancellationToken cancellationToken)
    {
        using var transaction = _dbConnection.BeginTransaction();

        try
        {
            ValidarRequest(request);

            var registroExistente = await VerificarIdempotencia(request);
            if (registroExistente != null)
                return registroExistente;

            await ValidarConta(request.IdContaCorrente);

            var movimento = CriarMovimento(request);

            var resultadoMovimentacao = await _movimentoDapperRepository.Movimentar(movimento);

            await RegistrarIdempotencia(request, resultadoMovimentacao);

            transaction.Commit();

            return resultadoMovimentacao;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    private void ValidarRequest(MovimentacaoCommand request)
    {
        if (request.Valor <= 0)
            throw new BusinessException(BusinessErrorType.INVALID_VALUE, "O valor da movimentação deve ser positivo.");

        if (request.TipoMovimento != TipoMovimento.C && request.TipoMovimento != TipoMovimento.D)
            throw new BusinessException(BusinessErrorType.INVALID_TYPE, "Tipo de movimentação inválido.");
    }

    private async Task<string?> VerificarIdempotencia(MovimentacaoCommand request)
    {
        return await _idempotenciaRepository.ObterResultado(request.IdRequisicao);
    }

    private async Task ValidarConta(string idContaCorrente)
    {
        var conta = await _contaCorrenteDapperRepository.ObterPorID(idContaCorrente);

        if (conta == null)
            throw new BusinessException(BusinessErrorType.INVALID_ACCOUNT, "Conta corrente inválida.");

        if (conta.ativo != 1)
            throw new BusinessException(BusinessErrorType.INACTIVE_ACCOUNT, "Conta corrente inativa.");
    }

    private MovimentacaoRequest CriarMovimento(MovimentacaoCommand request)
    {
        return new MovimentacaoRequest
        {
            Idmovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrente,
            IdRequisicao = request.IdRequisicao,
            TipoMovimento = request.TipoMovimento,
            Valor = request.Valor
        };
    }

    private async Task RegistrarIdempotencia(MovimentacaoCommand request, string resultado)
    {
        await _idempotenciaRepository.RegistrarResultado(
            request.IdRequisicao,
            JsonSerializer.Serialize(request),
            resultado
        );
    }
}
