using RCC.Discord;
using RCC.Discord.Classes;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RCC.Logger
{
    internal class DiscordLogger : FilteredLogger
    {
        /// <summary>
        /// Discord Webhook class for sending messages
        /// </summary>
        private readonly DiscordWebhook _hook;

        /// <summary>
        /// Queue of outgoing messages to respect the rate limit
        /// </summary>
        private readonly Queue<string> _messageQueue = new Queue<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public DiscordLogger(string webhookUrl)
        {
            _hook = new DiscordWebhook
            {
                Url = webhookUrl
            };

            var messageFlusher = new Thread(FlushMessages) { Name = "RCC DiscordLogger MessageFlusher" };
            messageFlusher.Start();

            QueueMessage("### Log started at " + FileLogLogger.GetTimestamp() + " ###");
        }

        /// <summary>
        /// Sends all pending messages to the discord guild
        /// </summary>
        /// <remarks>rate limit for sending messages is 5 messages per 5 seconds per channel</remarks>
        private void FlushMessages()
        {
            while (true)
            {
                Thread.Sleep(1200); // 1 message per 1 second + tolerance

                try
                {
                    if (_messageQueue.Count == 0)
                        continue;

                    var text = "";

                    while (_messageQueue.Count > 0 && text.Length < 2000 - 256)
                    {
                        text += _messageQueue.Dequeue() + "\r\n";
                    }

                    text = text[..^2];

                    var message = new DiscordMessage
                    {
                        Content = text,
                        Username = "RCC",
                    };

                    _hook.Send(message);
                }
                catch (Exception e)
                {
                    // Must use base since we already failed to write log
                    base.Error("Cannot write to webhook: " + e.Message);
                    base.Debug("Stack trace: \n" + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Queues a message for sending and also outputs it to console
        /// </summary>
        private void LogAndSave(string msg)
        {
            QueueMessage(msg);
            Log(msg);
        }

        /// <summary>
        /// Add a message to the send queue
        /// </summary>
        private void QueueMessage(string text)
        {
            _messageQueue.Enqueue(text);
        }

        /// <inheritdoc />
        public override void Chat(string msg)
        {
            if (!ChatEnabled) return;
            if (ShouldDisplay(FilterChannel.Chat, msg))
            {
                LogAndSave(msg);
            }
            else Debug("[Logger] One Chat message filtered: " + msg);
        }

        /// <inheritdoc />
        public override void Debug(string msg)
        {
            if (!DebugEnabled) return;
            if (ShouldDisplay(FilterChannel.Debug, msg))
            {
                LogAndSave("[DEBUG] " + msg);
            }
        }

        /// <inheritdoc />
        public override void Error(string msg)
        {
            base.Error(msg);
            if (ErrorEnabled)
                QueueMessage("[ERROR] " + msg);
        }

        /// <inheritdoc />
        public override void Info(string msg)
        {
            base.Info(msg);
            if (InfoEnabled)
                QueueMessage("[INFO] " + msg);
        }

        /// <inheritdoc />
        public override void Warn(string msg)
        {
            base.Warn(msg);
            if (WarnEnabled)
                QueueMessage("[WARN] " + msg);
        }
    }
}
