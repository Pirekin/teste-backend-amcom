using Dapper;

using Questao5.Domain.Entities;
using Questao5.Infrastructure.Repositories.Interfaces;

using System.Data;

namespace Questao5.Infrastructure.Repositories
{
    public class ContaCorrenteDapperRepository : IContaCorrenteDapperRepository
    {
        private readonly IDbConnection _dbConnection;
        public ContaCorrenteDapperRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<ContaCorrente> ObterPorID(string id)
        {
            string query = "SELECT * FROM contacorrente WHERE idcontacorrente = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<ContaCorrente>(query, new { Id = id });
        }

        public async Task<decimal> ObterSaldo(string id)
        {
            string query = @"SELECT
                                 IFNULL(SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE -valor END), 0) AS saldo
                            FROM
                                movimento
                            WHERE
                                idcontacorrente = @IdContaCorrente;";

            return await _dbConnection.QueryFirstOrDefaultAsync<decimal>(query, new { @IdContaCorrente = id });
        }
    }
}
