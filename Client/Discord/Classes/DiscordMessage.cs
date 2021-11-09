///////////////////////////////////////////////////////////////////
// This file contains modified code from 'CSharpDiscordWebhook'
// https://github.com/N4T4NM/CSharpDiscordWebhook
// which is released under MIT License.
// https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE
// Copyright 2021 N4T4NM
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RCC.Discord.Classes
{
    public class DiscordMessage
    {
        public DiscordMessage()
        {
            Embeds = new List<DiscordEmbed>();
        }

        [JsonPropertyName("content")]
        /// <summary>
        /// Message content
        /// </summary>
        public string Content { get; set; }

        [JsonPropertyName("tts")]
        /// <summary>
        /// Read message to everyone on the channel
        /// </summary>
        public bool TTS { get; set; }

        [JsonPropertyName("username")]
        /// <summary>
        /// Webhook profile username to be shown
        /// </summary>
        public string Username { get; set; }

        [JsonPropertyName("avatar_url")]
        /// <summary>
        /// Webhook profile avater to be shown
        /// </summary>
        public string AvatarUrl { get; set; }

        [JsonPropertyName("embeds")]
        /// <summary>
        /// List of embeds
        /// </summary>
        public List<DiscordEmbed> Embeds { get; set; }
    }
}
