///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Client.Client;
using Client.Entity;
using Client.Stream;

namespace Client.Sheet
{
    public class CharacterSheet : EntitySheet
    {
        private readonly SheetIdFactory _sheetIdFactory;

        // Character Gender.
        public byte Gender;
        // Character Race
        public PeopleType Race = PeopleType.Undefined;
        // Character's skeleton.
        public uint IdSkelFilename = new uint();
        // Base Name of the animation set.
        public uint IdAnimSetBaseName = new uint();
        // Automaton Type
        public uint IdAutomaton = new uint();
        public float Scale;
        // The sound family (used for sound context var 2)
        public uint SoundFamily;
        // The sound variation (used for sound context var 3)
        public uint SoundVariation;
        // Lod Character.
        public uint IdLodCharacterName = new uint();
        public float LodCharacterDistance;
        // value to scale the "pos" channel of the animation of the creature.
        public float CharacterScalePos;
        // The name of the faction the creature belongs to
        public uint IdFame = new uint();

        // Possible(impossible) Actions.
        public bool Selectable;
        public bool Talkable;
        public bool Attackable;
        public bool Givable;
        public bool Mountable;
        public bool Turn;
        public bool SelectableBySpace;

        // Equipment worm or creature body.
        public Equipment Body = new Equipment();
        public Equipment Legs = new Equipment();
        public Equipment Arms = new Equipment();
        public Equipment Hands = new Equipment();
        public Equipment Feet = new Equipment();
        public Equipment Head = new Equipment();
        public Equipment Face = new Equipment();
        public Equipment ObjectInRightHand = new Equipment();
        public Equipment ObjectInLeftHand = new Equipment();
        // Yoyo: if you add some, modify getWholeEquipmentList()


        public byte HairColor = new byte();
        public byte Skin = new byte();
        public byte EyesColor = new byte();

        //	NLMISC::TSStringId			IdFirstName;
        //	NLMISC::TSStringId			IdLastName;

        public float DistToFront;
        public float DistToBack;
        public float DistToSide;

        public float ColRadius;
        public float ColHeight;
        public float ColLength;
        public float ColWidth;

        public float ClipRadius;
        public float ClipHeight;

        public float MaxSpeed;
        public bool DisplayOSD;
        // New flags created for bot objects
        public bool DisplayInRadar; // display the entity in the radar
        public bool DisplayOSDName; // name is displayed if (DisplayOSD && DisplayName)
        public bool DisplayOSDBars; // bars are displayed if (DisplayOSD && DisplayBars)
        public bool DisplayOSDForceOver; // even if ClientCfg.ShowNameUnderCursor==false, force OSD to display when under cursor (DisplayOSD must be true)
        public bool Traversable; // the client can traverse this entity after some force time

        // Name positions on Z axis
        public float NamePosZLow;
        public float NamePosZNormal;
        public float NamePosZHigh;

        // Alternative Look
        public List<uint> IdAlternativeClothes = new List<uint>();

        // Hair Item List
        public List<Equipment> HairItemList = new List<Equipment>();

        // name of static FX played on entity (empty if none)
        public uint IdStaticFX = new uint();

        // spell casting prefix. This prefix is used to see which sheet contains dates about spell casting
        public uint SpellCastingPrefix = new uint();

        // attack lists filenames
        public List<uint> AttackLists = new List<uint>();

        // consider
        public byte RegionForce = new byte(); // Force depending on the region the creature belongs
        public byte ForceLevel = new byte(); // Level of creature inside the same region
        public ushort Level; // Precise level of the creature

        public bool R2Npc;

        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {
            _sheetIdFactory = sheetIdFactory;
        }

