///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;

namespace Client.Network
{
    /// <summary>
    /// Detailed information about one inventory item, as sent by the
    /// server through ITEM_INFO:SET.
    /// Mirrors CItemInfos (common/src/game_share/item_infos.h).
    /// </summary>
    public class ItemInfos
    {
        public const int MaxMagicProtectionByJewel = 3;
        public const int NumMagicFactorType = 4;

        public uint SlotId;
        public ushort VersionInfo;
        public uint Hp;
        public uint HpMax;
        public uint CreatorName;
        public uint CurrentDamage;
        public uint MaxDamage;
        public float HitRate;
        public uint SapLoadCurrent;
        public uint SapLoadMax;
        public float Range;
        public int ParryModifier;
        public int DodgeModifier;
        public int AdversaryParryModifier;
        public int AdversaryDodgeModifier;
        public float ProtectionFactor;
        public uint MaxSlashingProtection;
        public uint MaxBluntProtection;
        public uint MaxPiercingProtection;
        public readonly int[] MagicProtection = new int[MaxMagicProtectionByJewel];
        public readonly uint[] MagicProtectionFactor = new uint[MaxMagicProtectionByJewel];
        public uint DesertMagicResistance;
        public uint ForestMagicResistance;
        public uint LacustreMagicResistance;
        public uint JungleMagicResistance;
        public uint PrimaryRootMagicResistance;
        public int HpBuff;
        public int SapBuff;
        public int StaBuff;
        public int FocusBuff;

        /// <summary>Enchantment/sap phrase (CSPhraseCom). Null when the item has none.</summary>
        public PhraseEnchantment Enchantment;

        public float WearEquipmentMalus;
        public int RequiredSkill;
        public ushort RequiredSkillLevel;
        public int RequiredSkill2;
        public ushort RequiredSkillLevel2;
        public int RequiredCharac;
        public ushort RequiredCharacLevel;
        public readonly List<TypeSkillMod> TypeSkillMods = new List<TypeSkillMod>();
        public readonly float[] CastingSpeedFactor = new float[NumMagicFactorType];
        public readonly float[] MagicPowerFactor = new float[NumMagicFactorType];
        public string CustomText = "";
        public string R2ItemDescription = "";
        public string R2ItemComment = "";
        public uint PetNumber;

        /// <summary>
        /// Reads one CItemInfos from the wire (same order as CItemInfos::serial).
        /// </summary>
        public void Read(Stream.BitMemoryStream impulse)
        {
            impulse.Serial(ref SlotId);
            impulse.Serial(ref VersionInfo);
            impulse.Serial(ref Hp);
            impulse.Serial(ref HpMax);
            impulse.Serial(ref CreatorName);
            impulse.Serial(ref CurrentDamage);
            impulse.Serial(ref MaxDamage);
            impulse.Serial(ref HitRate);
            impulse.Serial(ref SapLoadCurrent);
            impulse.Serial(ref SapLoadMax);
            impulse.Serial(ref Range);
            impulse.Serial(ref ParryModifier);
            impulse.Serial(ref DodgeModifier);
            impulse.Serial(ref AdversaryParryModifier);
            impulse.Serial(ref AdversaryDodgeModifier);
            impulse.Serial(ref ProtectionFactor);
            impulse.Serial(ref MaxSlashingProtection);
            impulse.Serial(ref MaxBluntProtection);
            impulse.Serial(ref MaxPiercingProtection);
            for (var i = 0; i < MaxMagicProtectionByJewel; i++)
            {
                impulse.Serial(ref MagicProtection[i]);
                impulse.Serial(ref MagicProtectionFactor[i]);
            }
            impulse.Serial(ref DesertMagicResistance);
            impulse.Serial(ref ForestMagicResistance);
            impulse.Serial(ref LacustreMagicResistance);
            impulse.Serial(ref JungleMagicResistance);
            impulse.Serial(ref PrimaryRootMagicResistance);
            impulse.Serial(ref HpBuff);
            impulse.Serial(ref SapBuff);
            impulse.Serial(ref StaBuff);
            impulse.Serial(ref FocusBuff);

            Enchantment = PhraseEnchantment.Read(impulse);

            impulse.Serial(ref WearEquipmentMalus);

            impulse.Serial(ref RequiredSkill);
            impulse.Serial(ref RequiredSkillLevel);
            impulse.Serial(ref RequiredSkill2);
            impulse.Serial(ref RequiredSkillLevel2);
            impulse.Serial(ref RequiredCharac);
            impulse.Serial(ref RequiredCharacLevel);

            var modCount = 0;
            impulse.Serial(ref modCount);
            for (var i = 0; i < modCount; i++)
            {
                var mod = new TypeSkillMod();
                mod.Read(impulse);
                TypeSkillMods.Add(mod);
            }

            for (var i = 0; i < NumMagicFactorType; i++)
            {
                impulse.Serial(ref CastingSpeedFactor[i]);
                impulse.Serial(ref MagicPowerFactor[i]);
            }

            impulse.Serial(ref CustomText);
            impulse.Serial(ref R2ItemDescription);
            impulse.Serial(ref R2ItemComment);
            impulse.Serial(ref PetNumber);
        }

