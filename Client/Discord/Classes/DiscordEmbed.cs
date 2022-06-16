///////////////////////////////////////////////////////////////////
// This file contains modified code from 'CSharpDiscordWebhook'
// https://github.com/N4T4NM/CSharpDiscordWebhook
// which is released under MIT License.
// https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE
// Copyright 2021 N4T4NM
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Client.Discord.Classes
{
    public class DiscordEmbed
    {
        public DiscordEmbed()
        {
            Fields = new List<EmbedField>();
        }

        /// <summary>
        /// Embed title
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Embed description
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Embed url
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Embed timestamp
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public DateTime? Timestamp
        {
            get => DateTime.Parse(StringTimestamp);
            set => StringTimestamp = value?.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("timestamp")]
        public string StringTimestamp { get; private set; }

        /// <summary>
        /// Embed color
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Color? Color
        {
            get => HexColor.ToColor();
            set => HexColor = value.ToHex();
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("color")]
        public int? HexColor { get; private set; }

        /// <summary>
        /// Embed footer
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("footer")]
        public EmbedFooter Footer { get; set; }

        /// <summary>
        /// Embed image
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("image")]
        public EmbedMedia Image { get; set; }

        /// <summary>
        /// Embed thumbnail
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("thumbnail")]
        public EmbedMedia Thumbnail { get; set; }

        /// <summary>
        /// Embed video
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("video")]
        public EmbedMedia Video { get; set; }

        /// <summary>
        /// Embed provider
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("provider")]
        public EmbedProvider Provider { get; set; }

        /// <summary>
        /// Embed author
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("author")]
        public EmbedAuthor Author { get; set; }

        /// <summary>
        /// Embed fields list
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("fields")]
        public List<EmbedField> Fields { get; set; }
    }
}
