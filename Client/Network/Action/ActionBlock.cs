///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Client.Stream;

namespace Client.Network.Action
{
    /// <summary>
    /// a block of actions for sending and receiving
    /// </summary>
    internal class ActionBlock
    {
        public List<ActionBase> Actions = new List<ActionBase>();
        public uint Cycle;
        public int FirstPacket;
        public bool Success;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActionBlock()
        {
            Cycle = 0;
            FirstPacket = 0;
            Success = true;
        }

        /// <summary>
        /// serialisation method to the stream for the whole block
        /// </summary>
        public void Serial(BitMemoryStream msg)
        {
            if (!msg.IsReading() && Cycle == 0)
                RyzomClient.GetInstance().GetLogger()?.Warn($"Packing action block ({Actions.Count} actions) with unset date");

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
                    ActionBase action;

                    try
                    {
                        action = ActionFactory.Unpack(msg);
                    }
                    catch (Exception e)
                    {
                        RyzomClient.GetInstance().GetLogger()?.Warn($"ActionBase block upacking failed: {e.Message}");
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
                    var msgPosBefore = msg.GetPosInBit();
                    ActionFactory.Pack(Actions[i], msg);
                    var msgPosAfter = msg.GetPosInBit();

                    var actionSize = ActionFactory.Size(Actions[i]);

                    if (actionSize < msgPosAfter - msgPosBefore)
                        RyzomClient.GetInstance().GetLogger()?.Warn(
                            $"ActionBase {Actions[i].Code} declares a lower size ({actionSize} bits) from what it actually serialises ({msgPosAfter - msgPosBefore} bits)");
                    
                    //sprintf(cat, " %d(%d bits)", Actions[i]->Code, Actions[i]->size());
                    //strcat(buff, cat);
                }
            }

            //nlinfo("Block: %s", buff);
        }
    }
}