        /// <summary>Human readable summary for the console log.</summary>
        public string ToLogLine()
        {
            var sb = new StringBuilder();
            sb.Append($"slot={SlotId} v={VersionInfo} hp={Hp}/{HpMax}");

            if (Enchantment != null && Enchantment.HasBricks)
                sb.Append($" enchant={Enchantment.Name} ({Enchantment.Bricks.Count} bricks)");

            if (HpBuff != 0) sb.Append($" hpBuff={HpBuff}");
            if (SapBuff != 0) sb.Append($" sapBuff={SapBuff}");
            if (StaBuff != 0) sb.Append($" staBuff={StaBuff}");
            if (FocusBuff != 0) sb.Append($" focusBuff={FocusBuff}");

            if (RequiredSkill != 0 || RequiredSkillLevel != 0)
                sb.Append($" reqSkill={RequiredSkill}:{RequiredSkillLevel}");
            if (RequiredCharac != 0 || RequiredCharacLevel != 0)
                sb.Append($" reqCharac={RequiredCharac}:{RequiredCharacLevel}");

            if (TypeSkillMods.Count > 0)
            {
                sb.Append(" mods=");
                for (var i = 0; i < TypeSkillMods.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(TypeSkillMods[i].ToString());
                }
            }

            if (CustomText.Length > 0)
                sb.Append($" customText=\"{CustomText}\"");

            return sb.ToString();
        }

