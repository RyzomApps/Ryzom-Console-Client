// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using System.Collections.Generic;
using System.Diagnostics;
using RCC.Helper;
using RCC.Network;

namespace RCC.NetworkAction
{
    internal class GenericMultiPartTemp
    {
        readonly List<bool> _blockReceived = new List<bool>();
        int _nbBlock;
        int _nbCurrentBlock;
        readonly List<List<byte>> _temp = new List<List<byte>>();
        int _tempSize;

        public GenericMultiPartTemp()
        {
            _nbBlock = int.MaxValue;
        }

        public void Set(ActionGenericMultiPart agmp)
        {
            if (_nbBlock == int.MaxValue)
            {
                // new GenericMultiPart
                _nbBlock = (int) agmp.NbBlock;
                _nbCurrentBlock = 0;
                _tempSize = 0;
                _temp.Clear();
                _blockReceived.Clear();

                //Temp.resize(NbBlock);
                //BlockReceived.resize(NbBlock);

                for (int i = 0; i < _nbBlock; i++)
                {
                    _blockReceived.Add(false);
                    _temp.Add(new List<byte>());
                }
            }

            Debug.Assert(_nbBlock == agmp.NbBlock);
            Debug.Assert(_nbBlock > agmp.Part);

            // check if the block was already received
            if (_blockReceived[agmp.Part])
            {
                ConsoleIO.WriteLine("CLMPNET: This part is already received, discard it");
                return;
            }

            _temp[agmp.Part] = new List<byte>(agmp.PartCont);
            _blockReceived[agmp.Part] = true;

            _nbCurrentBlock++;
            _tempSize += (int) agmp.PartCont.Length;

            if (_nbCurrentBlock != _nbBlock) return;

            // reform the total action

            //nldebug("CLMPNET[%p]: Received a TOTAL generic action MP size: number %d nbblock %d", this,  agmp.Number, NbBlock);

            // TODO: real size instead of 255
            var bms = new BitMemoryStream(false);
            //bms.resetBufPos();

            // TODO: build that stream from the parts
            //byte ptr = bms.bufferToFill(TempSize);
            //
            foreach (var t in _temp)
            {
                //    memcpy(ptr, &(Temp[i][0]), Temp[i].size());
                //    ptr += Temp[i].size();
                byte[] arr = t.ToArray(); // bdh: LOL

                //bms.resetBufPos();
                bms.Serial(ref arr);
            }

            bms.Invert();

            _nbBlock = int.MaxValue; //0xFFFFFFFF;

            //nldebug("CLMPNET[%p]: Received a generic action size %d", this, bms.length());
            // todo interface api, call a user callback

            //Debug.Print(bms.ToString());

            NetworkConnection._ImpulseCallback?.Invoke(bms);
        }
    }
}