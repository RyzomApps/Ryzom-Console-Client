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

namespace RCC.Discord.Classes
{
    public class DiscordEmbed
    {
        public DiscordEmbed()
        {
            Fields = new List<EmbedField>();
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("title")]
        /// <summary>
        /// Embed title
        /// </summary>
        public string Title { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("description")]
        /// <summary>
        /// Embed description
        /// </summary>
        public string Description { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("url")]
        /// <summary>
        /// Embed url
        /// </summary>
        public string Url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        /// <summary>
        /// Embed timestamp
        /// </summary>
        public DateTime? Timestamp
        {
            get => DateTime.Parse(StringTimestamp);
            set => StringTimestamp = value?.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("timestamp")]
        public string StringTimestamp { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        /// <summary>
        /// Embed color
        /// </summary>
        public Color? Color
        {
            get => HexColor.ToColor();
            set => HexColor = value.ToHex();
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("color")]
        public int? HexColor { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("footer")]
        /// <summary>
        /// Embed footer
        /// </summary>
        public EmbedFooter Footer { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("image")]
        /// <summary>
        /// Embed image
        /// </summary>
        public EmbedMedia Image { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("thumbnail")]
        /// <summary>
        /// Embed thumbnail
        /// </summary>
        public EmbedMedia Thumbnail { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("video")]
        /// <summary>
        /// Embed video
        /// </summary>
        public EmbedMedia Video { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("provider")]
        /// <summary>
        /// Embed provider
        /// </summary>
        public EmbedProvider Provider { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("author")]
        /// <summary>
        /// Embed author
        /// </summary>
        public EmbedAuthor Author { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("fields")]
        /// <summary>
        /// Embed fields list
        /// </summary>
        public List<EmbedField> Fields { get; set; }
    }
}
