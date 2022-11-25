///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using API;

namespace Client.ActionHandler
{
    /// <summary>
    /// interface for action handlers
    /// </summary>
    /// <author>Nicolas Brigand</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public abstract class ActionHandlerBase : System.IDisposable
    {
        public string Name = ActionHandlerManager.EmptyName;

        private readonly IClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        protected ActionHandlerBase(IClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Execute the answer to the action
        /// </summary>
        /// <param name="caller">Control that calls the action</param>
        /// <param name="parameters">Parameters has the following form : paramName=theParam|paramName2=theParam2|...</param>
        public abstract void Execute(object caller, string parameters);

        public abstract void Dispose();

        public virtual string GetParam(string parameters, string parameterName)
        {
            return "";
        }

        public virtual List<System.Tuple<string, string>> GetAllParams(string parameters) { return new List<Tuple<string, string>>(); }
    }
}