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
        public readonly string Name = ActionHandlerManager.EmptyName;

        protected readonly IClient _client;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Ryzom Client</param>
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

        public virtual List<Tuple<string, string>> GetAllParams(string parameters) { return []; }

        protected virtual string GetParam(string @params, string paramName)
        {
            var allparam = @params;

            SkipBlankAtStart(ref allparam);

            var param = paramName.ToLower();

            while (!string.IsNullOrEmpty(allparam))
            {
                var e = allparam.IndexOf('=');

                if (e is -1 or 0)
                    break;

                var p = allparam.IndexOf('|');
                var tmp = allparam[..e].ToLower();

                SkipBlankAtEnd(ref tmp);

                if (tmp == param)
                {
                    var tmp2 = p == -1 ? allparam[(e + 1)..] : allparam.Substring(e + 1, p - e - 1);
                    SkipBlankAtStart(ref tmp2);
                    SkipBlankAtEnd(ref tmp2);
                    return tmp2;
                }

                if (p is -1 or 0)
                    break;

                allparam = allparam[(p + 1)..];
                SkipBlankAtStart(ref allparam);
            }

            return "";
        }

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
                    end = end[..^1];
                }
                else
                {
                    break;
                }
            }
        }
    }
}