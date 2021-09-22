///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Minecraft Console Client'
// https://github.com/ORelio/Minecraft-Console-Client
// which is released under CDDL-1.0 License
// http://opensource.org/licenses/CDDL-1.0
// Copyright 2021 ORelio and Contributers
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using RCC.Commands.Internal;

namespace RCC.Automata.Internal
{
    /// <summary>
    /// CommandBase class with constructor for creating command for automatons.
    /// </summary>
    public class AutomatonCommand : CommandBase
    {
        public AutomatonBase.CommandRunner Runner;

        public override string CmdName { get; }
        public override string CmdUsage { get; }
        public override string CmdDesc { get; }

        public override string Run(RyzomClient handler, string command, Dictionary<string, object> localVars)
        {
            return Runner(command, GetArgs(command));
        }

        /// <summary>
        /// automatonCommand Constructor
        /// </summary>
        /// <param name="cmdName">Name of the command</param>
        /// <param name="cmdDesc">Description of the command. Support tranlation.</param>
        /// <param name="cmdUsage">Usage of the command</param>
        /// <param name="callback">Method for handling the command</param>
        public AutomatonCommand(string cmdName, string cmdDesc, string cmdUsage, AutomatonBase.CommandRunner callback)
        {
            CmdName = cmdName;
            CmdDesc = cmdDesc;
            CmdUsage = cmdUsage;
            Runner = callback;
        }
    }
}