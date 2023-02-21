///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using Client.Stream;

namespace Client.Entity
{
    public class Equipment
    {
        public uint IdItem;
        public uint IdBindPoint;
        public byte Texture;
        public byte Color;

        public Equipment()
        {
            IdItem = 0;
            IdBindPoint = 0;
            Texture = 0;
            Color = 0;
        }

        //public string getItem()
        //{
        //    return ClientSheetsStrings.get(IdItem);
        //}
        //
        //public string getBindPoint()
        //{
        //    return ClientSheetsStrings.get(IdBindPoint);
        //}

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        public virtual void Serial(BitMemoryStream f)
        {
            //ClientSheetsStrings.serial(f, IdItem);
            //f.serial(Texture);
            //f.serial(Color);
            //ClientSheetsStrings.serial(f, IdBindPoint);
        }
    }

}
