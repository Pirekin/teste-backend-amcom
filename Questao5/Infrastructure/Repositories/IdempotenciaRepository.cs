using Dapper;

using Questao5.Infrastructure.Repositories.Interfaces;

using System.Data;

namespace Questao5.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly IDbConnection _dbConnection;
        public IdempotenciaRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<string> ObterResultado(string chave)
        {
            var query = "SELECT resultado FROM idempotencia WHERE chave_idempotencia = @chave";
            return await _dbConnection.ExecuteScalarAsync<string>(query, new { chave });
        }

        public async Task RegistrarResultado(string chave, string requisicao, string resultado)
        {
            var query = @"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                      VALUES (@chave, @requisicao, @resultado)";
            await _dbConnection.ExecuteAsync(query, new { chave, requisicao, resultado });
        }
    }
}
