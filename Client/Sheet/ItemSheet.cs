///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using API.Sheet;
using Client.Inventory;
using Client.Stream;
using System.Collections.Generic;
using API.Inventory;

namespace Client.Sheet
{
    public class ItemSheet : Sheet, IItemSheet
    {
        /// <summary>shape file name</summary>
        public string Shape;
        /// <summary>Female shape file name</summary>
        public string ShapeFemale;
        /// <summary>shape file name for fyros</summary>
        public string ShapeFyros;
        /// <summary>Female shape file name for fyros</summary>
        public string ShapeFyrosFemale;
        /// <summary>shape file name for matis</summary>
        public string ShapeMatis;
        /// <summary>Female shape file name for matis</summary>
        public string ShapeMatisFemale;
        /// <summary>shape file name for tryker</summary>
        public string ShapeTryker;
        /// <summary>Female shape file name for tryker</summary>
        public string ShapeTrykerFemale;
        /// <summary>shape file name for zorai</summary>
        public string ShapeZorai;
        /// <summary>Female shape file name for zorai</summary>
        public string ShapeZoraiFemale;

        /// <summary>Equipment slot. This is a bitField matching each bit to SLOTTYPE::TSlotType</summary>
        public ulong SlotBf;
        /// <summary>texture variant.</summary>
        public uint MapVariant;
        /// <summary>Item Family</summary>
        public ItemFamily Family { get; set; }

        /// <summary>Item Type</summary>
        public ItemType ItemType { get; set; }

        /// <summary>icon file name for race type</summary>
        public string IconBack;
        /// <summary>icon file name for main icon type</summary>
        public string IconMain;
        /// <summary>icon file name for overlay</summary>
        public string IconOver;
        /// <summary>icon file name for overlay2</summary>
        public string IconOver2;

        // Special Color to modulate with
        public uint IconColor;
        public uint IconBackColor;
        public uint IconOverColor;
        public uint IconOver2Color;

        /// <summary>icon Special Text (raw materials)</summary>
        public string IconText;
        /// <summary>Part of the animation set ot use with this item.</summary>
        public string AnimSet;

        /// <summary>Item Color. Special Enum for armours</summary>
        public byte Color;
        /// <summary>has fx</summary>
        public bool HasFx;
        /// <summary>Does the player can sell the item?</summary>
        public bool DropOrSell;
        /// <summary>Item is not persistent to a disconnection?</summary>
        public bool IsItemNoRent;
        /// <summary>item max stack size</summary>
        public uint Stackable;
        /// <summary>is item consumable</summary>
        public bool IsConsumable;
        /// <summary>Bulk.</summary>
        public float Bulk;
        /// <summary>Equip Time</summary>
        public uint EquipTime;
        /// <summary>true if this item can be hidden when equipped</summary>
        public bool NeverHideWhenEquipped;

        // FX
        //public CItemFXSheet FX = new CItemFXSheet();

        // item special effects
        public string Effect1;
        public string Effect2;
        public string Effect3;
        public string Effect4;

        // Only used for Mp
        public readonly List<MpItemPart> MpItemParts = new List<MpItemPart>();

        // item requirements
        //public CHARACTERISTICS.TCharacteristics RequiredCharac = new CHARACTERISTICS.TCharacteristics();
        public ushort RequiredCharacLevel;
        //public SKILLS.ESkills RequiredSkill = new SKILLS.ESkills();
        public ushort RequiredSkillLevel;

        /// <summary>if craftable, the craft plan</summary>
        public readonly SheetId CraftPlan;

        //public ITEM_ORIGIN.EItemOrigin ItemOrigin = new ITEM_ORIGIN.EItemOrigin();

        public Scroll Scroll;
        public Consumable Consumable;

        private IItemFamilyProperty _itemFamilyProperty;

        public ItemSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {
            CraftPlan = new SheetId(sheetIdFactory);

            Shape = "";
            ShapeFemale = "";
            ShapeFyros = "";
            ShapeFyrosFemale = "";
            ShapeMatis = "";
            ShapeMatisFemale = "";
            ShapeTryker = "";
            ShapeTrykerFemale = "";
            ShapeZorai = "";
            ShapeZoraiFemale = "";

            MapVariant = 0;
            ItemType = ItemType.UNDEFINED;
            Family = 0;
            SlotBf = 0;

            IconBack = "";
            IconMain = "";
            IconOver = "";
            IconOver2 = "";
            IconText = "";
            AnimSet = "";

            Color = 0;
            HasFx = false;
            DropOrSell = false;
            IsItemNoRent = false;
            Stackable = 0;
            IsConsumable = false;

            Effect1 = "";
            Effect2 = "";
            Effect3 = "";
            Effect4 = "";

            Type = SheetType.ITEM;
            Bulk = 0.0f;
            EquipTime = 0;
            NeverHideWhenEquipped = false;
            RequiredCharacLevel = 0;
            RequiredSkillLevel = 0;
        }

