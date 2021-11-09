///////////////////////////////////////////////////////////////////
// This file contains modified code from 'CSharpDiscordWebhook'
// https://github.com/N4T4NM/CSharpDiscordWebhook
// which is released under MIT License.
// https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE
// Copyright 2021 N4T4NM
///////////////////////////////////////////////////////////////////

using System.Text.Json.Serialization;

namespace Client.Discord.Classes
{
    public class EmbedField
    {
        [JsonPropertyName("name")]
        /// <summary>
        /// Field name
        /// </summary>
        public string Name { get; set; }

        [JsonPropertyName("value")]
        /// <summary>
        /// Field value
        /// </summary>
        public string Value { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("inline")]
        /// <summary>
        /// Field align
        /// </summary>
        public bool? InLine { get; set; }
    }
}
