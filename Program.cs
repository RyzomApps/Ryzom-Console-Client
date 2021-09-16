using RCC.WinAPI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using RCC.Config;
using RCC.Helper;

namespace RCC
{
    /// <summary>
    /// Ryzom Console Client by bierdosenhalter and Contributors (c) 2021.
    /// Allows to connect to the Ryzom server, send and receive text, automated scripts.
    /// </summary>
    internal class Program
    {
        private static RyzomClient _client;

        public static string[] Startupargs;

        public static string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        public static readonly string BuildInfo;

        // TODO: add parameters: 
        // login
        // password
        // shard_id
        // slot

        /// <summary>
        /// The main entry point of Ryzom Console Client
        /// </summary>
        private static void Main(string[] args)
        {
            ConsoleIO.WriteLineFormatted($"§bConsole Client for Ryzom §av{Version}§b - By bierdosenhalter & Contributors");

            // Debug input ?
            if (args.Length == 1 && args[0] == "--keyboard-debug")
            {
                ConsoleIO.WriteLine("Keyboard debug mode: Press any key to display info");
                ConsoleIO.DebugReadInput();
            }

            // Setup ConsoleIO
            ConsoleIO.LogPrefix = "§8[RCC] ";
            if (args.Length >= 1 && args[^1] == "BasicIO" || args.Length >= 1 && args[^1] == "BasicIO-NoColor")
            {
                if (args.Length >= 1 && args[^1] == "BasicIO-NoColor")
                {
                    ConsoleIO.BasicIO_NoColor = true;
                }
                ConsoleIO.BasicIO = true;
                args = args.Where(o => !ReferenceEquals(o, args[^1])).ToArray();
            }

            // Take advantage of Windows 10 / Mac / Linux UTF-8 console
            if (IsUsingMono || WindowsVersion.WinMajorVersion >= 10)
            {
                Console.OutputEncoding = Console.InputEncoding = Encoding.UTF8;
            }

            // Process ini configuration file
            if (args.Length >= 1 && File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() == ".cfg")
            {
                ClientCfg.LoadFile(args[0]);

                // remove ini configuration file from arguments array
                var argsTmp = args.ToList();
                argsTmp.RemoveAt(0);
                args = argsTmp.ToArray();
            }
            else if (File.Exists("client.cfg"))
            {
                ClientCfg.LoadFile("client.cfg");
            }
            else ClientCfg.WriteDefaultSettings("client.cfg");

            // Asking the user to type in missing data such as Username and Password
            if (ClientCfg.Username == "")
            {
                ConsoleIO.WriteLineFormatted("§dPlease enter your username:");
                ClientCfg.Username = Console.ReadLine();
            }
            if (ClientCfg.Password == "")
            {
                RequestPassword();
            }

            // Setup exit cleaning code
            ExitCleanUp.Add(delegate
            {
                // Do NOT use Program.Exit() as creating new Thread cause program to freeze
                if (_client == null) return;

                _client.Disconnect(); ConsoleIO.Reset();
                //if (offlinePrompt != null) { offlinePrompt.Abort(); offlinePrompt = null; ConsoleIO.Reset(); }
                //if (ClientCfg.playerHeadAsIcon) { ConsoleIcon.revertToRCCIcon(); }
            });

            Startupargs = args;
            InitializeClient();
        }

        /// <summary>
        /// Reduest user to submit password.
        /// </summary>
        private static void RequestPassword()
        {
            ConsoleIO.WriteLineFormatted($"§dPlease type the password for {ClientCfg.Username}:");
            ClientCfg.Password = ConsoleIO.BasicIO ? Console.ReadLine() : ConsoleIO.ReadPassword();
            if (ClientCfg.Password == "") { ClientCfg.Password = "-"; }

            if (ConsoleIO.BasicIO) return;

            //Hide password length
            Console.CursorTop--; Console.Write(@"********");
            for (var i = 9; i < Console.BufferWidth; i++) { Console.Write(' '); }
            Console.WriteLine();
        }

        /// <summary>
        /// Start a new Client
        /// </summary>
        private static void InitializeClient()
        {
            _client = new RyzomClient();
        }


        /// <summary>
        /// Detect if the user is running Ryzom Console Client through Mono
        /// </summary>
        public static bool IsUsingMono => Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// Enumerate types in namespace through reflection
        /// </summary>
        /// <param name="nameSpace">Namespace to process</param>
        /// <param name="assembly">Assembly to use. Default is Assembly.GetExecutingAssembly()</param>
        /// <returns></returns>
        public static Type[] GetTypesInNamespace(string nameSpace, Assembly assembly = null)
        {
            if (assembly == null) { assembly = Assembly.GetExecutingAssembly(); }
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        /// <summary>
        /// Static initialization of build information, read from assembly information
        /// </summary>
        static Program()
        {
            if (typeof(Program)
                .Assembly
                .GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false)
                .FirstOrDefault() is AssemblyConfigurationAttribute attribute)
                BuildInfo = attribute.Configuration;
        }

        /// <summary>
        /// Disconnect the current client from the server and exit the app
        /// </summary>
        public static void Exit(int exitcode = 0)
        {
            new Thread(new ThreadStart(delegate
            {
                if (_client != null) { _client.Disconnect(); ConsoleIO.Reset(); }
                //if (offlinePrompt != null) { offlinePrompt.Abort(); offlinePrompt = null; ConsoleIO.Reset(); }
                //if (Settings.playerHeadAsIcon) { ConsoleIcon.revertToRCCIcon(); }
                Environment.Exit(exitcode);
            })).Start();
        }
    }
}
