///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Client.Client;
using Client.Entity;
using Client.Stream;

namespace Client.Sheet
{
    public class CharacterSheet : EntitySheet
    {
        private readonly SheetIdFactory _sheetIdFactory;

        /// <summary>Character Gender.</summary>
        public byte Gender;
        /// <summary>Character Race</summary>
        public PeopleType Race = PeopleType.Undefined;
        /// <summary>Character's skeleton.</summary>
        public uint IdSkelFilename = new uint();
        /// <summary>Base Name of the animation set.</summary>
        public uint IdAnimSetBaseName = new uint();
        /// <summary>Automaton Type</summary>
        public uint IdAutomaton = new uint();
        public float Scale;
        /// <summary>The sound family (used for sound context var 2)</summary>
        public uint SoundFamily;
        /// <summary>The sound variation (used for sound context var 3)</summary>
        public uint SoundVariation;
        /// <summary>Lod Character.</summary>
        public uint IdLodCharacterName = new uint();
        public float LodCharacterDistance;
        /// <summary>value to scale the "pos" channel of the animation of the creature.</summary>
        public float CharacterScalePos;
        /// <summary>The name of the faction the creature belongs to</summary>
        public uint IdFame = new uint();

        /// <summary>Possible(impossible) Actions.</summary>
        public bool Selectable;
        public bool Talkable;
        public bool Attackable;
        public bool Givable;
        public bool Mountable;
        public bool Turn;
        public bool SelectableBySpace;

        /// <summary>Equipment worm or creature body.</summary>
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

        /// NLMISC::TSStringId			IdFirstName;
        /// NLMISC::TSStringId			IdLastName;

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

        /// <summary>display the entity in the radar</summary>
        public bool DisplayInRadar;
        /// <summary>name is displayed if (DisplayOSD && DisplayName)</summary>
        public bool DisplayOSDName;
        /// <summary>bars are displayed if (DisplayOSD && DisplayBars)</summary>
        public bool DisplayOSDBars;
        /// <summary>even if ClientCfg.ShowNameUnderCursor==false, force OSD to display when under cursor (DisplayOSD must be true)</summary>
        public bool DisplayOSDForceOver;
        /// <summary>the client can traverse this entity after some force time</summary>
        public bool Traversable;

        /// <summary>Name positions on Z axis</summary>
        public float NamePosZLow;
        public float NamePosZNormal;
        public float NamePosZHigh;

        /// <summary>Alternative Look</summary>
        public List<uint> IdAlternativeClothes = new List<uint>();

        /// <summary>Hair Item List</summary>
        public List<Equipment> HairItemList = new List<Equipment>();

        /// <summary>name of static FX played on entity (empty if none)</summary>
        public uint IdStaticFX = new uint();

        /// <summary>spell casting prefix. This prefix is used to see which sheet contains dates about spell casting</summary>
        public uint SpellCastingPrefix = new uint();

        /// <summary>attack lists filenames</summary>
        public List<uint> AttackLists = new List<uint>();

        // consider
        /// <summary>Force depending on the region the creature belongs</summary>
        public byte RegionForce = new byte();
        /// <summary>Level of creature inside the same region</summary>
        public byte ForceLevel = new byte();
        /// <summary>Precise level of the creature</summary>
        public ushort Level;

        public bool R2Npc;

        /// <summary>
        /// Constructor
        /// </summary>
        public CharacterSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {
            _sheetIdFactory = sheetIdFactory;

            CharacterScalePos = 1;
            Scale = 1.0f;
            SoundFamily = 0;
            SoundVariation = 0;
            //Type				= CEntitySheet::FAUNA;
            Gender = 0;
            //Race				= EGSPD::CPeople::EndPeople;
            IdSkelFilename = 0;
            IdAnimSetBaseName = 0;
            IdAutomaton = 0;
            IdLodCharacterName = 0;
            LodCharacterDistance = 0.0f;
            IdFame = 0;
            DisplayOSD = true;
            DisplayInRadar = true;
            DisplayOSDName = true;
            DisplayOSDBars = true;
            DisplayOSDForceOver = false;
            Traversable = true;
            ClipRadius = 0.0f;
            ClipHeight = 0.0f;
            R2Npc = false;

            Selectable = false;
            Talkable = false;
            Attackable = false;
            Givable = false;
            Mountable = false;
            Turn = false;
            SelectableBySpace = false;

            //HLState				= LHSTATE::NONE;

            HairColor = 0;
            Skin = 0;
            EyesColor = 0;

            DistToFront = 0.0f;
            DistToBack = 0.0f;
            DistToSide = 0.0f;

            ColRadius = 0.0f;
            ColHeight = 0.0f;
            ColLength = 0.0f;
            ColWidth = 0.0f;

            MaxSpeed = 0.0f;

            NamePosZLow = 0.0f;
            NamePosZNormal = 0.0f;
            NamePosZHigh = 0.0f;

            IdStaticFX = 0;

            SpellCastingPrefix = 0;

            RegionForce = 0;
            ForceLevel = 0;
            Level = 0;
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
            // TODO: Merge both serial methods
        }

