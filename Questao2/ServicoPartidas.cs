namespace Questao2
{
    public class ServicoPartidas: IServicoPartidas
    {
        private readonly HttpClient _httpClient;
        private readonly string _url = "https://jsonmock.hackerrank.com/api/football_matches";
        public ServicoPartidas(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ObterDadosPartidas()
        {
            try
            {
                HttpResponseMessage retorno =  await _httpClient.GetAsync(this._url);
                return "";
            }
            catch (HttpRequestException error)
            {
                return $"Erro ao buscar dados da partida: {error.Message}";
            }
        }
    }
}
