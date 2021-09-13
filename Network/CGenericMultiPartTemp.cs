using System.Collections.Generic;
using RCC.NetworkAction;

namespace RCC.Network
{
    internal class CGenericMultiPartTemp
    {
        int NbBlock;
        int NbCurrentBlock;
        int TempSize;
        List<List<byte>> Temp = new List<List<byte>>();

        List<bool> BlockReceived = new List<bool>();

        public CGenericMultiPartTemp()
        {
            NbBlock = int.MaxValue;
        }

        public void set(CActionGenericMultiPart agmp)
        {
            if (NbBlock == int.MaxValue)
            {
                // new GenericMultiPart
                NbBlock = (int)agmp.NbBlock;
                NbCurrentBlock = 0;
                TempSize = 0;
                Temp.Clear();
                BlockReceived.Clear();

                //Temp.resize(NbBlock);
                //BlockReceived.resize(NbBlock);

                for (int i = 0; i < NbBlock; i++)
                {
                    BlockReceived.Add(false);
                    Temp.Add(new List<byte>());
                }
            }

            //nlassert(NbBlock == agmp.NbBlock);
            //nlassert(NbBlock > agmp.Part);

            // check if the block was already received
            if (BlockReceived[agmp.Part])
            {
                ConsoleIO.WriteLine("CLMPNET: This part is already received, discard it");
                return;
            }

            Temp[agmp.Part] = agmp.PartCont;
            BlockReceived[agmp.Part] = true;

            NbCurrentBlock++;
            TempSize += (int)agmp.PartCont.Count;

            if (NbCurrentBlock == NbBlock)
            {
                // reform the total action

                //nldebug("CLMPNET[%p]: Received a TOTAL generic action MP size: number %d nbblock %d", this,  agmp.Number, NbBlock);

                var bms = new CBitMemStream(true);

                // TODO: build that stream from the parts
                //byte ptr = bms.bufferToFill(TempSize);
                //
                //for (uint i = 0; i < Temp.size(); i++)
                //{
                //    memcpy(ptr, &(Temp[i][0]), Temp[i].size());
                //    ptr += Temp[i].size();
                //}

                NbBlock = int.MaxValue; //0xFFFFFFFF;

                //nldebug("CLMPNET[%p]: Received a generic action size %d", this, bms.length());
                // todo interface api, call a user callback

                if (NetworkConnection._ImpulseCallback != null)
                    NetworkConnection._ImpulseCallback(bms, NetworkConnection._LastReceivedNumber, NetworkConnection._ImpulseArg);

            }
        }
    }
}