///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics.CodeAnalysis;
using API.Sheet;
using Client.Stream;

namespace Client.Sheet
{
    /// <summary>
    /// SheetId
    /// </summary>
    /// <author>Stephane Coutelas</author>
    /// <author>Nevrax France</author>
    /// <date>2002</date>
    /// <remarks>
    /// This class is case insensitive. It means that you can call build() and 
    /// buildIdVector() with string with anycase, it'll work.
    /// </remarks>
    public class SheetId : IComparable<SheetId>, ISheetId
    {
        private readonly SheetIdFactory _sheetIdFactory;

        internal uint _id;
        internal uint _type;

        public uint Id
        {
            get => _id;
            set => _id = value;
        }

        public uint Type
        {
            get => _type;
            set => _type = value;
        }

        public string Name => ToString();

        /// <summary>
        /// Constructor
        /// </summary>
        public SheetId(SheetIdFactory sheetIdFactory)
        {
            _sheetIdFactory = sheetIdFactory;
        }

        /// <summary>
        /// Return the **whole** sheet id (id+type)
        /// </summary>
        public uint AsInt()
        {
            return _id;
        }

        /// <summary>
        /// Serial
        /// </summary>
        public void Serial(BitMemoryStream f)
        {
            f.Serial(ref _id);
        }

        /// <summary>
        /// Serial
        /// </summary>
        internal void Serial(BitStreamFile s)
        {
            s.Serial(out _id);
        }

        public int CompareTo([AllowNull] SheetId other)
        {
            return Id.CompareTo(other.Id);
        }

        public void BuildSheetId(int shortId, SheetType type)
        {
            // TODO: BuildSheetId implementation!
            _id = (uint)shortId;
            _type = (uint)type;

            //_sheetIdFactory.SheetId(shortId);
        }

        /// <summary>
        /// Return the sheet id as a string
        /// If the sheet id is not found, then:
        /// - if 'ifNotFoundUseNumericId==false' the returned string is "<Sheet %d not found in sheet_id.bin>" with the id in %d
        /// - if 'ifNotFoundUseNumericId==tue'   the returned string is "#%u" with the id in %u
        /// </summary>
        public override string ToString()
        {
            return _sheetIdFactory.ToString(this, false);
        }

        /// <inheritdoc cref="ToString()"/>
        public string ToString(bool ifNotFoundUseNumericId)
        {
            return _sheetIdFactory.ToString(this, ifNotFoundUseNumericId);
        }
    }
}