///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Client;
using Client.Sheet;
using Client.Stream;

namespace Client.Messages
{
    /// <summary>Message to create a character.</summary>
    /// <author>PUZIN Guillaume (GUIGUI)</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    public class CreateCharMsg
    {
        public byte Slot;

        public SheetId SheetId;

        public uint Mainland; //mainland where char is
        public string Name = ""; //character name choose by player
        public byte People; //use people.h enum
        public byte Sex; //use gender.h enum

        //nb points allocated to role (0 not get, else between 1 to 3)
        public byte NbPointFighter;
        public byte NbPointCaster;
        public byte NbPointCrafter;
        public byte NbPointHarvester;

        public int StartPoint; //enum of starting point choosing by player (byte?)

        public byte GabaritHeight;
        public byte GabaritTorsoWidth;
        public byte GabaritArmsWidth;
        public byte GabaritLegsWidth;
        public byte GabaritBreastSize;

        public byte MorphTarget1; // 0 - 7
        public byte MorphTarget2;
        public byte MorphTarget3;
        public byte MorphTarget4;
        public byte MorphTarget5;
        public byte MorphTarget6;
        public byte MorphTarget7;
        public byte MorphTarget8;
        public byte EyesColor; // 0 - 7
        public byte Tattoo; // 0 = neutral, 1 - 15 Tattoo

        // hair
        public byte HairType; // 0 - 3
        public byte HairColor; // 0 - 5

        // color for equipement slots (Only for pre-equipped perso created with sheet)
        public byte JacketColor;
        public byte TrousersColor;
        public byte HatColor;
        public byte ArmsColor;
        public byte HandsColor;
        public byte FeetColor;

        // 101 Aniro        fr
        // 102 Leanon       de
        // 103 Arispotle    en
        // 301 Yubo         yubo
        internal void SetupFromCharacterSummary(CharacterSummary cs, SheetIdFactory sheetIdFactory)
        {
            Slot = 1;
            SheetId = sheetIdFactory.Unknown;
            Mainland = 101; //cs.Mainland; TODO remove tests
            Name = cs.Name;
            People = 3; // Zorai //(byte)cs.People; TODO remove tests
            Sex = 0; // cs.VisualPropA.PropertySubData.Sex;

            HairType = 0; // cs.VisualPropA.PropertySubData.HatModel;
            HairColor = 0; // cs.VisualPropA.PropertySubData.HatColor;

            GabaritHeight = 0; // cs.VisualPropC.PropertySubData.CharacterHeight;
            GabaritTorsoWidth = 0; // cs.VisualPropC.PropertySubData.TorsoWidth;
            GabaritArmsWidth = 0; // cs.VisualPropC.PropertySubData.ArmsWidth;
            GabaritLegsWidth = 0; // cs.VisualPropC.PropertySubData.LegsWidth;
            GabaritBreastSize = 0; // cs.VisualPropC.PropertySubData.BreastSize;

            // color for equipement slots
            JacketColor = 0; // cs.VisualPropA.PropertySubData.JacketColor;
            TrousersColor = 0; // cs.VisualPropA.PropertySubData.TrouserColor;
            HatColor = 0; // cs.VisualPropA.PropertySubData.HatColor;
            ArmsColor = 0; // cs.VisualPropA.PropertySubData.ArmColor;
            HandsColor = 0; // cs.VisualPropB.PropertySubData.HandsColor;
            FeetColor = 0; // cs.VisualPropB.PropertySubData.FeetColor;

            // blend shapes
            MorphTarget1 = 0; // cs.VisualPropC.PropertySubData.MorphTarget1;
            MorphTarget2 = 0; // cs.VisualPropC.PropertySubData.MorphTarget2;
            MorphTarget3 = 0; // cs.VisualPropC.PropertySubData.MorphTarget3;
            MorphTarget4 = 0; // cs.VisualPropC.PropertySubData.MorphTarget4;
            MorphTarget5 = 0; // cs.VisualPropC.PropertySubData.MorphTarget5;
            MorphTarget6 = 0; // cs.VisualPropC.PropertySubData.MorphTarget6;
            MorphTarget7 = 0; // cs.VisualPropC.PropertySubData.MorphTarget7;
            MorphTarget8 = 0; // cs.VisualPropC.PropertySubData.MorphTarget8;

            // eyes color
            EyesColor = 0; // cs.VisualPropC.PropertySubData.EyesColor;

            // tattoo number
            Tattoo = 0; // cs.VisualPropC.PropertySubData.Tattoo;
        }

        public void SerialBitMemStream(BitMemoryStream f)
        {
            f.Serial(ref Slot);

            // Serialise SheetId, used for create character with sheet for tests
            var sheetId = SheetId.AsInt();
            f.Serial(ref sheetId);

            // Serialize the user character.
            f.Serial(ref Mainland);
            f.Serial(ref Name);

            f.Serial(ref People);
            f.Serial(ref Sex);

            f.Serial(ref NbPointFighter);
            f.Serial(ref NbPointCaster);
            f.Serial(ref NbPointCrafter);
            f.Serial(ref NbPointHarvester);

            f.Serial(ref StartPoint);

            f.Serial(ref HairType);
            f.Serial(ref HairColor);

            f.Serial(ref GabaritHeight); // 0 - 15
            f.Serial(ref GabaritTorsoWidth);
            f.Serial(ref GabaritArmsWidth);
            f.Serial(ref GabaritLegsWidth);
            f.Serial(ref GabaritBreastSize);

            f.Serial(ref MorphTarget1); // 0 - 7
            f.Serial(ref MorphTarget2);
            f.Serial(ref MorphTarget3);
            f.Serial(ref MorphTarget4);
            f.Serial(ref MorphTarget5);
            f.Serial(ref MorphTarget6);
            f.Serial(ref MorphTarget7);
            f.Serial(ref MorphTarget8);
            f.Serial(ref EyesColor); // 0 - 7
            f.Serial(ref Tattoo); // 0 = neutral, 1 - 64 Tattoo

            // color for equipement slots (Only for pre-equipped perso created with sheet)
            f.Serial(ref JacketColor);
            f.Serial(ref TrousersColor);
            f.Serial(ref HatColor);
            f.Serial(ref ArmsColor);
            f.Serial(ref HandsColor);
            f.Serial(ref FeetColor);

            //Debug.Print(f.DebugData);
        }
    }
}