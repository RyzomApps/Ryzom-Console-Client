﻿///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using API.Logger;
using API.Plugins.Interfaces;

namespace API
{
    /// <summary>
    /// Represents a client implementation.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Returns the primary logger associated with this client instance.
        /// </summary>
        /// <returns>Logger associated with this client</returns>
        ILogger GetLogger();

        /// <summary>
        /// Gets the plugin manager for interfacing with plugins.
        /// </summary>
        /// <returns>a plugin manager for this client instance</returns>
        IPluginManager GetPluginManager();

        /// <summary>
        /// Online State
        /// </summary>
        /// <returns>true when the client has entered a game and is online</returns>
        bool IsInGame();

        /// <summary>
        /// Perform an internal RCC command (not a server command, use SendText() instead for that!)
        /// </summary>
        /// <param name="command">The command</param>
        /// <param name="responseMsg">May contain a confirmation or error message after processing the command, or "" otherwise.</param>
        /// <param name="localVars">Local variables passed along with the command</param>
        /// <returns>true if the command was indeed an internal RCC command</returns>
        bool PerformInternalCommand(string command, ref string responseMsg, Dictionary<string, object> localVars = null);
    }
}