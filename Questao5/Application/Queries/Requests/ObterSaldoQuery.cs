using MediatR;

using Questao5.Application.Queries.Responses;

public class ObterSaldoQuery : IRequest<ObterSaldoResponse>
{
    public string IdContaCorrente { get; set; }
}