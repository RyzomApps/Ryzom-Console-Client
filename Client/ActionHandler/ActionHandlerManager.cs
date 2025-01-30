///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Client.ActionHandler
{
    /// <summary>
    /// Interface for action handlers factory
    /// no release in this factory : a handler must be destroyed by the control that created it
    /// </summary>
    public class ActionHandlerManager
    {
        private readonly RyzomClient _client;

        // map of action handler factories
        private readonly SortedDictionary<string, ActionHandlerBase> _factoryMap = new();
        //private readonly SortedDictionary<ActionHandlerBase, string> _nameMap = new();
        public const string EmptyName = "";
        private bool _handlersLoaded;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionHandlerManager(RyzomClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Ah name must all be lower case
        /// </summary>
        public void RegisterActionHandler(ActionHandlerBase handler, string name)
        {
            Debug.Assert(name != null);

            foreach (var c in name)
            {
                Debug.Assert(char.IsLower(c) || !char.IsLetter(c));
            }

            _factoryMap.Add(name, handler);
            //_nameMap.Add(handler, name);
        }

        /// <summary>
        /// Load action handlers from the 'ActionHandler' namespace
        /// </summary>
        public void LoadActionHandlers()
        {
            if (_handlersLoaded) return;

            var cmdsClasses = Program.GetTypesInNamespace("Client.Commands");

            foreach (var type in cmdsClasses)
            {
                if (!type.IsSubclassOf(typeof(ActionHandlerBase))) continue;

                try
                {
                    var cmd = (ActionHandlerBase)Activator.CreateInstance(type);

                    if (cmd != null)
                    {
                        RegisterActionHandler(cmd, cmd.Name);
                    }
                }
                catch (Exception e)
                {
                    _client.Log.Warn(e.Message);
                }
            }

            _handlersLoaded = true;
        }

        /// <summary>
        /// Executes an action handler based on the provided command line input.
        /// </summary>
        public void RunActionHandler(string cmdLine, object caller, string userParams)
        {
            // Exit if no command line is provided
            if (string.IsNullOrWhiteSpace(cmdLine))
                return;

            // Split the command line into action name and parameters
            var index = cmdLine.IndexOf(':');
            var ahName = index != -1 ? cmdLine[..index] : cmdLine;
            var ahParams = index != -1 ? cmdLine[(index + 1)..] : string.Empty;

            // Replace parameters with user-defined parameters if provided
            if (!string.IsNullOrEmpty(userParams))
                ahParams = userParams;

            // Attempt to retrieve and execute the action handler
            if (!_factoryMap.TryGetValue(ahName, out var actionHandler))
            {
                _client.GetLogger().Warn($"Action handler '{ahName}' not found.");
                return;
            }

            actionHandler.Execute(caller, ahParams);

            //// Attempt to execute the quick help action handler
            //const string submitQuickHelp = "submit_quick_help";
            //if (_factoryMap.TryGetValue(submitQuickHelp, out var quickHelpHandler))
            //{
            //    var eventString = $"{ahName}:{ahParams}";
            //    quickHelpHandler.Execute(null, eventString);
            //}
            //else
            //{
            //    _client.GetLogger().Warn($"Action handler '{submitQuickHelp}' not found.");
            //}
        }
    }
}