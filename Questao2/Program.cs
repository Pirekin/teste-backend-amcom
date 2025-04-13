using Newtonsoft.Json;
using Questao2;

using System.Text;

public class Program
{

    // TODO: Refatorar

    static readonly HttpClient _client = new HttpClient();
    static readonly string _url = "https://jsonmock.hackerrank.com/api/football_matches";
    public static async Task Main()
    {

        string teamName = "Paris Saint-Germain";
        int year = 2013;
        int totalGolsEmCasa = await getTotalScoredGoals(teamName, year);
        int totalGolsVisitante = await getTotalScoredGoals(teamName, year, visitante: true);

        Console.WriteLine("Team "+ teamName +" scored "+ (totalGolsEmCasa + totalGolsVisitante).ToString() + " goals in "+ year);

        teamName = "Chelsea";
        year = 2014;
        totalGolsEmCasa = await getTotalScoredGoals(teamName, year);
        totalGolsVisitante = await getTotalScoredGoals(teamName, year, visitante: true);

        Console.WriteLine("Team " + teamName + " scored " + (totalGolsEmCasa + totalGolsVisitante).ToString() + " goals in " + year);

        // Output expected:
        // Team Paris Saint - Germain scored 109 goals in 2013
        // Team Chelsea scored 92 goals in 2014
    }

    public static async Task<int> getTotalScoredGoals(string team, int year, bool visitante = false, int page = 1, int totalGols = 0)
    {
        var parametrosRequest = new ParametrosApiPartida
        {
            page = page,
            year = year,
            team1 = !visitante ? team : "",
            team2 = visitante ? team : ""
        };

        ApiPartidasResponse responsePartidas = await obterDadosPartidas(parametrosRequest);

        totalGols += responsePartidas.data.Sum(p => int.Parse(visitante ? p.team2goals : p.team1goals));

        if (page >= responsePartidas.total_pages)
            return totalGols;

        //Chamada recursiva devivo o numero de páginas.
        return await getTotalScoredGoals(team, year, visitante, parametrosRequest.page + 1, totalGols);
    }

    public static async Task<ApiPartidasResponse> obterDadosPartidas(ParametrosApiPartida parametrosApiPartida)
    {
        try
        {
            var builder = new UriBuilder(_url);
            var query = new StringBuilder();

            if (parametrosApiPartida.page > 0)
                query.Append($"&page={parametrosApiPartida.page}");

            if (parametrosApiPartida.year > 0)
                query.Append($"&year={parametrosApiPartida.year}");

            if (!string.IsNullOrWhiteSpace(parametrosApiPartida.team1))
                query.Append($"&team1={Uri.EscapeDataString(parametrosApiPartida.team1)}");

            if (!string.IsNullOrWhiteSpace(parametrosApiPartida.team2))
                query.Append($"&team2={Uri.EscapeDataString(parametrosApiPartida.team2)}");

            builder.Query = query.ToString();

            HttpResponseMessage responseApi = await _client.GetAsync(builder.Uri);

            string responseJson = await responseApi.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiPartidasResponse>(responseJson);
        }
        catch (HttpRequestException error)
        {
            throw new HttpRequestException("Falha na requisição HTTP ao buscar os dados.", error);
        }
    }

}