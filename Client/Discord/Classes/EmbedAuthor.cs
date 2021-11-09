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
    public class EmbedAuthor
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        /// <summary>
        /// Author name
        /// </summary>
        public string Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("url")]
        /// <summary>
        /// Author url
        /// </summary>
        public string Url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("icon_url")]
        /// <summary>
        /// Author icon
        /// </summary>
        public string IconUrl { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("proxy_icon_url")]
        /// <summary>
        /// Author icon proxy
        /// </summary>
        public string ProxyIconUrl { get; set; }
    }
}
