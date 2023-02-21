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
    /// <summary>
    /// Class to manage the player sheet.
    /// </summary>
    /// <author>Guillaume PUZIN (GUIGUI)</author>
    /// <author>Nevrax France</author>
    /// <date>2001</date>
    public class PlayerSheet : EntitySheet
    {
        private readonly SheetIdFactory _sheetIdFactory;
        public Equipment Ammo = new Equipment();
        public string AnimSetBaseName = "";
        public Equipment AnkleL = new Equipment();
        public Equipment AnkleR = new Equipment();
        public Equipment Arms = new Equipment();
        public Equipment Back = new Equipment();

        /// <summary>
        /// Equipment for player
        /// </summary>
        public Equipment Body = new Equipment();

        /// <summary>
        /// caracs
        /// </summary>
        public ushort[] Characteristics = new ushort[0];

        /// <summary>
        /// value to scale the "pos" channel of the animation of the player.
        /// </summary>
        public float CharacterScalePos;

        public string DefaultArms = "";
        public string DefaultChest = "";

        /// <summary>
        /// Default player look.
        /// </summary>
        public string DefaultFace = "";

        public string DefaultFeet = "";
        public string DefaultHair = "";
        public string DefaultHands = "";
        public string DefaultLegs = "";
        public Equipment EarL = new Equipment();
        public Equipment EarR = new Equipment();
        public Equipment Face = new Equipment();
        public Equipment Feet = new Equipment();
        public Equipment FingerL = new Equipment();
        public Equipment FingerR = new Equipment();

        /// <summary>
        /// Player Gender.
        /// </summary>
        public byte Gender;

        public Equipment Hands = new Equipment();
        public Equipment Head = new Equipment();
        public Equipment Headdress = new Equipment();
        public Equipment Legs = new Equipment();
        public float LodCharacterDistance;

        /// <summary>
        /// Lod Character.
        /// </summary>
        public string LodCharacterName = "";

        public Equipment Neck = new Equipment();
        public Equipment ObjectInLeftHand = new Equipment();
        public Equipment ObjectInRightHand = new Equipment();

        /// <summary>
        /// People of the player (FYROS, MATIS, ...)
        /// </summary>
        public PeopleType People = PeopleType.Undefined;

        public float Scale;
        public Equipment Shoulders = new Equipment();
        public string SkelFilename = "";

        /// <summary>
        /// skills
        /// </summary>
        public List<object> Skills = new List<object>();

        public Equipment WristL = new Equipment();
        public Equipment WristR = new Equipment();

        public PlayerSheet(SheetIdFactory sheetIdFactory) : base(sheetIdFactory)
        {
            _sheetIdFactory = sheetIdFactory;
        }

        public override void Serial(BitMemoryStream f)
        {
            throw new NotImplementedException();
        }

        public override void Serial(BitStreamFile s)
        {
            throw new NotImplementedException();
        }
    }
}