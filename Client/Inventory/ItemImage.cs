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
        private DatabaseNodeLeaf Sheet;
        private DatabaseNodeLeaf Quality;
        private DatabaseNodeLeaf Quantity;
        private DatabaseNodeLeaf UserColor;
        private DatabaseNodeLeaf CharacBuffs;
        private DatabaseNodeLeaf Price;
        private DatabaseNodeLeaf Weight;
        private DatabaseNodeLeaf NameId;
        private DatabaseNodeLeaf InfoVersion;
        private DatabaseNodeLeaf ResaleFlag;

        public uint GetSheetID() { return (uint)(Sheet != null ? Sheet.GetValue32() : 0); }
        public ushort GetQuality() { return (ushort)(Quality != null ? Quality.GetValue16() : 0); }
        public ushort GetQuantity() { return (ushort)(Quantity != null ? Quantity.GetValue16() : 0); }
        public byte GetUserColor() { return (byte)(UserColor != null ? UserColor.GetValue8() : 0); }
        public byte GetCharacBuffs() { return (byte)(CharacBuffs != null ? CharacBuffs.GetValue8() : 0); }
        public uint GetPrice() { return (uint)(Price != null ? Price.GetValue32() : 0); }
        public uint GetWeight() { return (uint)(Weight != null ? Weight.GetValue32() : 0); }
        public uint GetNameId() { return (uint)(NameId != null ? NameId.GetValue32() : 0); }
        public byte GetInfoVersion() { return (byte)(InfoVersion != null ? (byte)InfoVersion.GetValue8() : 0); }
        public byte GetResaleFlag() { return (byte)(ResaleFlag != null ? (byte)ResaleFlag.GetValue8() : 0); }
        public bool GetLockedByOwner() { return (bool)(ResaleFlag != null ? (ResaleFlag.GetValue8() == (byte)BotChatResaleFlag.ResaleKOLockedByOwner) : false); }

        public void SetSheetID(uint si) { if (Sheet != null) { Sheet.SetValue32((int)si); } }
        public void SetQuality(ushort quality) { if (Quality != null) { Quality.SetValue16((short)quality); } }
        public void SetQuantity(ushort quantity) { if (Quantity != null) { Quantity.SetValue16((short)quantity); } }
        public void SetUserColor(byte uc) { if (UserColor != null) { UserColor.SetValue8((byte)uc); } }
        public void SetCharacBuffs(byte uc) { if (CharacBuffs != null) { CharacBuffs.SetValue8((byte)uc); } }
        public void SetPrice(uint price) { if (Price != null) { Price.SetValue32((int)price); } }
        public void SetWeight(uint wgt) { if (Weight != null) { Weight.SetValue32((int)wgt); } }
        public void SetNameId(uint nid) { if (NameId != null) { NameId.SetValue32((int)nid); } }
        public void SetInfoVersion(byte iv) { if (InfoVersion != null) { InfoVersion.SetValue8((byte)iv); } }
        public void SetResaleFlag(byte resale) { if (ResaleFlag != null) { ResaleFlag.SetValue8(resale); } }

        public ItemImage()
        {
            Sheet = null;
            Quality = null;
            Quantity = null;
            UserColor = null;
            CharacBuffs = null;
            Price = null;
            Weight = null;
            NameId = null;
            InfoVersion = null;
        }

        public void Build(DatabaseNodeBranch branch)
        {
            if (branch == null)
                return;
            
            Sheet = branch.GetNode(new TextId("SHEET"), false) as DatabaseNodeLeaf;
            Quality = branch.GetNode(new TextId("QUALITY"), false) as DatabaseNodeLeaf;
            Quantity = branch.GetNode(new TextId("QUANTITY"), false) as DatabaseNodeLeaf;
            UserColor = branch.GetNode(new TextId("USER_COLOR"), false) as DatabaseNodeLeaf;
            CharacBuffs = branch.GetNode(new TextId("CHARAC_BUFFS"), false) as DatabaseNodeLeaf;
            Price = branch.GetNode(new TextId("PRICE"), false) as DatabaseNodeLeaf;
            Weight = branch.GetNode(new TextId("WEIGHT"), false) as DatabaseNodeLeaf;
            NameId = branch.GetNode(new TextId("NAMEID"), false) as DatabaseNodeLeaf;
            InfoVersion = branch.GetNode(new TextId("INFO_VERSION"), false) as DatabaseNodeLeaf;
            ResaleFlag = branch.GetNode(new TextId("RESALE_FLAG"), false) as DatabaseNodeLeaf;

            // Should always have at least those one: (ie all but Price)
            Debug.Assert(Sheet != null || Quality != null || Quantity != null || UserColor != null || Weight != null || NameId != null || InfoVersion != null);
        }
    }
}
