///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Chat;
using Client.Database;
using System.Diagnostics;

namespace Client.Inventory
{
    /// <summary>
	/// Image of an item in the database
	/// Unavailable fields are Set to NULL
	/// </summary>
	/// <author>Nicolas Vizerie</author>
	/// <author>Nevrax France</author>
	/// <date>September 2003</date>
    public class ItemImage
    {
        private DatabaseNodeLeaf _sheet;
        private DatabaseNodeLeaf _quality;
        private DatabaseNodeLeaf _quantity;
        private DatabaseNodeLeaf _userColor;
        private DatabaseNodeLeaf _characBuffs;
        private DatabaseNodeLeaf _price;
        private DatabaseNodeLeaf _weight;
        private DatabaseNodeLeaf _nameId;
        private DatabaseNodeLeaf _infoVersion;
        private DatabaseNodeLeaf _resaleFlag;

        public uint GetSheetId() { return (uint)(_sheet?.GetValue32() ?? 0); }
        public ushort GetQuality() { return (ushort)(_quality?.GetValue16() ?? 0); }
        public ushort GetQuantity() { return (ushort)(_quantity?.GetValue16() ?? 0); }
        public byte GetUserColor() { return (byte)(_userColor?.GetValue8() ?? 0); }
        public byte GetCharacBuffs() { return (byte)(_characBuffs?.GetValue8() ?? 0); }
        public uint GetPrice() { return (uint)(_price?.GetValue32() ?? 0); }
        public uint GetWeight() { return (uint)(_weight?.GetValue32() ?? 0); }
        public uint GetNameId() { return (uint)(_nameId?.GetValue32() ?? 0); }
        public byte GetInfoVersion() { return (byte)(_infoVersion != null ? (byte)_infoVersion.GetValue8() : 0); }
        public byte GetResaleFlag() { return (byte)(_resaleFlag != null ? (byte)_resaleFlag.GetValue8() : 0); }
        public bool GetLockedByOwner() { return (bool)(_resaleFlag != null && (_resaleFlag.GetValue8() == (byte)BotChatResaleFlag.ResaleKOLockedByOwner)); }

        public void SetSheetId(uint si) { _sheet?.SetValue32((int)si); }
        public void SetQuality(ushort quality) { _quality?.SetValue16((short)quality); }
        public void SetQuantity(ushort quantity) { _quantity?.SetValue16((short)quantity); }
        public void SetUserColor(byte uc) { _userColor?.SetValue8((byte)uc); }
        public void SetCharacBuffs(byte uc) { _characBuffs?.SetValue8((byte)uc); }
        public void SetPrice(uint price) { _price?.SetValue32((int)price); }
        public void SetWeight(uint wgt) { _weight?.SetValue32((int)wgt); }
        public void SetNameId(uint nid) { _nameId?.SetValue32((int)nid); }
        public void SetInfoVersion(byte iv) { _infoVersion?.SetValue8((byte)iv); }
        public void SetResaleFlag(byte resale) { _resaleFlag?.SetValue8(resale); }

        public void Build(DatabaseNodeBranch branch)
        {
            if (branch == null)
                return;

            _sheet = branch.GetNode(new TextId("SHEET"), false) as DatabaseNodeLeaf;
            _quality = branch.GetNode(new TextId("QUALITY"), false) as DatabaseNodeLeaf;
            _quantity = branch.GetNode(new TextId("QUANTITY"), false) as DatabaseNodeLeaf;
            _userColor = branch.GetNode(new TextId("USER_COLOR"), false) as DatabaseNodeLeaf;
            _characBuffs = branch.GetNode(new TextId("CHARAC_BUFFS"), false) as DatabaseNodeLeaf;
            _price = branch.GetNode(new TextId("PRICE"), false) as DatabaseNodeLeaf;
            _weight = branch.GetNode(new TextId("WEIGHT"), false) as DatabaseNodeLeaf;
            _nameId = branch.GetNode(new TextId("NAMEID"), false) as DatabaseNodeLeaf;
            _infoVersion = branch.GetNode(new TextId("INFO_VERSION"), false) as DatabaseNodeLeaf;
            _resaleFlag = branch.GetNode(new TextId("RESALE_FLAG"), false) as DatabaseNodeLeaf;

            // Should always have at least those one: (ie all but Price)
            Debug.Assert(_sheet != null || _quality != null || _quantity != null || _userColor != null || _weight != null || _nameId != null || _infoVersion != null);
        }
    }
}
