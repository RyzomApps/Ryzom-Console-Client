///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;

namespace RCC.Network.Action
{
    /// <summary>
    /// temporary multipart holder until the generic action is complete
    /// </summary>
    internal class GenericMultiPartTemp
    {
        private readonly List<bool> _blockReceived = new List<bool>();
        private readonly List<List<byte>> _temp = new List<List<byte>>();
        private int _nbBlock;
        private int _nbCurrentBlock;

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericMultiPartTemp()
        {
            _nbBlock = int.MaxValue;
        }

        /// <summary>
        /// add a part to the temp action - and call an action if message is complete
        /// </summary>
        public void Set(ActionGenericMultiPart agmp, NetworkConnection networkConnection)
        {
            if (_nbBlock == int.MaxValue)
            {
                // new GenericMultiPart
                _nbBlock = (int) agmp.NbBlock;
                _nbCurrentBlock = 0;
                _temp.Clear();
                _blockReceived.Clear();

                for (var i = 0; i < _nbBlock; i++)
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
                RyzomClient.GetInstance().GetLogger()?.Debug("CLMPNET: This part is already received, discard it");
                return;
            }

            _temp[agmp.Part] = new List<byte>(agmp.PartCont);
            _blockReceived[agmp.Part] = true;

            _nbCurrentBlock++;

            if (_nbCurrentBlock != _nbBlock) return;

            // reform the total action
            var bms = new BitMemoryStream();

            foreach (var t in _temp)
            {
                var arr = t.ToArray();
                bms.Serial(ref arr);
            }

            bms.Invert();

            _nbBlock = int.MaxValue; //0xFFFFFFFF;

            networkConnection.ImpulseCallback?.Invoke(bms);
        }
    }
}