        /// <summary>
        /// Detailed multi-line description of the enriched item info, one
        /// string per non-empty aspect, for the ItemDetail command.
        /// </summary>
        public List<string> ToDetailLines()
        {
            var lines = new List<string>();
            if (Hp != 0 || HpMax != 0) lines.Add($"hp: {Hp}/{HpMax}");
            if (HpBuff != 0) lines.Add($"hp buff: {HpBuff:+0;-0}");
            if (SapBuff != 0) lines.Add($"sap buff: {SapBuff:+0;-0}");
            if (StaBuff != 0) lines.Add($"sta buff: {StaBuff:+0;-0}");
            if (FocusBuff != 0) lines.Add($"focus buff: {FocusBuff:+0;-0}");

            if (CurrentDamage != 0 || MaxDamage != 0) lines.Add($"damage: {CurrentDamage}/{MaxDamage}");
            if (HitRate != 0) lines.Add($"hit rate: {HitRate:0.00}");
            if (Range != 0) lines.Add($"range: {Range:0.##}");
            if (ProtectionFactor != 0) lines.Add($"protection factor: {ProtectionFactor:0.00}");

            if (MaxSlashingProtection != 0 || MaxBluntProtection != 0 || MaxPiercingProtection != 0)
                lines.Add($"armor protection: slash {MaxSlashingProtection}, blunt {MaxBluntProtection}, pierce {MaxPiercingProtection}");
            if (DesertMagicResistance != 0 || ForestMagicResistance != 0 || LacustreMagicResistance != 0 || JungleMagicResistance != 0 || PrimaryRootMagicResistance != 0)
                lines.Add($"magic resist: desert {DesertMagicResistance}, forest {ForestMagicResistance}, lacustre {LacustreMagicResistance}, jungle {JungleMagicResistance}, root {PrimaryRootMagicResistance}");
            for (var i = 0; i < MaxMagicProtectionByJewel; i++)
                if (MagicProtection[i] != 0 || MagicProtectionFactor[i] != 0)
                    lines.Add($"magic protection {i + 1}: {MagicProtection[i]} (factor {MagicProtectionFactor[i]})");

            if (SapLoadMax != 0) lines.Add($"sap load: {SapLoadCurrent}/{SapLoadMax}");

            if (ParryModifier != 0) lines.Add($"parry modifier: {ParryModifier:+0;-0}");
            if (DodgeModifier != 0) lines.Add($"dodge modifier: {DodgeModifier:+0;-0}");
            if (AdversaryParryModifier != 0) lines.Add($"adversary parry modifier: {AdversaryParryModifier:+0;-0}");
            if (AdversaryDodgeModifier != 0) lines.Add($"adversary dodge modifier: {AdversaryDodgeModifier:+0;-0}");

            if (RequiredSkill != 0 || RequiredSkillLevel != 0)
                lines.Add($"requires skill: {RequiredSkill} level {RequiredSkillLevel}");
            if (RequiredSkill2 != 0 || RequiredSkillLevel2 != 0)
                lines.Add($"requires skill 2: {RequiredSkill2} level {RequiredSkillLevel2}");
            if (RequiredCharac != 0 || RequiredCharacLevel != 0)
                lines.Add($"requires characteristic: {RequiredCharac} level {RequiredCharacLevel}");
            if (WearEquipmentMalus != 0) lines.Add($"wear equipment malus: {WearEquipmentMalus:0.##}");

            for (var i = 0; i < NumMagicFactorType; i++)
                if (CastingSpeedFactor[i] != 0 || MagicPowerFactor[i] != 0)
                    lines.Add($"magic factor {i + 1}: speed x{CastingSpeedFactor[i]:0.##}, power x{MagicPowerFactor[i]:0.##}");

            if (TypeSkillMods.Count > 0)
                foreach (var mod in TypeSkillMods)
                    lines.Add($"craft bonus: {mod}");

            if (Enchantment != null && Enchantment.HasBricks)
                lines.Add($"enchantment: {Enchantment.Name} ({Enchantment.Bricks.Count} bricks)");

            if (CreatorName != 0) lines.Add($"creator name id: {CreatorName}");
            if (PetNumber != 0) lines.Add($"pet number: {PetNumber}");

            if (CustomText.Length > 0) lines.Add($"custom text: \"{CustomText}\"");
            if (R2ItemDescription.Length > 0) lines.Add($"description: {R2ItemDescription}");
            if (R2ItemComment.Length > 0) lines.Add($"comment: {R2ItemComment}");

            return lines;
        }
    }

    /// <summary>
    /// The enchantment phrase of an item, as wired in CItemInfos::Enchantment
    /// (CSPhraseCom serial: utf8 name, 16 bit compressed bricks, icon index).
    /// </summary>
    public class PhraseEnchantment
    {
        public string Name = "";
        public readonly List<ushort> Bricks = new List<ushort>();

        public bool HasBricks => Bricks.Count > 0;

        public static PhraseEnchantment Read(Stream.BitMemoryStream impulse)
        {
            var phrase = new PhraseEnchantment();

            // CSPhraseCom::serial stores the name as utf8 std::string
            impulse.Serial(ref phrase.Name, false);

            var compBrickCount = 0;
            impulse.Serial(ref compBrickCount);
            for (var i = 0; i < compBrickCount; i++)
            {
                ushort brick = 0;
                impulse.Serial(ref brick);
                phrase.Bricks.Add(brick);
            }

            return phrase;
        }
    }

    /// <summary>
    /// One type skill mod entry (CTypeSkillMod: sint32 modifier + sint32 enum type).
    /// </summary>
    public class TypeSkillMod
    {
        public int Modifier;
        public int Type;

        public void Read(Stream.BitMemoryStream impulse)
        {
            impulse.Serial(ref Modifier);
            impulse.Serial(ref Type);
        }

        public override string ToString()
        {
            return $"{Type}:{Modifier}";
        }
    }
}
