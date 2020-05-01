using System;
using System.Collections.Generic;
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
            Stream stream;
            for(;;)
            {
                try{
                    stream = await client.GetStreamAsync(uri);
                    break;
                }
                catch(HttpRequestException e)
                {
                    if(e.Message.Contains("Too Many"))
                        await Task.Delay(20000);
                    else
                        throw;
                }
            }

            Log.Information($"downloading from url {uri}");
            var games = await JsonSerializer.DeserializeAsync<GamesRoot>( stream);
            Log.Information($"{games.Games.Length} games found so far...");
            List<FileStream> opened = new List<FileStream>();
            List<StreamWriter> writers = new List<StreamWriter>();
            var savingTasks = games.Games.Select( async t=> {
                var path = Path.ChangeExtension(Path.Combine(savePath,Path.GetFileName(t.Url)),"pgn");
                Log.Information($"trying to save game in:{path}");
                using(var fs = new FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None,2048,true))
                using(var sr = new StreamWriter(fs))
                {
                    await sr.WriteAsync(t.Pgn);
                }
                Log.Information($"{path} saved.");
            });
            try{
                await Task.WhenAll(savingTasks);
            }
            catch(Exception e)
            {
                Log.Fatal($"Cannot write to file:{e}");
                throw;
            }
        }
    }
}
