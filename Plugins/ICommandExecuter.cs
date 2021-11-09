///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using RCC.Commands.Internal;

namespace RCC.Plugins
{
    public interface ICommandExecuter
    {
        /// <summary>
        /// Executes the given command, returning its success
        /// </summary>
        /// <param name="sender">Source of the command</param>
        /// <param name="command">Command which was executed</param>
        /// <param name="label">Alias of the command which was used</param>
        /// <param name="args">Passed command arguments</param>
        /// <returns>true if a valid command, otherwise false</returns>
        public bool OnCommand(object sender, CommandBase command, string label, string[] args);
    }
}