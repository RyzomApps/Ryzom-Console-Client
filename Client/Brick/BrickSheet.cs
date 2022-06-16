///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;
using Client.Network;
using Client.Sheet;

namespace Client.Brick
{
    internal class BrickSheet : EntitySheet
    {
        public string IdIcon;

        public BrickSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {

        }

        public override void Build(object item)
        {

        }

        public override void Serial(BitMemoryStream f)
        {

        }

        public override void Serial(BitStreamFile s)
        {
            s.Serial(out uint UsedSkills);

            for (int i = 0; i < UsedSkills; i++)
            {
                s.Serial(out uint Skill);
            }

            s.Serial(out uint BrickFamily);

            s.Serial(out byte IndexInFamily);
            s.Serial(out byte Level);

            s.Serial(out string sTmp);

            s.Serial(out IdIcon);
            s.Serial(out string IdIconBack);
            s.Serial(out string IdIconOver);
            s.Serial(out string IdIconOver2);

            s.Serial(out uint IconColor);
            s.Serial(out uint IconBackColor);
            s.Serial(out uint IconOverColor);
            s.Serial(out uint IconOver2Color);

            s.Serial(out uint SabrinaCost);

            s.Serial(out uint SabrinaRelativeCost);

            s.SerialCont(out List<ushort> MandatoryFamilies);
            s.SerialCont(out List<ushort> OptionalFamilies);
            s.SerialCont(out List<ushort> ParameterFamilies);
            s.SerialCont(out List<ushort> CreditFamilies);

            s.Serial(out string IdForbiddenDef);
            s.Serial(out string IdForbiddenExclude);

            // begin workaround: s.Serial(out uint FaberPlan);
            s.Serial(out uint ItemBuilt);
            s.SerialCont(out List<ulong> ItemPartMps);
            s.SerialCont(out List<ulong> FormulaMps);

            s.Serial(out uint ToolType);
            s.Serial(out uint NbItemBuilt);
            // end workaround

            s.SerialCont(out List<string> Properties);
            s.Serial(out byte MinCastTime);
            s.Serial(out byte MaxCastTime);
            s.Serial(out byte MinRange);
            s.Serial(out byte MaxRange);

            s.Serial(out uint BrickRequiredFlags1);
            s.Serial(out uint BrickRequiredFlags2);

            s.Serial(out uint SPCost);

            s.Serial(out uint ActionNature);

            s.Serial(out uint numRequiredOneOfSkills);
            for (var i = 0; i < numRequiredOneOfSkills; i++)
            {
                s.SerialBuffer(out byte[] arr1, 8);
            }

            s.Serial(out uint numRequiredSkills);
            for (var i = 0; i < numRequiredSkills; i++)
            {
                s.SerialBuffer(out byte[] arr2, 8);
            }

            s.Serial(out uint numRequiredBricks);
            for (var i = 0; i < numRequiredBricks; i++)
            {
                s.SerialBuffer(out byte[] arr3, 4);
            }

            s.Serial(out bool AvoidCyclic);

            s.Serial(out bool UsableWithEmptyHands);

            s.Serial(out uint CivRestriction);

            s.Serial(out uint FactionIndex);

            s.Serial(out uint MinFameValue);

            s.Serial(out uint MagicResistType);

            Debug.Print("");
        }
    }
}