        public override void Serial(BitStreamFile f)
        {
            // Serialize class components.
            //	ClientSheetsStrings.Serial(f, IdFirstName);
            //	ClientSheetsStrings.Serial(f, IdLastName);
            f.Serial(out Gender);
            f.Serial(out uint race);
            Race = (PeopleType)race;
            ClientSheetsStringsSerial(f, out IdSkelFilename);
            ClientSheetsStringsSerial(f, out IdAnimSetBaseName);
            ClientSheetsStringsSerial(f, out IdAutomaton);

            throw new NotImplementedException();
            //f.Serial(out Scale);
            //f.Serial(out SoundFamily);
            //f.Serial(out SoundVariation);
            //ClientSheetsStringsSerial(f, out IdLodCharacterName);
            //
            //f.Serial(out LodCharacterDistance);
            //f.Serial(out Selectable);
            //f.Serial(out Talkable);
            //f.Serial(out Attackable);
            //f.Serial(out Givable);
            //f.Serial(out Mountable);
            //f.Serial(out Turn);
            //f.Serial(out SelectableBySpace);
            //
            //f.Serial(out uint HLState);
            //
            //f.Serial(out CharacterScalePos);
            //f.Serial(out NamePosZLow);
            //f.Serial(out NamePosZNormal);
            //f.Serial(out NamePosZHigh);
            //ClientSheetsStringsSerial(f, out IdFame);
            //
            //f.Serial(out Body);
            //f.Serial(out Legs);
            //f.Serial(out Arms);
            //f.Serial(out Hands);
            //f.Serial(out Feet);
            //f.Serial(out Head);
            //f.Serial(out Face);
            //f.Serial(out ObjectInRightHand);
            //f.Serial(out ObjectInLeftHand);
            //
            //f.Serial(out HairColor);
            //f.Serial(out Skin);
            //f.Serial(out EyesColor);
            //
            //f.Serial(out DistToFront);
            //f.Serial(out DistToBack);
            //f.Serial(out DistToSide);
            //
            //// Collisions
            //f.Serial(out ColRadius);
            //f.Serial(out ColHeight);
            //f.Serial(out ColLength);
            //f.Serial(out ColWidth);
            //f.Serial(out MaxSpeed);
            //
            //// Clip
            //f.Serial(out ClipRadius);
            //f.Serial(out ClipHeight);
            //
            //// Alternative Look
            //ClientSheetsStringsSerial(f, out IdAlternativeClothes);
            //
            //// Hair Item List
            //f.SerialCont(HairItemList);
            //// Ground fxs
            //f.SerialCont(out List<ushort> GroundFX);
            //// Display OSD
            //f.Serial(out DisplayOSD);
            //// static FX
            //ClientSheetsStringsSerial(f, out IdStaticFX);
            //// body to bone
            //f.Serial(out BodyToBone);
            //// attack list
            //f.Serial(out uint size);
            //AttackLists = new List<uint>((int)size);
            //
            //for (int k = 0; k < size; ++k)
            //{
            //    ClientSheetsStringsSerial(f, out string lst);
            //    AttackLists[k] = uint.Parse(lst);
            //}
            //
            //// bot object flags
            //f.Serial(out DisplayInRadar);
            //f.Serial(out DisplayOSDName);
            //f.Serial(out DisplayOSDBars);
            //f.Serial(out DisplayOSDForceOver);
            //f.Serial(out Traversable);
            //
            //f.Serial(out RegionForce);
            //f.Serial(out ForceLevel);
            //f.Serial(out Level);
            //
            //f.SerialCont(out List<ushort> ProjectileCastRay);
            //
            //f.Serial(out R2Npc);
        }

        public void ClientSheetsStringsSerial(BitStreamFile f, out uint strId)
        {
            string tmp = "";
            f.Serial(out tmp);

            // TODO: Use the mapper here

            strId = 0;
        }
    }
}
