///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Stream;

namespace Client.Sheet
{
    public class ItemSheet : EntitySheet
    {
        public ItemSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {
            //IdShape = 0;
            //IdShapeFemale = 0;
            //IdShapeFyros = 0;
            //IdShapeFyrosFemale = 0;
            //IdShapeMatis = 0;
            //IdShapeMatisFemale = 0;
            //IdShapeTryker = 0;
            //IdShapeTrykerFemale = 0;
            //IdShapeZorai = 0;
            //IdShapeZoraiFemale = 0;
            //
            //MapVariant = 0;
            //ItemType = ITEM_TYPE.UNDEFINED;
            //Family = ITEMFAMILY.UNDEFINED;
            //SlotBF = 0;
            //IdIconBack = 0;
            //IdIconMain = 0;
            //IdIconOver = 0;
            //IdIconOver2 = 0;
            //IdIconText = 0;
            //IdAnimSet = 0;
            //Color = 0;
            //HasFx = false;
            //DropOrSell = false;
            //IsItemNoRent = false;
            //Stackable = 0;
            //IsConsumable = false;
            //IdEffect1 = 0;
            //IdEffect2 = 0;
            //IdEffect3 = 0;
            //IdEffect4 = 0;
            //
            //Type = CEntitySheet.ITEM;
            //Bulk = 0.0f;
            //EquipTime = 0;
            //NeverHideWhenEquipped = false;
            //RequiredCharac = CHARACTERISTICS.Unknown;
            //RequiredCharacLevel = 0;
            //RequiredSkill = SKILLS.unknown;
            //RequiredSkillLevel = 0;
            //IconColor = NLMISC.CRGBA.White;
            //IconBackColor = NLMISC.CRGBA.White;
            //IconOverColor = NLMISC.CRGBA.White;
            //IconOver2Color = NLMISC.CRGBA.White;
            //
            //ItemOrigin = ITEM_ORIGIN.UNKNOWN;
            //
            //Cosmetic.VPValue = 0;
            //Cosmetic.Gender = GSGENDER.unknown;
            //
            //Armor.ArmorType = ARMORTYPE.UNKNOWN;
            //
            //MeleeWeapon.WeaponType = WEAPONTYPE.UNKNOWN;
            //MeleeWeapon.Skill = SKILLS.unknown;
            //MeleeWeapon.DamageType = DMGTYPE.UNDEFINED;
            //MeleeWeapon.MeleeRange = 0;
            //
            //RangeWeapon.WeaponType = WEAPONTYPE.UNKNOWN;
            //RangeWeapon.RangeWeaponType = RANGE_WEAPON_TYPE.Unknown;
            //RangeWeapon.Skill = SKILLS.unknown;
            //
            //Ammo.Skill = SKILLS.unknown;
            //Ammo.DamageType = DMGTYPE.UNDEFINED;
            //Ammo.Magazine = 0;
            //
            //Mp.Ecosystem = ECOSYSTEM.unknown;
            //Mp.MpCategory = MP_CATEGORY.Undefined;
            //Mp.HarvestSkill = SKILLS.unknown;
            //Mp.Family = RM_FAMILY.Unknown;
            //Mp.UsedAsCraftRequirement = false;
            //Mp.MpColor = 0;
            //Mp.StatEnergy = 0;
            //Mp.ItemPartBF = 0;
            //
            //Shield.ShieldType = SHIELDTYPE.NONE;
            //
            //Tool.Skill = SKILLS.unknown;
            //Tool.CraftingToolType = TOOL_TYPE.Unknown;
            //Tool.CommandRange = 0;
            //Tool.MaxDonkey = 0;
            //
            //GuildOption.MoneyCost = 0;
            //GuildOption.XPCost = 0;
            //
            //Pet.Slot = 0;
            //
            //Teleport.Type = TELEPORT_TYPES.NONE;
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
            throw new System.NotImplementedException();

            //ClientSheetsStrings.serial(f, IdShape);
            //ClientSheetsStrings.serial(f, IdShapeFemale);
            //ClientSheetsStrings.serial(f, IdShapeFyros);
            //ClientSheetsStrings.serial(f, IdShapeFyrosFemale);
            //ClientSheetsStrings.serial(f, IdShapeMatis);
            //ClientSheetsStrings.serial(f, IdShapeMatisFemale);
            //ClientSheetsStrings.serial(f, IdShapeTryker);
            //ClientSheetsStrings.serial(f, IdShapeTrykerFemale);
            //ClientSheetsStrings.serial(f, IdShapeZorai);
            //ClientSheetsStrings.serial(f, IdShapeZoraiFemale);
            //f.serial(SlotBF); // Serialize Slots used.
            //f.serial(MapVariant); // Serialize Map Variant.
            //f.serialEnum(Family); // Serialize Family.
            //f.serialEnum(ItemType); // Serialize ItemType.
            //ClientSheetsStrings.serial(f, IdIconMain);
            //ClientSheetsStrings.serial(f, IdIconBack);
            //ClientSheetsStrings.serial(f, IdIconOver);
            //ClientSheetsStrings.serial(f, IdIconOver2);
            //f.serial(IconColor);
            //f.serial(IconBackColor);
            //f.serial(IconOverColor);
            //f.serial(IconOver2Color);
            //ClientSheetsStrings.serial(f, IdIconText);
            //ClientSheetsStrings.serial(f, IdAnimSet);
            //f.serial(Color); // Serialize the item color.
            //f.serial(HasFx); // Serialize the has fx.
            //f.serial(DropOrSell);
            //f.serial(IsItemNoRent);
            //f.serial(NeverHideWhenEquipped);
            //f.serial(Stackable);
            //f.serial(IsConsumable);
            //f.serial(Bulk);
            //f.serial(EquipTime);
            //
            //f.serial(FX);
            //
            //ClientSheetsStrings.serial(f, IdEffect1);
            //ClientSheetsStrings.serial(f, IdEffect2);
            //ClientSheetsStrings.serial(f, IdEffect3);
            //ClientSheetsStrings.serial(f, IdEffect4);
            //
            //f.serialCont(MpItemParts);
            //
            //f.serial(CraftPlan);
            //
            //f.serialEnum(RequiredCharac);
            //f.serial(RequiredCharacLevel);
            //f.serialEnum(RequiredSkill);
            //f.serial(RequiredSkillLevel);
            //
            //// **** Serial Help Infos
            //f.serialEnum(ItemOrigin);
            //
            //// item commands
            //f.serial(Scroll);
            //
            //// Different Serial according to family
            //switch (Family)
            //{
            //    case ITEMFAMILY.COSMETIC:
            //        f.serial(Cosmetic);
            //        break;
            //    case ITEMFAMILY.ARMOR:
            //        f.serial(Armor);
            //        break;
            //    case ITEMFAMILY.MELEE_WEAPON:
            //        f.serial(MeleeWeapon);
            //        break;
            //    case ITEMFAMILY.RANGE_WEAPON:
            //        f.serial(RangeWeapon);
            //        break;
            //    case ITEMFAMILY.AMMO:
            //        f.serial(Ammo);
            //        break;
            //    case ITEMFAMILY.RAW_MATERIAL:
            //        f.serial(Mp);
            //        break;
            //    case ITEMFAMILY.SHIELD:
            //        f.serial(Shield);
            //        break;
            //    // Same for any tool
            //    case ITEMFAMILY.CRAFTING_TOOL:
            //    case ITEMFAMILY.HARVEST_TOOL:
            //    case ITEMFAMILY.TAMING_TOOL:
            //        f.serial(Tool);
            //        break;
            //    case ITEMFAMILY.GUILD_OPTION:
            //        f.serial(GuildOption);
            //        break;
            //    case ITEMFAMILY.PET_ANIMAL_TICKET:
            //        f.serial(Pet);
            //        break;
            //    case ITEMFAMILY.TELEPORT:
            //        f.serial(Teleport);
            //        break;
            //    // keep for readability
            //    case ITEMFAMILY.SCROLL:
            //        //f.serial(Scroll);
            //        break;
            //    case ITEMFAMILY.CONSUMABLE:
            //        f.serial(Consumable);
            //        break;
            //    default:
            //        break;
            //};
        }
    }

}
