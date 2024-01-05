///////////////////////////////////////////////////////////////////
// This file contains modified code from 'Ryzom - MMORPG Framework'
// http://dev.ryzom.com/projects/ryzom/
// which is released under GNU Affero General Public License.
// http://www.gnu.org/licenses/
// Copyright 2010 Winch Gate Property Limited
///////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using Client.Sheet;

namespace Client.Stream
{
    // TODO: BitMemoryStream and BitStreamFile should use the same interface
    public class BitStreamFile
    {
        byte[] _fileBytes;
        uint _filePointer;

        public bool Open(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                _fileBytes = File.ReadAllBytes(path);
                _filePointer = 0;
                return true;
            }

            return false;
        }

        public void SerialCheck(uint value)
        {
            Serial(out uint read);
            if (read != value)
                throw new Exception("EInvalidDataStream(*this)");
        }

        public void SerialVersion(uint currentVersion)
        {
            uint v;

            Serial(out byte b);

            if (b == 0xFF)
                Serial(out v);
            else
                v = b;

            var streamVersion = v;

            // Exception test.
            var _ThrowOnOlder = true;
            if (_ThrowOnOlder && streamVersion < currentVersion)
                throw new Exception("EOlderStream(*this)");

            var _ThrowOnNewer = true;
            if (_ThrowOnNewer && streamVersion > currentVersion)
                throw new Exception("ENewerStream(*this)");
        }

        internal void Serial(out string sTmp)
        {
            var tmp = new byte[4];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            var contLen = BitConverter.ToInt32(tmp);
            _filePointer += 4;

            tmp = new byte[contLen];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            sTmp = System.Text.Encoding.UTF8.GetString(tmp);
            _filePointer += (uint)contLen;
        }

        private void Serial(out ulong value)
        {
            var tmp = new byte[8];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = BitConverter.ToUInt64(tmp);
        }

        public void Serial(out uint value)
        {
            var tmp = new byte[4];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = BitConverter.ToUInt32(tmp);
        }

        public void Serial(out ushort value)
        {
            var tmp = new byte[2];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = BitConverter.ToUInt16(tmp);
        }

        public void Serial(out byte value)
        {
            var tmp = new byte[1];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = tmp[0];
        }

        public void Serial(out float value)
        {
            var tmp = new byte[4];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = tmp[0];
        }

        public void SerialBuffer(out byte[] b, in uint dependBlockSize)
        {
            b = new byte[dependBlockSize];
            Array.Copy(_fileBytes, _filePointer, b, 0, b.Length);
            _filePointer += dependBlockSize;
        }

        internal void Serial(out bool value)
        {
            var tmp = new byte[1];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = BitConverter.ToBoolean(tmp, 0);
        }

        internal void SerialCont(out List<ushort> container)
        {
            container = new List<ushort>();

            Serial(out uint len);

            for (var i = 0; i < len; i++)
            {
                Serial(out ushort value);

                container.Add(value);
            }
        }

        internal void SerialCont(out List<string> container)
        {
            container = new List<string>();

            Serial(out uint len);

            for (var i = 0; i < len; i++)
            {
                Serial(out string value);

                container.Add(value);
            }
        }

        internal void SerialCont(out List<ulong> container)
        {
            container = new List<ulong>();

            Serial(out uint len);

            for (var i = 0; i < len; i++)
            {
                Serial(out ulong value);

                container.Add(value);
            }
        }

        public void SerialCont(out SortedDictionary<SheetId, SheetManagerEntry> container, RyzomClient client)
        {
            Serial(out uint len);

            container = new SortedDictionary<SheetId, SheetManagerEntry>();

            for (var i = 0; i < len; i++)
            {
                Serial(out uint sheetId);

                var sme = new SheetManagerEntry(client);

                sme.Serial(this);

                container.Add((SheetId)client.GetSheetIdFactory().SheetId(sheetId), sme);
            }
        }

        public void Close()
        {
            _fileBytes = null;
            _filePointer = 0;
        }
    }
}