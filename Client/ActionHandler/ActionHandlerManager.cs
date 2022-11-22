///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;

namespace Client.ActionHandler
{
    /// <summary>
    /// interface for action handlers factory
    /// no release in this factory : a handler must be destroyed by the control that created it
    /// </summary>
    public class ActionHandlerManager
    {
        private static ActionHandlerManager _globalInstance;

        /// map of action handler factories
        public SortedDictionary<string, IActionHandler> FactoryMap = new SortedDictionary<string, IActionHandler>();
        public SortedDictionary<IActionHandler, string> NameMap = new SortedDictionary<IActionHandler, string>();
        public string EmptyName = "";

        public static ActionHandlerManager GetInstance()
        {
            return _globalInstance ??= new ActionHandlerManager();
        }

        public void GetActionHandlers(List<string> handlers)
        {
            handlers.Clear();

            using var itr = FactoryMap.GetEnumerator();

            while (itr.MoveNext())
            {
                handlers.Add(itr.Current.Key);
            }
        }

        /// <summary>
        /// return pointer to action handler or null if it doesn't exist
        /// </summary>
        public IActionHandler GetActionHandler(in string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (FactoryMap.ContainsKey(name))
                return FactoryMap[name];

            RyzomClient.GetInstance().GetLogger().Warn("Couldn't find action handler " + name);
            return null;

        }

        /// <summary>
        /// Return the name of the action handler given its pointer
        /// </summary>
        public string GetActionHandlerName(IActionHandler pAh)
        {
            return NameMap.ContainsKey(pAh) ? NameMap[pAh] : EmptyName;
        }

        /// <summary>
        /// return the Action Handler 'name'. if name is of form 'ah:params', then params are filled (NB: else not changed)
        /// </summary>
        public IActionHandler GetAh(in string name, ref string @params)
        {
            // Special AH form?
            var i = name.IndexOf(':');

            if (i == -1)
                // standalone form
                return GetActionHandler(name);

            var ahName = name.Substring(0, i);
            @params = name[(i + 1)..];
            return GetActionHandler(ahName);
        }

        /// <summary>
        /// Ah name must all be lower case
        /// </summary>
        public static void REGISTER_ACTION_HANDLER(IActionHandler handler, string name)
        {
            Debug.Assert(name != null);

            foreach (var c in name)
            {
                Debug.Assert(char.IsLower(c) || !char.IsLetter(c));
            }

            var pAhfm = GetInstance();
            pAhfm.FactoryMap.Add(name, handler);
            pAhfm.NameMap.Add(handler, name);
        }

        // TODO: Load all action handlers
        ///// <summary>
        ///// Load commands from the 'Commands' namespace
        ///// </summary>
        //public void LoadCommands()
        //{
        //    if (_commandsLoaded) return;
        //
        //    var cmdsClasses = Program.GetTypesInNamespace("Client.Commands");
        //
        //    foreach (var type in cmdsClasses)
        //    {
        //        if (!type.IsSubclassOf(typeof(CommandBase))) continue;
        //
        //        try
        //        {
        //            var cmd = (CommandBase)Activator.CreateInstance(type);
        //
        //            if (cmd != null)
        //            {
        //                _cmds[cmd.CmdName.ToLower()] = cmd;
        //                _cmdNames.Add(cmd.CmdName.ToLower());
        //                foreach (var alias in cmd.GetCmdAliases())
        //                    _cmds[alias.ToLower()] = cmd;
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Warn(e.Message);
        //        }
        //    }
        //
        //    _commandsLoaded = true;
        //}
    }
}