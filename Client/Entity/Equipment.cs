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
        //private readonly RyzomClient _client;

        public Equipment(/*RyzomClient client*/)
        {
            //_client = client;
            IdItem = 0;
            IdBindPoint = 0;
            Texture = 0;
            Color = 0;
        }

        //public string getItem()
        //{
        //    return ClientSheetsStrings.get(IdItem);
        //}
        
        //public string getBindPoint()
        //{
        //    return ClientSheetsStrings.get(IdBindPoint);
        //}

        /// <summary>
        /// Serialize character sheet into binary data file.
        /// </summary>
        public virtual void Serial(BitStreamFile f)
        {
            // workaround = ClientSheetsStrings.serial(f, IdItem);
            f.Serial(out string a);
            f.Serial(out Texture);
            f.Serial(out Color);
            // workaround ClientSheetsStrings.serial(f, IdBindPoint);
            f.Serial(out string b);
        }
    }

}
