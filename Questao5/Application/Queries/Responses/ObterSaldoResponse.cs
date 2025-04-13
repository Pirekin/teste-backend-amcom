namespace Questao5.Application.Queries.Responses
{
    public class ObterSaldoResponse
    {
        public uint NumeroConta { get; set; }
        public string NomeCorrentista { get; set; }
        public DateTime DataHora { get; set; }
        public decimal Saldo { get; set; }

    }
}
