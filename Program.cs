using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace chess.com.downloader
{
    class Program
    {
        static string savePath="/var/chess-games/pgn";
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

            if(args.Length == 0 )
            {
                Log.Error("At least the user is required");
                Environment.Exit(1);
            }
            if(args.Length > 1) //not a smart command line parser, but a KISS one...
                savePath = args[1];
            await DownloadAllGamesFor(args[0]);
        }

        private static async Task DownloadAllGamesFor(string user)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", "github-com_FelicePollano_chess-com-downloader");
            var archivesroot = await JsonSerializer.DeserializeAsync<ArchivesRoot>( await client.GetStreamAsync($"https://api.chess.com/pub/player/{user}/games/archives"));
            Log.Information($"user {user} has {archivesroot.Archives.Length} archives to download" );
            var tasks = archivesroot.Archives.Select(u=>DownloadGamesFor(user,client,u));
            await Task.WhenAll(tasks);
        }
        private static async Task DownloadGamesFor(string user,HttpClient client,string uri)
        {
            Log.Information($"downloading from url {uri}");
            var games = await JsonSerializer.DeserializeAsync<GamesRoot>( await client.GetStreamAsync(uri));
            Log.Information($"{games.Games.Length} games found so far...");
            
            var savingTasks = games.Games.Select(t=>{
                var path = Path.Combine(savePath,Path.GetFileName(t.Url));
                Log.Information($"trying to save game in:{path}");
                using ( var fs = new FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite) )
                using( var sr = new StreamWriter(fs))
                {
                    return sr.WriteAsync(t.Pgn);
                }
            });
            await Task.WhenAll(savingTasks);
        }
    }
}