        public override void Serial(BitMemoryStream f)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        public override void Serial(BitStreamFile f)
        {
            f.Serial(out Shape);
            f.Serial(out ShapeFemale);
            f.Serial(out ShapeFyros);
            f.Serial(out ShapeFyrosFemale);
            f.Serial(out ShapeMatis);
            f.Serial(out ShapeMatisFemale);
            f.Serial(out ShapeTryker);
            f.Serial(out ShapeTrykerFemale);
            f.Serial(out ShapeZorai);
            f.Serial(out ShapeZoraiFemale);

            #region Serialize Slots used - ulong workaround

            f.SerialBuffer(out var bytes, 8);

            SlotBf = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                SlotBf |= (ulong)bytes[i] << (i * 8);
            }

            #endregion

            f.Serial(out MapVariant); // Serialize Map Variant

            f.Serial(out uint family); // Serialize Family
            Family = (ItemFamily)family;

            f.Serial(out uint itemType); // Serialize ItemType
            ItemType = (ItemType)itemType;

            f.Serial(out IconMain);
            f.Serial(out IconBack);
            f.Serial(out IconOver);
            f.Serial(out IconOver2);
            f.Serial(out IconColor);
            f.Serial(out IconBackColor);
            f.Serial(out IconOverColor);
            f.Serial(out IconOver2Color);
            f.Serial(out IconText);
            f.Serial(out AnimSet);
            f.Serial(out Color); // Serialize the item color
            f.Serial(out HasFx); // Serialize the has fx
            f.Serial(out DropOrSell);
            f.Serial(out IsItemNoRent);
            f.Serial(out NeverHideWhenEquipped);
            f.Serial(out Stackable);
            f.Serial(out IsConsumable);
            f.Serial(out Bulk);
            f.Serial(out EquipTime);

            #region FX workaround - not used since rcc has no graphical output

            f.Serial(out float _); // TrailMinSliceTime (float)
            f.Serial(out float _); // TrailMaxSliceTime (float)
            f.Serial(out float _); f.Serial(out float _); f.Serial(out float _); // Vector AttackFXOffset (3 x float)
            f.Serial(out string _); // _Trail (string)
            f.Serial(out string _); // _AdvantageFX (string)
            f.Serial(out string _); // _AttackFX (string)
            f.Serial(out float _); f.Serial(out float _); f.Serial(out float _); // Vector AttackFXRot (3 x float)
            f.Serial(out float _); // ImpactFXDelay (float)

            // Workaround for SerialCont
            f.Serial(out uint len);

            for (var i = 0; i < len; i++)
            {
                f.Serial(out string _); // Name (string)
                f.Serial(out string _); // Bone (string)
                f.Serial(out float _); f.Serial(out float _); f.Serial(out float _); // Vector Offset (3 x float)
            }

            #endregion 

            f.Serial(out Effect1);
            f.Serial(out Effect2);
            f.Serial(out Effect3);
            f.Serial(out Effect4);

            #region MpItemParts workaround for SerialCont

            f.Serial(out uint len2);

            for (var i = 0; i < len2; i++)
            {
                var part = new MpItemPart();
                part.Serial(f);
                MpItemParts.Add(part);
            }

            #endregion

            CraftPlan.Serial(f);

            f.Serial(out uint _); // int
            f.Serial(out RequiredCharacLevel);
            f.Serial(out uint _); // int
            f.Serial(out RequiredSkillLevel);

            // Serial Help Infos
            f.Serial(out uint _); // int

            // item commands
            Scroll = new Scroll();
            Scroll.Serial(f);

            // Different Serial according to family
            switch (Family)
            {
                case ItemFamily.COSMETIC:
                    _itemFamilyProperty = new Cosmetic();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.ARMOR:
                    _itemFamilyProperty = new Armor();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.MELEE_WEAPON:
                    _itemFamilyProperty = new MeleeWeapon();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.RANGE_WEAPON:
                    _itemFamilyProperty = new RangeWeapon();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.AMMO:
                    _itemFamilyProperty = new Ammo();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.RAW_MATERIAL:
                    _itemFamilyProperty = new Mp();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.SHIELD:
                    _itemFamilyProperty = new Shield();
                    _itemFamilyProperty.Serial(f);
                    break;

                // Same for any tool
                case ItemFamily.CRAFTING_TOOL:
                case ItemFamily.HARVEST_TOOL:
                case ItemFamily.TAMING_TOOL:
                    _itemFamilyProperty = new Tool();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.GUILD_OPTION:
                    _itemFamilyProperty = new GuildOption();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.PET_ANIMAL_TICKET:
                    _itemFamilyProperty = new Pet();
                    _itemFamilyProperty.Serial(f);
                    break;

                case ItemFamily.TELEPORT:
                    _itemFamilyProperty = new Teleport();
                    _itemFamilyProperty.Serial(f);
                    break;

                // keep for readability
                case ItemFamily.SCROLL:
                    break;

                case ItemFamily.CONSUMABLE:
                    _itemFamilyProperty = new Consumable();
                    _itemFamilyProperty.Serial(f);
                    break;
            }
        }
    }
}
