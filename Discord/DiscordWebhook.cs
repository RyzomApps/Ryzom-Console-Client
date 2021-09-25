///////////////////////////////////////////////////////////////////
// This file contains modified code from 'CSharpDiscordWebhook'
// https://github.com/N4T4NM/CSharpDiscordWebhook
// which is released under MIT License.
// https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE
// Copyright 2021 N4T4NM
///////////////////////////////////////////////////////////////////

using RCC.Discord.Classes;
using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace RCC.Discord
{
    public class DiscordWebhook
    {
        /// <summary>
        /// Webhook url
        /// </summary>
        public string Url { get; set; }

        private void AddField(MemoryStream stream, string bound, string cDisposition, string cType, byte[] data)
        {
            string prefix = stream.Length > 0 ? "\r\n--" : "--";
            string fBegin = $"{prefix}{bound}\r\n";

            stream.Write(fBegin);
            stream.Write(cDisposition);
            stream.Write(cType);
            stream.Write(data);
        }

        private void SetJsonPayload(MemoryStream stream, string bound, string json)
        {
            string cDisposition = "Content-Disposition: form-data; name=\"payload_json\"\r\n";
            string cType = "Content-Type: application/octet-stream\r\n\r\n";
            AddField(stream, bound, cDisposition, cType, json.Encode());
        }

        private void SetFile(MemoryStream stream, string bound, int index, FileInfo file)
        {
            string cDisposition = $"Content-Disposition: form-data; name=\"file_{index}\"; filename=\"{file.Name}\"\r\n";
            string cType = "Content-Type: application/octet-stream\r\n\r\n";
            AddField(stream, bound, cDisposition, cType, File.ReadAllBytes(file.FullName));
        }

        /// <summary>
        /// Send webhook message
        /// </summary>
        public void Send(DiscordMessage message, params FileInfo[] files)
        {
            if (string.IsNullOrEmpty(Url))
                throw new ArgumentNullException("Invalid Webhook URL.");

            string bound = "------------------------" + DateTime.Now.Ticks.ToString("x");

            WebClient webhook = new WebClient();
            webhook.Headers.Add("Content-Type", "multipart/form-data; boundary=" + bound);

            MemoryStream stream = new MemoryStream();
            for (int i = 0; i < files.Length; i++)
                SetFile(stream, bound, i, files[i]);

            string json = JsonSerializer.Serialize(message);
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
