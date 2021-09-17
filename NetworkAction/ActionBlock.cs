// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System;
using System.Collections.Generic;
using RCC.Helper;
using RCC.Network;

namespace RCC.NetworkAction
{
    internal class ActionBlock
    {
        public List<Action> Actions = new List<Action>();
        public uint Cycle;
        public int FirstPacket;
        bool Success;

        /// Constructor
        public ActionBlock()
        {
            Cycle = 0;
            FirstPacket = 0;
            Success = true;
        }

        /// <summary>
        ///     serialisation method
        /// </summary>
        public void serial(BitMemoryStream msg)
        {
            if (!msg.IsReading() && Cycle == 0)
                ConsoleIO.WriteLineFormatted("§ePacking action block (" + Actions.Count + " actions) with unset date");

            msg.Serial(ref Cycle);

            int i;

            byte num = (byte) Actions.Count;
            msg.Serial(ref num);

            //static char	buff[1024], cat[128];

            if (msg.IsReading())
            {
                //sprintf(buff, "Unpack[%d]:", Cycle);
                for (i = 0; i < num; ++i)
                {
                    Action action;

                    try
                    {
                        action = ActionFactory.unpack(msg, false);
                    }
                    catch (Exception e)
                    {
                        ConsoleIO.WriteLineFormatted("§9" + e.Message);
                        action = null;
                    }

                    if (action == null)
                    {
                        Success = false; // reject an incorrect block
                    }
                    else
                    {
                        //sprintf(cat, " %d(%d bits)", action->Code, action->size());
                        //strcat(buff, cat);
                        Actions.Add(action);
                    }
                }
            }
            else
            {
                //sprintf(buff, "Pack[%d]:", Cycle);
                for (i = 0; i < num; ++i)
                {
                    int msgPosBefore = msg.GetPosInBit();
                    ActionFactory.pack(Actions[i], msg);
                    int msgPosAfter = msg.GetPosInBit();

                    int actionSize = ActionFactory.size(Actions[i]);

                    if (actionSize < msgPosAfter - msgPosBefore)
                        ConsoleIO.WriteLineFormatted("§eAction " + Actions[i].Code + " declares a lower size (" +
                                                     actionSize + " bits) from what it actually serialises (" +
                                                     (msgPosAfter - msgPosBefore) + " bits)");
                    //sprintf(cat, " %d(%d bits)", Actions[i]->Code, Actions[i]->size());
                    //strcat(buff, cat);
                }
            }

            //nlinfo("Block: %s", buff);
        }

        static uint getHeaderSizeInBits()
        {
            return (sizeof(int) + sizeof(byte)) * 8;
        }
    }
}