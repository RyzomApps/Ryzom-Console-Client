///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RCC.WinAPI
{
    /// <summary>
    ///     Perform clean up before quitting application
    /// </summary>
    /// <remarks>
    ///     Only ctrl+c/ctrl+break will be captured when running on mono
    /// </remarks>
    public static class ExitCleanUp
    {
        /// <summary>
        ///     Store codes to run before quitting
        /// </summary>
        private static readonly List<Action> Actions = new List<Action>();

        private static readonly ConsoleCtrlHandler Handler;

        static ExitCleanUp()
        {
            try
            {
                // Capture all close event
                Handler += CleanUp;
                // Use delegate directly cause program to crash
                SetConsoleCtrlHandler(Handler, true);
            }
            catch (DllNotFoundException)
            {
                // Probably on mono, fallback to ctrl+c only
                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) { RunCleanUp(); };
            }
        }

        /// <summary>
        ///     Add a new action to be performed before application exit
        /// </summary>
        /// <param name="cleanUpCode">Action to run</param>
        public static void Add(Action cleanUpCode)
        {
            Actions.Add(cleanUpCode);
        }

        /// <summary>
        ///     Run all actions
        /// </summary>
        /// <remarks>
        ///     For .Net native
        /// </remarks>
        private static void RunCleanUp()
        {
            foreach (Action action in Actions)
            {
                action();
            }
        }

        /// <summary>
        ///     Run all actions
        /// </summary>
        /// <param name="sig"></param>
        /// <returns></returns>
        /// <remarks>
        ///     For win32 API
        /// </remarks>
        private static bool CleanUp(CtrlType sig)
        {
            foreach (Action action in Actions)
            {
                action();
            }

            return false;
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler handler, bool add);

        private delegate bool ConsoleCtrlHandler(CtrlType sig);

        enum CtrlType
        {
            CtrlCEvent = 0,
            CtrlBreakEvent = 1,
            CtrlCloseEvent = 2,
            CtrlLogoffEvent = 5,
            CtrlShutdownEvent = 6
        }
    }
}