///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Client.Stream;

namespace Client.Inventory
{
    public interface IItemFamilyProperty
    {
        void Serial(BitStreamFile f);
    }

    public class Cosmetic : IItemFamilyProperty
    {
        private uint VPValue;
        private uint Gender;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out VPValue);
            f.Serial(out Gender);
        }
    }

    public class Armor : IItemFamilyProperty
    {
        private uint ArmorType;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out ArmorType);
        }
    }

    public class MeleeWeapon : IItemFamilyProperty
    {
        private uint WeaponType;
        private uint Skill;
        private uint DamageType;
        private uint MeleeRange;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out WeaponType);
            f.Serial(out Skill);
            f.Serial(out DamageType);
            f.Serial(out MeleeRange);
        }
    }

    public class RangeWeapon : IItemFamilyProperty
    {
        private uint WeaponType;
        private uint RangeWeaponType;
        private uint Skill;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out WeaponType);
            f.Serial(out Skill);
            f.Serial(out RangeWeaponType);
        }
    }

    public class Ammo : IItemFamilyProperty
    {
        private uint Skill;
        private uint DamageType;
        private uint Magazine;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out Skill);
            f.Serial(out DamageType);
            f.Serial(out Magazine);
        }
    }

    // Info on a itemPart which can be build by this MP
    public class MpItemPart : IItemFamilyProperty
    {
        const int NumRMStatType = 34;

        // The origin filter. It is actually a ITEM_ORIGIN::EItemOrigin
        private byte OriginFilter;

        // The differents stats for this itemPart
        private byte[] Stats = new byte[NumRMStatType];

        internal MpItemPart()
        {
            OriginFilter = 0;
        }

        public void Serial(BitStreamFile f)
        {
            f.Serial(out OriginFilter);

            for (uint i = 0; i < NumRMStatType; i++)
            {
                f.Serial(out Stats[i]);
            }
        }
    }

    public class Mp : IItemFamilyProperty
    {
        public uint Ecosystem;
        public uint MpCategory;
        public uint HarvestSkill;
        // The MP Family
        public uint Family;
        // If the MP is used as a special Craft Component requirement
        public bool UsedAsCraftRequirement;
        // The Mp color
        public byte MpColor;
        // The mean Stat Energy of this MP
        public ushort StatEnergy;
        // The ItemParts this MP can craft
        public ulong ItemPartBF;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out Ecosystem);
            f.Serial(out MpCategory);
            f.Serial(out HarvestSkill);
            f.Serial(out Family);

            #region ulong workaround

            f.SerialBuffer(out var bytes, 8); // Workaround - Serialize Slots used

            ItemPartBF = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                ItemPartBF |= (ulong)bytes[i] << (i * 8);
            }

            #endregion

            f.Serial(out UsedAsCraftRequirement);
            f.Serial(out MpColor);
            f.Serial(out StatEnergy);
        }
    }

    public class Shield : IItemFamilyProperty
    {
        private uint ShieldType;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out ShieldType);
        }
    }

    public class Tool : IItemFamilyProperty
    {
        private uint Skill; // Used by HARVEST, TAMING, TRAINING tools
        private uint CraftingToolType; // Used by CRAFTING tool
        private uint CommandRange; // Used by TAMING tool
        private uint MaxDonkey; // Used by TAMING tool

        public void Serial(BitStreamFile f)
        {
            f.Serial(out Skill);
            f.Serial(out CraftingToolType);
            f.Serial(out CommandRange);
            f.Serial(out MaxDonkey);
        }
    }

    public class GuildOption : IItemFamilyProperty
    {
        private uint MoneyCost;
        private uint XPCost;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out MoneyCost);
            f.Serial(out XPCost);
        }
    }

    public class Pet : IItemFamilyProperty
    {
        private uint Slot;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out Slot);
        }
    }

    public class Teleport : IItemFamilyProperty
    {
        private uint Type;

        public void Serial(BitStreamFile f)
        {
            f.Serial(out Type);
        }
    }

    public class Scroll : IItemFamilyProperty
    {
        private string Texture = "";
        private string LuaCommand = "";
        private string WebCommand = "";
        private string Label = "";

        public void Serial(BitStreamFile f)
        {
            f.Serial(out Texture);
            f.Serial(out LuaCommand);
            f.Serial(out WebCommand);
            f.Serial(out Label);
        }
    }

    public class Consumable : IItemFamilyProperty
    {
        private ushort OverdoseTimer;
        private ushort ConsumptionTime;

        internal Consumable()
        {
            OverdoseTimer = 0;
            ConsumptionTime = 0;
        }

        private List<string> Properties = new List<string>();

        public void Serial(BitStreamFile f)
        {
            f.Serial(out OverdoseTimer);
            f.Serial(out ConsumptionTime);
            f.SerialCont(out Properties);
        }
    }
}