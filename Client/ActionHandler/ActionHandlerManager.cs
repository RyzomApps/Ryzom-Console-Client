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
using Client.Commands;

namespace Client.ActionHandler
{
    /// <summary>
    /// Interface for action handlers factory
    /// no release in this factory : a handler must be destroyed by the control that created it
    /// </summary>
    public class ActionHandlerManager
    {
        private readonly RyzomClient _client;

        /// map of action handler factories
        public SortedDictionary<string, ActionHandlerBase> FactoryMap = new SortedDictionary<string, ActionHandlerBase>();
        public SortedDictionary<ActionHandlerBase, string> NameMap = new SortedDictionary<ActionHandlerBase, string>();
        public static string EmptyName = "";
        private bool _handlersLoaded;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionHandlerManager(RyzomClient client)
        {
            _client = client;
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
        public ActionHandlerBase GetActionHandler(in string name)
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
        public string GetActionHandlerName(ActionHandlerBase pAh)
        {
            return NameMap.ContainsKey(pAh) ? NameMap[pAh] : EmptyName;
        }

        /// <summary>
        /// return the Action Handler 'name'. if name is of form 'ah:params', then params are filled (NB: else not changed)
        /// </summary>
        public ActionHandlerBase GetAh(in string name, ref string @params)
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
        public void RegisterActionHandler(ActionHandlerBase handler, string name)
        {
            Debug.Assert(name != null);

            foreach (var c in name)
            {
                Debug.Assert(char.IsLower(c) || !char.IsLetter(c));
            }

            //var pAhfm = GetInstance();
            /*pAhfm.*/FactoryMap.Add(name, handler);
            /*pAhfm.*/NameMap.Add(handler, name);
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
    }
}