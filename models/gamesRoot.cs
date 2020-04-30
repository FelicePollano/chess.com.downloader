using System.Text.Json.Serialization;

namespace chess.com.downloader
{
    class GamesRoot
    {
        [JsonPropertyName("games")]
        public Game[] Games { get; set; }
    }
    class Game
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("Pgn")]
        public string Pgn { get; set; }
    }
}