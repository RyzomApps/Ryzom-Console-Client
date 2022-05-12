///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Network;

namespace Client.Client
{
    /// <summary>
    /// Holds information about the character such as name, sheetid and title.
    /// </summary>
    public class CharacterSummary
    {
        private byte _characterSlot;
        private bool _hasEditSession;
        private bool _inNewbieland;

        private bool _inRingSession;

        /// <summary>
        /// Localisation
        /// </summary>
        private uint _location;

        private int _title;

        /// <summary>
        /// visual property for appearance A
        /// </summary>
        private long _visualPropA;

        /// <summary>
        /// visual property for appearance B
        /// </summary>
        private long _visualPropB;

        /// <summary>
        /// visual property for appearance C
        /// </summary>
        private long _visualPropC;

        public uint Mainland;

        public string Name;

        public int People;

        public int SheetId;

        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterSummary()
        {
            Mainland = 101; // TODO: Get this from the server
            Name = string.Empty;
            _location = 0;
            _visualPropA = 0;
            _visualPropB = 0;
            _visualPropC = 0;
            People = (int)PeopleType.Unknown; // 142;
            _title = 238;
            _characterSlot = 255;
            _inRingSession = false;
            _hasEditSession = false;
            _inNewbieland = false;
        }

        /// <summary>
        /// serialisation coming from a stream (net message)
        /// </summary>
        public void Serial(BitMemoryStream f)
        {
            f.SerialVersion(0);
            f.Serial(ref Mainland);
            f.Serial(ref Name);
            f.Serial(ref People);
            f.Serial(ref _location);
            f.Serial(ref _visualPropA);
            f.Serial(ref _visualPropB);
            f.Serial(ref _visualPropC);
            f.Serial(ref SheetId);
            f.Serial(ref _title);
            f.Serial(ref _characterSlot);
            f.Serial(ref _inRingSession);
            f.Serial(ref _hasEditSession);
            f.Serial(ref _inNewbieland);
        }
    }
}