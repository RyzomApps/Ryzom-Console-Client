///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using API.Commands;

namespace Client.Commands.Internal
{
    /// <summary>
    /// Represents an internal RCC command: CommandBase name, source code and usage message
    /// To add a new command, inherit from this class while adding the command class to the folder "Commands".
    /// If inheriting from the 'CommandBase' class and placed in the 'Commands' namespace, the command will be
    /// automatically loaded and available in main chat prompt, scripts, remote control and command help.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// The command name
        /// </summary>
        public abstract string CmdName { get; }

        /// <summary>
        /// CommandBase description with translation support. Please add your message in Translations.cs file and return mapping
        /// key in this property
        /// </summary>
        public abstract string CmdDesc { get; }

        /// <summary>
        /// Usage message, eg: 'name [args]'
        /// </summary>
        public abstract string CmdUsage { get; }

        /// <summary>
        /// Get the translated version of command description.
        /// </summary>
        /// <returns>Translated command description</returns>
        public string GetCmdDescTranslated()
        {
            var s = string.IsNullOrEmpty(CmdUsage) || string.IsNullOrEmpty(CmdDesc)
                ? ""
                : ": "; // If either one is empty, no colon :
            return CmdUsage + s + CmdDesc;
        }

        /// <summary>
        /// Perform the command
        /// </summary>
        /// <param name="handler">Client</param>
        /// <param name="command">The full command, eg: 'mycommand arg1 arg2'</param>
        /// <param name="localVars">Local variables passed along with the command (may be null)</param>
        /// <returns>A confirmation/error message, or "" if no message</returns>
        public abstract string Run(RyzomClient handler, string command, Dictionary<string, object> localVars);

        /// <summary>
        /// Return a list of aliases for this command.
        /// Override this method if you wish to put aliases to the command
        /// </summary>
        public virtual IEnumerable<string> GetCmdAliases()
        {
            return new string[0];
        }

        /// <summary>
        /// Check if at least one argument has been passed to the command
        /// </summary>
        public static bool HasArg(string command)
        {
            var firstSpace = command.IndexOf(' ');
            return firstSpace > 0 && firstSpace < command.Length - 1;
        }

        /// <summary>
        /// Extract the argument string from the command
        /// </summary>
        /// <returns>Argument or "" if no argument</returns>
        public static string GetArg(string command)
        {
            return HasArg(command) ? command[(command.IndexOf(' ') + 1)..] : "";
        }

        /// <summary>
        /// Extract the arguments as a string array from the command
        /// </summary>
        /// <returns>Argument array or empty array if no arguments</returns>
        public static string[] GetArgs(string command)
        {
            var args = GetArg(command).Split(' ');
            return args.Length == 1 && args[0] == "" ? new string[] { } : args;
        }
    }
}