///////////////////////////////////////////////////////////////////
// This file contains modified code from 'CSharpDiscordWebhook'
// https://github.com/N4T4NM/CSharpDiscordWebhook
// which is released under MIT License.
// https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE
// Copyright 2021 N4T4NM
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Client.Discord.Classes
{
    public class DiscordMessage
    {
        public DiscordMessage()
        {
            Embeds = new List<DiscordEmbed>();
        }

        /// <summary>
        /// Message content
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// Read message to everyone on the channel
        /// </summary>
        [JsonPropertyName("tts")]
        public bool Tts { get; set; }

        /// <summary>
        /// Webhook profile username to be shown
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }

        /// <summary>
        /// Webhook profile avater to be shown
        /// </summary>
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// List of embeds
        /// </summary>
        [JsonPropertyName("embeds")]
        public List<DiscordEmbed> Embeds { get; set; }
    }
}
