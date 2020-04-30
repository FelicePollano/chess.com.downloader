using System.Text.Json.Serialization;

namespace chess.com.downloader
{
    class ArchivesRoot
    {
        [JsonPropertyName("archives")]
        public string[] Archives { get; set; }
    }
}