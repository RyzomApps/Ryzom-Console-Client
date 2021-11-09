///////////////////////////////////////////////////////////////////
// This file contains modified code from 'CSharpDiscordWebhook'
// https://github.com/N4T4NM/CSharpDiscordWebhook
// which is released under MIT License.
// https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE
// Copyright 2021 N4T4NM
///////////////////////////////////////////////////////////////////

using System.Text.Json.Serialization;

namespace RCC.Discord.Classes
{
    public class EmbedMedia
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("url")]
        /// <summary>
        /// Media url
        /// </summary>
        public string Url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("proxy_url")]
        /// <summary>
        /// Media proxy url
        /// </summary>
        public string ProxyUrl { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("height")]
        /// <summary>
        /// Media height
        /// </summary>
        public int? Height { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("width")]
        /// <summary>
        /// Media width
        /// </summary>
        public int? Width { get; set; }
    }
}
