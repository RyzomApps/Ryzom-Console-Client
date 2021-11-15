///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace API.Entity
{
    public class EntityBase
    {
        #region Static Methods

        /// <summary>
        /// Removes the specification from a name. The specification is surrounded by '$', and tells the title of the entity (guard, matis merchant, etc ..)
        /// </summary>
        public static string RemoveTitleFromName(string name)
        {
            var p1 = name.IndexOf('$');

            if (p1 == -1)
            {
                return name;
            }

            var p2 = name.IndexOf('$', p1 + 1);

            if (p2 != -1)
            {
                return name.Substring(0, p1) + name[(p2 + 1)..];
            }

            return name.Substring(0, p1);
        }

        /// <summary>
        /// Removes the shard from a name (if player from the same shard). The shard is surrounded by (), and tells the incoming shard of the entity (aniro, leanon etc...)
        /// </summary>
        public static string RemoveShardFromName(string name)
        {
            // The string must contains a '(' and a ')'
            var p0 = name.IndexOf('(');
            var p1 = name.IndexOf(')');

            if (p0 == -1 || p1 == -1 || p1 <= p0)
                return name;

            // Remove all shard names (hack)
            return name.Substring(0, p0) + name[(p1 + 1)..];
        }

        /// <summary>
        /// Removes title and shard from a name. See <see cref="RemoveTitleFromName"/> and <see cref="RemoveShardFromName"/>.
        /// </summary>
        public static string RemoveTitleAndShardFromName(string name)
        {
            return RemoveTitleFromName(RemoveShardFromName(name));
        }
        #endregion
    }
}