        //public string getSkelFilename()
        //{
        //    return  ClientSheetsStrings.get(IdSkelFilename);
        //}
        //
        //public string getAnimSetBaseName()
        //{
        //    return ClientSheetsStrings.get(IdAnimSetBaseName);
        //}
        //
        //public string getAutomaton()
        //{
        //    return ClientSheetsStrings.get(IdAutomaton);
        //}
        //
        //public string getLodCharacterName()
        //{
        //    return ClientSheetsStrings.get(IdLodCharacterName);
        //}
        //
        //public string getFame()
        //{
        //    return ClientSheetsStrings.get(IdFame);
        //}
        //
        //public string getAlternativeClothes(uint i)
        //{
        //    return ClientSheetsStrings.get(IdAlternativeClothes[i]);
        //}
        //
        //public string getStaticFX()
        //{
        //    return ClientSheetsStrings.get(IdStaticFX);
        //}

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        public override void Serial(BitMemoryStream f)
        {
            //// Serialize class components.
            ////	ClientSheetsStrings.serial(f, IdFirstName);
            ////	ClientSheetsStrings.serial(f, IdLastName);
            //f.serial(Gender);
            //f.serialEnum(Race);
            //ClientSheetsStrings.serial(f, IdSkelFilename);
            //ClientSheetsStrings.serial(f, IdAnimSetBaseName);
            //ClientSheetsStrings.serial(f, IdAutomaton);
            //f.serial(Scale);
            //f.serial(SoundFamily);
            //f.serial(SoundVariation);
            //ClientSheetsStrings.serial(f, IdLodCharacterName);
            //
            //f.serial(LodCharacterDistance);
            //f.serial(Selectable);
            //f.serial(Talkable);
            //f.serial(Attackable);
            //f.serial(Givable);
            //f.serial(Mountable);
            //f.serial(Turn);
            //f.serial(SelectableBySpace);
            //f.serialEnum(HLState);
            //f.serial(CharacterScalePos);
            //f.serial(NamePosZLow);
            //f.serial(NamePosZNormal);
            //f.serial(NamePosZHigh);
            //ClientSheetsStrings.serial(f, IdFame);
            //
            //f.serial(Body);
            //f.serial(Legs);
            //f.serial(Arms);
            //f.serial(Hands);
            //f.serial(Feet);
            //f.serial(Head);
            //f.serial(Face);
            //f.serial(ObjectInRightHand);
            //f.serial(ObjectInLeftHand);
            //
            //f.serial(HairColor);
            //f.serial(Skin);
            //f.serial(EyesColor);
            //
            //f.serial(DistToFront);
            //f.serial(DistToBack);
            //f.serial(DistToSide);
            //
            //// Collisions
            //f.serial(ColRadius);
            //f.serial(ColHeight);
            //f.serial(ColLength);
            //f.serial(ColWidth);
            //f.serial(MaxSpeed);
            //
            //// Clip
            //f.serial(ClipRadius);
            //f.serial(ClipHeight);
            //
            //// Alternative Look
            //ClientSheetsStrings.serial(f, IdAlternativeClothes);
            //
            //// Hair Item List
            //f.serialCont(HairItemList);
            //// Ground fxs
            //f.serialCont(GroundFX);
            //// Display OSD
            //f.serial(DisplayOSD);
            //// static FX
            //ClientSheetsStrings.serial(f, IdStaticFX);
            //// body to bone
            //f.serial(BodyToBone);
            //// attack list
            //uint size = (uint)AttackLists.size();
            //f.serial(size);
            //AttackLists.resize(size);
            ////
            //for (uint k = 0; k < size; ++k)
            //{
            //    ClientSheetsStrings.serial(f, AttackLists[k]);
            //}
            //
            //// bot object flags
            //f.serial(DisplayInRadar);
            //f.serial(DisplayOSDName);
            //f.serial(DisplayOSDBars);
            //f.serial(DisplayOSDForceOver);
            //f.serial(Traversable);
            //
            //f.serial(RegionForce);
            //f.serial(ForceLevel);
            //f.serial(Level);
            //
            //f.serialCont(ProjectileCastRay);
            //
            //f.serial(R2Npc);
        }

        public override void Serial(BitStreamFile s)
        {
            throw new System.NotImplementedException();
        }
    }
}
