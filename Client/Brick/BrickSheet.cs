///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Client.Sheet;
using Client.Stream;

namespace Client.Brick;

public class BrickSheet(SheetIdFactory sheetIdFactory) : Sheet.Sheet(sheetIdFactory)
{
    public string IdIcon;

    public byte IndexInFamily { get; private set; }

    public BrickFamily BrickFamily { get; private set; } = BrickFamily.Unknown;

    public bool IsRoot()
    {
        return BrickFamilyHelper.IsRootFamily(BrickFamily);
    }

    public bool IsCombat()
    {
        return BrickFamilyHelper.GetBrickType(BrickFamily) == BrickFamilyHelper.BrickType.COMBAT;
    }

    public override void Serial(BitMemoryStream f)
    {
        // No serialization logic here
    }

    public override void Serial(BitStreamFile s)
    {
        s.Serial(out uint usedSkills);

        for (var i = 0; i < usedSkills; i++)
        {
            s.Serial(out uint skill);
        }

        s.Serial(out uint brickFamily);
        BrickFamily = (BrickFamily)brickFamily;

        s.Serial(out byte indexInFamily);
        IndexInFamily = indexInFamily;

        s.Serial(out byte level);

        s.Serial(out string sTmp);

        s.Serial(out IdIcon);
        s.Serial(out string idIconBack);
        s.Serial(out string idIconOver);
        s.Serial(out string idIconOver2);

        s.Serial(out uint iconColor);
        s.Serial(out uint iconBackColor);
        s.Serial(out uint iconOverColor);
        s.Serial(out uint iconOver2Color);

        s.Serial(out uint sabrinaCost);

        s.Serial(out uint sabrinaRelativeCost);

        s.SerialCont(out List<ushort> mandatoryFamilies);
        s.SerialCont(out List<ushort> optionalFamilies);
        s.SerialCont(out List<ushort> parameterFamilies);
        s.SerialCont(out List<ushort> creditFamilies);

        s.Serial(out string idForbiddenDef);
        s.Serial(out string idForbiddenExclude);

        // begin workaround: s.Serial(out uint FaberPlan);
        s.Serial(out uint itemBuilt);
        s.SerialCont(out List<ulong> itemPartMps);
        s.SerialCont(out List<ulong> formulaMps);

        s.Serial(out uint toolType);
        s.Serial(out uint nbItemBuilt);
        // end workaround

        s.SerialCont(out List<string> properties);
        s.Serial(out byte minCastTime);
        s.Serial(out byte maxCastTime);
        s.Serial(out byte minRange);
        s.Serial(out byte maxRange);

        s.Serial(out uint brickRequiredFlags1);
        s.Serial(out uint brickRequiredFlags2);

        s.Serial(out uint sPCost);

        s.Serial(out uint actionNature);

        s.Serial(out uint numRequiredOneOfSkills);
        for (var i = 0; i < numRequiredOneOfSkills; i++)
        {
            s.SerialBuffer(out var arr1, 8);
        }

        s.Serial(out uint numRequiredSkills);
        for (var i = 0; i < numRequiredSkills; i++)
        {
            s.SerialBuffer(out var arr2, 8);
        }

        s.Serial(out uint numRequiredBricks);
        for (var i = 0; i < numRequiredBricks; i++)
        {
            s.SerialBuffer(out var arr3, 4);
        }

        s.Serial(out bool avoidCyclic);

        s.Serial(out bool usableWithEmptyHands);

        s.Serial(out uint civRestriction);

        s.Serial(out uint factionIndex);

        s.Serial(out uint minFameValue);

        s.Serial(out uint magicResistType);
    }
}