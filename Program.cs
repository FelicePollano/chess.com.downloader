using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace chess.com.downloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
           
            await DownloadAllGamesFor(args[0]);
        }

        private static async Task DownloadAllGamesFor(string user)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", "https://github.com/FelicePollano/chess-com-downloader");
            var archivesroot = JsonSerializer.DeserializeAsync<ArchivesRoot>( await client.GetStreamAsync($"https://api.chess.com/pub/player/{user}/games/archives")); 
        }
    }
}
