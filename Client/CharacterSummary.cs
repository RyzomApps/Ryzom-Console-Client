// This code is a modified version of a file from the 'Ryzom - MMORPG Framework'
// <http://dev.ryzom.com/projects/ryzom/>,
// which is released under GNU Affero General Public License.
// <http://www.gnu.org/licenses/>
// Original Copyright 2010 by Winch Gate Property Limited

using RCC.Network;

namespace RCC.Client
{
    public class CharacterSummary
    {
        byte _characterSlot;
        bool _hasEditSession;
        bool _inNewbieland;

        bool _inRingSession;

        /// Localisation
        uint _location;

        /// mainland
        public uint Mainland;

        /// name
        public string Name;

        public int People;

        public int SheetId;

        int _title;

        /// visual property for appearance
        long _visualPropA;

        long _visualPropB;
        long _visualPropC;

        public CharacterSummary()
        {
            Mainland = 0;
            Name = string.Empty;
            _location = 0;
            _visualPropA = 0;
            _visualPropB = 0;
            _visualPropC = 0;
            People = (int) Client.People.Unknown; // 142;
            _title = 238;
            _characterSlot = 255;
            _inRingSession = false;
            _hasEditSession = false;
            _inNewbieland = false;
        }

        /// serialisation coming from a stream (net message)
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