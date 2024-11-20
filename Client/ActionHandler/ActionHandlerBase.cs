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
    public abstract class ActionHandlerBase
    {
        public string Name = ActionHandlerManager.EmptyName;

        protected readonly IClient _client;

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

        private static void SkipBlankAtStart(ref string start)
        {
            while (!string.IsNullOrEmpty(start))
            {
                if (start[0] == ' ' || start[0] == '\t' || start[0] == '\r' || start[0] == '\n')
                {
                    start = start.Substring(1, start.Length);
                }
                else
                {
                    break;
                }
            }
        }

        private static void SkipBlankAtEnd(ref string end)
        {
            while (!string.IsNullOrEmpty(end))
            {
                if (end[^1] == ' ' || end[^1] == '\t' || end[^1] == '\r' || end[^1] == '\n')
                {
                    end = end[0..^1];
                }
                else
                {
                    break;
                }
            }
        }

        public virtual string GetParam(string Params, string ParamName)
        {
            string allparam = Params;
            SkipBlankAtStart(ref allparam);
            string param = ParamName.ToLower();

            while (!string.IsNullOrEmpty(allparam))
            {
                int e = allparam.IndexOf('=');
                if (e == -1 || e == 0)
                {
                    break;
                }
                int p = allparam.IndexOf('|');
                string tmp = allparam.Substring(0, e).ToLower();
                SkipBlankAtEnd(ref tmp);
                if (tmp == param)
                {
                    string tmp2 = allparam.Substring(e + 1, p - e - 1);
                    SkipBlankAtStart(ref tmp2);
                    SkipBlankAtEnd(ref tmp2);
                    return tmp2;
                }
                if (p == -1 || p == 0)
                {
                    break;
                }
                allparam = allparam.Substring(p + 1, allparam.Length);
                SkipBlankAtStart(ref allparam);
            }

            return "";
        }

        public virtual List<Tuple<string, string>> GetAllParams(string parameters) { return new List<Tuple<string, string>>(); }
    }
}