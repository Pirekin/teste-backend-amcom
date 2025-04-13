using Dapper;
using Questao5.Domain.Entities;
using Questao5.Infrastructure.Repositories.Interfaces;

using System.Data;

namespace Questao5.Infrastructure.Repositories
{
    public class MovimentoDapperRepository : IMovimentoDapperRepository
    {
        private readonly IDbConnection _dbConnection;
        public MovimentoDapperRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<string> Movimentar(MovimentacaoRequest movimentacaoRequest)
        {
            string query = @"
                INSERT INTO movimento (idmovimento, datamovimento, idcontacorrente, valor, tipomovimento)
                VALUES (@IdMovimento, DATETIME('now'), @IdContaCorrente, @Valor, @TipoMovimento);";

            await _dbConnection.ExecuteAsync(query, new
            {
                IdMovimento = movimentacaoRequest.Idmovimento,
                IdRequisicao = movimentacaoRequest.IdRequisicao,
                IdContaCorrente = movimentacaoRequest.IdContaCorrente,
                Valor = movimentacaoRequest.Valor,
                TipoMovimento = movimentacaoRequest.TipoMovimento.ToString()
            });

            return movimentacaoRequest.Idmovimento;
        }
    }
}
