using RCC.WinAPI;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using RCC.Config;
using RCC.Helper;

namespace RCC
{
    /// <summary>
    /// Ryzom Console Client by bierdosenhalter and Contributors (c) 2021.
    /// Allows to connect to the Ryzom server, send and receive text, automated scripts.
    /// </summary>
    class Program
    {
        private static readonly RyzomClient Client = new RyzomClient();

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
            Console.WriteLine("Console Client for Ryzom v{0} - By bierdosenhalter & Contributors", Version);

            // Debug input ?
            if (args.Length == 1 && args[0] == "--keyboard-debug")
            {
                ConsoleIO.WriteLine("Keyboard debug mode: Press any key to display info");
                ConsoleIO.DebugReadInput();
            }

            // Setup ConsoleIO
            ConsoleIO.LogPrefix = "§8[MCC] ";
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
            if (args.Length >= 1 && System.IO.File.Exists(args[0]) && System.IO.Path.GetExtension(args[0]).ToLower() == ".ini")
            {
                ClientCfg.LoadFile(args[0]);

                // remove ini configuration file from arguments array
                var argsTmp = args.ToList();
                argsTmp.RemoveAt(0);
                args = argsTmp.ToArray();
            }
            else if (System.IO.File.Exists("RyzomClient.ini"))
            {
                ClientCfg.LoadFile("RyzomClient.ini");
            }
            else ClientCfg.WriteDefaultSettings("RyzomClient.ini");

            // TODO: session caching

            // Setup exit cleaning code
            ExitCleanUp.Add(delegate
            {
                // Do NOT use Program.Exit() as creating new Thread cause program to freeze
                if (Client == null) return;

                Client.Disconnect(); ConsoleIO.Reset();
                //if (offlinePrompt != null) { offlinePrompt.Abort(); offlinePrompt = null; ConsoleIO.Reset(); }
                //if (ClientCfg.playerHeadAsIcon) { ConsoleIcon.revertToMCCIcon(); }
            });

            Startupargs = args;

            //try
            //{
                Client.Connect();
            //}
            //catch (Exception e)
            //{
            //    ConsoleIO.WriteLine(e.Message);
            //    Console.Read();
            //    return;
            //}

            Console.Read();
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
    }
}
