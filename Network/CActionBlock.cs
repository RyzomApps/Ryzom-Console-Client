using System.Collections.Generic;
using RCC.Helper;
using RCC.NetworkAction;

namespace RCC.Network
{
    internal class CActionBlock
    {
        public int Cycle;
        public int FirstPacket;
        public List<CAction> Actions = new List<CAction>();
        bool Success;

        /// Constructor
        public CActionBlock()
        {
            Cycle = 0;
            FirstPacket = 0;
            Success = true;
        }

        /// <summary>
        /// serialisation method
        /// </summary>
        public void serial(CBitMemStream msg)
        {
            if (!msg.isReading() && Cycle == 0)
                ConsoleIO.WriteLineFormatted("§ePacking action block (" + Actions.Count + " actions) with unset date");

            msg.serial(ref Cycle);

            int i;

            byte num = (byte)Actions.Count;
            msg.serial(ref num);

            //static char	buff[1024], cat[128];

            if (msg.isReading())
            {
                //sprintf(buff, "Unpack[%d]:", Cycle);
                for (i = 0; i < num; ++i)
                {
                    CAction action = CActionFactory.unpack(msg, false);
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
                    int msgPosBefore = msg.getPosInBit();
                    CActionFactory.pack(Actions[i], msg);
                    int msgPosAfter = msg.getPosInBit();

                    int actionSize = CActionFactory.size(Actions[i]);

                    if (actionSize < msgPosAfter - msgPosBefore)
                        ConsoleIO.WriteLineFormatted("§eAction " + Actions[i].Code + " declares a lower size (" + actionSize + " bits) from what it actually serialises (" + (msgPosAfter - msgPosBefore) + " bits)");
                    //sprintf(cat, " %d(%d bits)", Actions[i]->Code, Actions[i]->size());
                    //strcat(buff, cat);
                }
            }
            //nlinfo("Block: %s", buff);
        }

        static uint getHeaderSizeInBits() { return (sizeof(int) + sizeof(byte)) * 8; }
    }
}