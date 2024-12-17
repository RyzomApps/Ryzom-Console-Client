namespace Client.Logger
{
    internal class DebugLogger : LoggerBase
    {
        public override void Debug(string msg)
        {
            if (!DebugEnabled) return;

            Log("[DEBUG] " + msg);
        }

        public override void Info(string msg)
        {
            if (!InfoEnabled)
                return;

            Log("[INFO] " + msg);
        }

        public override void Warn(string msg)
        {
            if (!WarnEnabled)
                return;

            Log("[WARN] " + msg);
        }

        public override void Error(string msg)
        {
            if (!ErrorEnabled)
                return;

            Log("[ERROR] " + msg);
        }

        public override void Chat(string msg)
        {
            if (!ChatEnabled)
                return;

            Log("[CHAT] " + msg);
        }

        /// <summary>
        /// Log a formatted object. ToString is used.
        /// </summary>
        protected override void Log(object msg)
        {
            System.Diagnostics.Debug.Print(msg.ToString());
        }

        /// <summary>
        /// Log a formatted message
        /// </summary>
        protected override void Log(string msg)
        {
            System.Diagnostics.Debug.Print(msg);
        }

        /// <summary>
        /// Log a formatted message, with format arguments
        /// </summary>
        protected override void Log(string msg, params object[] args)
        {
            System.Diagnostics.Debug.Print(string.Format(msg, args));
        }
    }
}
