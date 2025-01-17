///////////////////////////////////////////////////////////////////
// Ryzom Console Client
// https://github.com/RyzomApps/RCC
// Copyright 2021 bierdosenhalter and Contributers
///////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Client.Config;
using Client.Helper;
using Client.WinAPI;

namespace Client
{
    /// <summary>
    /// Ryzom Console Client by bierdosenhalter and Contributors (c) 2021.
    /// Allows to connect to the Ryzom server, send and receive text, automated scripts.
    /// </summary>
    internal class Program
    {
        private static RyzomClient _client;

        public static string[] Startupargs;

        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        public static readonly string BuildInfo;

        /// <summary>
        /// ISO-8859-1: Windows-1252 or CP-1252 (code page 1252) single-byte character encoding (commonly mislabeled as "ANSI")
        /// </summary>
        public static readonly Encoding Enc1252;

        /// <summary>
        /// Detect if the user is running Ryzom Console Client through Mono
        /// </summary>
        public static bool IsUsingMono => Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// Static initialization of build information, read from assembly information
        /// </summary>
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Enc1252 = Encoding.GetEncoding(1252);

            if (typeof(Program)
                .Assembly
                .GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false)
                .FirstOrDefault() is AssemblyConfigurationAttribute attribute)
                BuildInfo = attribute.Configuration;
        }

        private static void Banner()
        {
            ConsoleIO.WriteLineFormatted("                                          ");
            ConsoleIO.WriteLineFormatted(" §f    _/_/_/  §e     _/_/_/§c     _/_/_/ ");
            ConsoleIO.WriteLineFormatted(" §f   _/    _/ §e  _/       §c  _/        ");
            ConsoleIO.WriteLineFormatted(" §f  _/_/_/    §e _/        §c _/         ");
            ConsoleIO.WriteLineFormatted(" §f _/    _/   §e_/         §c_/          ");
            ConsoleIO.WriteLineFormatted(" §f_/    _/    §e _/_/_/    §c _/_/_/     ");
            ConsoleIO.WriteLineFormatted("                                          ");
        }

        /// <summary>
        /// The main entry point of Ryzom Console Client
        /// </summary>
        private static void Main(string[] args)
        {
            //Console.OutputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Enc1252;

            Banner();

            Console.Title = $@"[RCC] {Version}";
            ConsoleIO.WriteLineFormatted($"§bRyzom Console Client §av{Version}§r");

            // Debug input ?
            if (args.Length == 1 && args[0] == "--keyboard-debug")
            {
                ConsoleIO.WriteLine("Keyboard debug mode: Press any key to display info");
                ConsoleIO.DebugReadInput();
            }

            // Setup ConsoleIO
            ConsoleIO.EnableTimestamps = true;
            ConsoleIO.LogPrefix = "§7[§fR§eC§cC§7] §r";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || args.Length >= 1 && args[^1] == "BasicIO" || args.Length >= 1 && args[^1] == "BasicIO-NoColor")
            {
                if (args.Length >= 1 && args[^1] == "BasicIO-NoColor")
                {
                    ConsoleIO.BasicIoNoColor = true;
                }

                ConsoleIO.BasicIo = true;
                args = args.Where(o => !ReferenceEquals(o, args[^1])).ToArray();
            }

            // Take advantage of Windows 10 / Mac / Linux UTF-8 console
            if (IsUsingMono || WindowsVersion.WinMajorVersion >= 10)
            {
                Console.OutputEncoding = Console.InputEncoding = Encoding.Unicode;
            }

            // Process INI configuration file
            if (args.Length >= 1 && File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() == ".cfg")
            {
                ClientConfig.LoadFile(args[0]);

                // remove INI configuration file from arguments array
                var argsTmp = args.ToList();
                argsTmp.RemoveAt(0);
                args = argsTmp.ToArray();
            }
            else if (File.Exists("client.cfg"))
            {
                ClientConfig.LoadFile("client.cfg");
            }
            else ClientConfig.WriteDefaultSettings("client.cfg");

            // Asking the user to type in missing data such as user name and password
            if (ClientConfig.Username == "")
            {
                ConsoleIO.WriteLineFormatted("§dPlease enter your username:");
                ClientConfig.Username = Console.ReadLine();
            }

            if (ClientConfig.Password == "")
            {
                RequestPassword();
            }

            // Setup exit cleanup code for the console
            ExitCleanUp.Add(ConsoleIO.Reset);

            Startupargs = args;
            InitializeClient();
        }

        /// <summary>
        /// Request user to submit password.
        /// </summary>
        private static void RequestPassword()
        {
            ConsoleIO.WriteLineFormatted($"§dPlease type the password for {ClientConfig.Username}:");
            ClientConfig.Password = ConsoleIO.BasicIo ? Console.ReadLine() : ConsoleIO.ReadPassword();
            if (ClientConfig.Password == "")
            {
                ClientConfig.Password = "-";
            }

            if (ConsoleIO.BasicIo) return;

            // Hide password length
            Console.CursorTop--;
            Console.Write(@"********");
            for (var i = 9; i < Console.BufferWidth; i++)
            {
                Console.Write(' ');
            }

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
        /// Enumerate types in namespace through reflection
        /// </summary>
        /// <param name="nameSpace">Namespace to process</param>
        /// <param name="assembly">Assembly to use. Default is Assembly.GetExecutingAssembly()</param>
        /// <returns></returns>
        public static Type[] GetTypesInNamespace(string nameSpace, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            return assembly.GetTypes().Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                .ToArray();
        }

        /// <summary>
        /// Disconnect the current client from the server and exit the app
        /// </summary>
        public static void Exit(int exitcode = 0)
        {
            // Exit the application
            Environment.Exit(exitcode);
        }
    }
}