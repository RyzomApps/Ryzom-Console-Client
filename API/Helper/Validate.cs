///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Bukkit'
// https://github.com/Bukkit/Bukkit
// which is released under GNU General Public License v3.0.
// https://www.gnu.org/licenses/gpl-3.0.en.html
// Copyright 2021 Bukkit Team
///////////////////////////////////////////////////////////////////

namespace API.Helper
{
    public class Validate
    {
        public static void IsTrue(bool test, string text)
        {
            if(!test)
                throw new System.Exception(text);
        }

        public static void NotNull(object test, string text)
        {
            if(test == null)
                throw new System.Exception(text);
        }
    }
}