///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

namespace Client.Brick;

public static class BrickFamilyHelper
{
    public static BrickFamily ToSBrickFamily(string str)
    {
        // Implement string to TBrickFamily conversion logic here
        return BrickFamily.Unknown; // default return value
    }

    public static string ToString(BrickFamily family)
    {
        // Implement TBrickFamily to string conversion logic here
        return family.ToString();
    }

    public static bool IsRootFamily(BrickFamily family)
    {
        return (family >= BrickFamily.BeginCombatRoot && family <= BrickFamily.EndCombatRoot) ||
               (family >= BrickFamily.BeginMagicRoot && family <= BrickFamily.EndMagicRoot) ||
               (family >= BrickFamily.BeginFaberRoot && family <= BrickFamily.EndFaberRoot) ||
               (family >= BrickFamily.BeginHarvestRoot && family <= BrickFamily.EndHarvestRoot) ||
               (family >= BrickFamily.BeginForageProspectionRoot && family <= BrickFamily.EndForageProspectionRoot) ||
               (family >= BrickFamily.BeginForageExtractionRoot && family <= BrickFamily.EndForageExtractionRoot) ||
               (family >= BrickFamily.BeginPowerRoot && family <= BrickFamily.EndPowerRoot) ||
               (family >= BrickFamily.BeginProcEnchantement && family <= BrickFamily.EndProcEnchantement);
    }

    public static bool IsMandatoryFamily(BrickFamily family)
    {
        return (family >= BrickFamily.BeginCombatMandatory && family <= BrickFamily.EndCombatMandatory) ||
               (family >= BrickFamily.BeginMagicMandatory && family <= BrickFamily.EndMagicMandatory) ||
               (family >= BrickFamily.BeginFaberMandatory && family <= BrickFamily.EndFaberMandatory) ||
               (family >= BrickFamily.BeginHarvestMandatory && family <= BrickFamily.EndHarvestMandatory) ||
               (family >= BrickFamily.BeginForageProspectionMandatory && family <= BrickFamily.EndForageProspectionMandatory) ||
               (family >= BrickFamily.BeginForageExtractionMandatory && family <= BrickFamily.EndForageExtractionMandatory) ||
               (family >= BrickFamily.BeginPowerMandatory && family <= BrickFamily.EndPowerMandatory);
    }

    public static bool IsOptionFamily(BrickFamily family)
    {
        return (family >= BrickFamily.BeginCombatOption && family <= BrickFamily.EndCombatOption) ||
               (family >= BrickFamily.BeginMagicOption && family <= BrickFamily.EndMagicOption) ||
               (family >= BrickFamily.BeginFaberOption && family <= BrickFamily.EndFaberOption) ||
               (family >= BrickFamily.BeginHarvestOption && family <= BrickFamily.EndHarvestOption) ||
               (family >= BrickFamily.BeginForageProspectionOption && family <= BrickFamily.EndForageProspectionOption) ||
               (family >= BrickFamily.BeginForageExtractionOption && family <= BrickFamily.EndForageExtractionOption);
    }

    public static bool IsCreditFamily(BrickFamily family)
    {
        return (family >= BrickFamily.BeginCombatCredit && family <= BrickFamily.EndCombatCredit) ||
               (family >= BrickFamily.BeginMagicCredit && family <= BrickFamily.EndMagicCredit) ||
               (family >= BrickFamily.BeginFaberCredit && family <= BrickFamily.EndFaberCredit) ||
               (family >= BrickFamily.BeginHarvestCredit && family <= BrickFamily.EndHarvestCredit) ||
               (family >= BrickFamily.BeginForageProspectionCredit && family <= BrickFamily.EndForageProspectionCredit) ||
               (family >= BrickFamily.BeginForageExtractionCredit && family <= BrickFamily.EndForageExtractionCredit) ||
               (family >= BrickFamily.BeginMagicPowerCredit && family <= BrickFamily.EndMagicPowerCredit);
    }

    public static bool IsParameterFamily(BrickFamily family)
    {
        return (family >= BrickFamily.BeginCombatParameter && family <= BrickFamily.EndCombatParameter) ||
               (family >= BrickFamily.BeginMagicParameter && family <= BrickFamily.EndMagicParameter) ||
               (family >= BrickFamily.BeginFaberParameter && family <= BrickFamily.EndFaberParameter) ||
               (family >= BrickFamily.BeginHarvestParameter && family <= BrickFamily.EndHarvestParameter) ||
               (family >= BrickFamily.BeginForageProspectionParameter && family <= BrickFamily.EndForageProspectionParameter) ||
               (family >= BrickFamily.BeginForageExtractionParameter && family <= BrickFamily.EndForageExtractionParameter) ||
               (family >= BrickFamily.BeginPowerParameter && family <= BrickFamily.EndPowerParameter);
    }

    public static bool IsCharacBuyFamily(BrickFamily family)
    {
        return (family >= BrickFamily.BeginCharacBuy && family <= BrickFamily.EndCharacBuy);
    }

    public static BrickType GetBrickType(BrickFamily family)
    {
        return family switch
        {
            >= BrickFamily.BeginCombat and <= BrickFamily.EndCombat => BrickType.COMBAT,
            >= BrickFamily.BeginMagic and <= BrickFamily.EndMagic => BrickType.MAGIC,
            >= BrickFamily.BeginFaber and <= BrickFamily.EndFaber => BrickType.FABER,
            >= BrickFamily.BeginHarvest and <= BrickFamily.EndHarvest => BrickType.HARVEST,
            >= BrickFamily.BeginForageProspection and <= BrickFamily.EndForageProspection => BrickType.FORAGE_PROSPECTION,
            >= BrickFamily.BeginForageExtraction and <= BrickFamily.EndForageExtraction => BrickType.FORAGE_EXTRACTION,
            >= BrickFamily.BeginSpecialPowers and <= BrickFamily.EndSpecialPowers => BrickType.SPECIAL_POWER,
            >= BrickFamily.BeginProcEnchantement and <= BrickFamily.EndProcEnchantement => BrickType.PROC_ENCHANTEMENT,
            >= BrickFamily.BeginTraining and <= BrickFamily.EndTraining => BrickType.TRAINING,
            >= BrickFamily.BeginTimedActions and <= BrickFamily.EndTimedActions => BrickType.TIMED_ACTION,
            >= BrickFamily.BeginBonus and <= BrickFamily.EndBonus => BrickType.BONUS,
            _ => BrickType.UNKNOWN
        };
    }

    public enum BrickType
    {
        MAGIC = 0,
        COMBAT,
        FABER,
        FORAGE_PROSPECTION,
        FORAGE_EXTRACTION,
        HARVEST,
        QUARTER,
        TRACKING,
        SHOPKEEPER,
        TRAINING,
        MISCELLANEOUS,
        COMMERCE,
        SPECIAL_POWER,
        PROC_ENCHANTEMENT,
        TIMED_ACTION,
        BRICK_TYPE_COUNT,
        BONUS,
        UNKNOWN // Warning: Shouldn't exceed 32
    };

}