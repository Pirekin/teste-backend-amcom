namespace Questao5.Infrastructure.Repositories.Interfaces
{
    public interface IIdempotenciaRepository
    {
        Task<string> ObterResultado(string idRequisicao);
        Task RegistrarResultado(string chave, string Idrequisicao, string resultado);
    }
}
