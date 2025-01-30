///////////////////////////////////////////////////////////////////
// This file contains modified code from 'CSharpDiscordWebhook'
// https://github.com/N4T4NM/CSharpDiscordWebhook
// which is released under MIT License.
// https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE
// Copyright 2021 N4T4NM
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net;
using System.Text.Json;
using Client.Discord.Classes;

namespace Client.Discord
{
    public class DiscordWebhook
    {
        /// <summary>
        /// Webhook url
        /// </summary>
        public string Url { get; set; }

        private static void AddField(MemoryStream stream, string bound, string cDisposition, string cType, byte[] data)
        {
            var prefix = stream.Length > 0 ? "\r\n--" : "--";
            var fBegin = $"{prefix}{bound}\r\n";

            stream.Write(fBegin);
            stream.Write(cDisposition);
            stream.Write(cType);
            stream.Write(data);
        }

        private static void SetJsonPayload(MemoryStream stream, string bound, string json)
        {
            const string cDisposition = "Content-Disposition: form-data; name=\"payload_json\"\r\n";
            const string cType = "Content-Type: application/octet-stream\r\n\r\n";
            AddField(stream, bound, cDisposition, cType, json.Encode());
        }

        private static void SetFile(MemoryStream stream, string bound, int index, FileInfo file)
        {
            var cDisposition = $"Content-Disposition: form-data; name=\"file_{index}\"; filename=\"{file.Name}\"\r\n";
            const string cType = "Content-Type: application/octet-stream\r\n\r\n";
            AddField(stream, bound, cDisposition, cType, File.ReadAllBytes(file.FullName));
        }

        /// <summary>
        /// Send webhook message
        /// </summary>
        public void Send(DiscordMessage message, params FileInfo[] files)
        {
            if (string.IsNullOrEmpty(Url))
                throw new NullReferenceException(@"Invalid Webhook URL.");

            var bound = $"------------------------{DateTime.Now.Ticks:x}";

            var webhook = new WebClient();
            webhook.Headers.Add("Content-Type", $"multipart/form-data; boundary={bound}");

            var stream = new MemoryStream();

            for (var i = 0; i < files.Length; i++)
                SetFile(stream, bound, i, files[i]);

            var json = JsonSerializer.Serialize(message);
            SetJsonPayload(stream, bound, json);
            stream.Write($"\r\n--{bound}--");

            try
            {
                webhook.UploadData(Url, stream.ToArray());
            }
            catch (WebException ex)
            {
                throw new WebException(ex.Response.GetResponseStream().Decode());
            }

            stream.Dispose();
        }
    }
}
