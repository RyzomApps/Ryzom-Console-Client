using System;
using System.Collections.Generic;
using System.IO;
using Client.Brick;
using Client.Phrase;

namespace Client.Sheet
{
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

        public void Serial(out uint value)
        {
            var tmp = new byte[4];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = BitConverter.ToUInt32(tmp);
        }

        public void Serial(out byte value)
        {
            var tmp = new byte[1];
            Array.Copy(_fileBytes, _filePointer, tmp, 0, tmp.Length);
            _filePointer += (uint)tmp.Length;
            value = tmp[0];
        }

        public void SerialBuffer(ref byte[] b, in uint dependBlockSize)
        {
            b = new byte[dependBlockSize];
            Array.Copy(_fileBytes, _filePointer, b, 0, b.Length);
            _filePointer += dependBlockSize;
        }

        public void SerialCont(ref SortedDictionary<SheetId, SheetManagerEntry> container, SheetIdFactory sheetIdFactory)
        {
            Serial(out uint len);

            for (var i = 0; i < len; i++)
            {
                Serial(out uint sheetId);

                Serial(out uint intType);

                SheetId s = sheetIdFactory.SheetId(sheetId);

                var type = (EntitySheet.TType)intType;

                EntitySheet es;

                switch (type)
                {
                    case EntitySheet.TType.SBRICK:
                        es = new BrickSheet(sheetIdFactory);
                        InitSheet(es, this, type);
                        break;

                    case EntitySheet.TType.SPHRASE:
                        es = new PhraseSheet(sheetIdFactory);
                        InitSheet(es, this, type);
                        break;

                    case EntitySheet.TType.@sbyte:
                    case EntitySheet.TType.FAUNA:
                    case EntitySheet.TType.FLORA:
                    case EntitySheet.TType.OBJECT:
                    case EntitySheet.TType.FX:
                    case EntitySheet.TType.BUILDING:
                    case EntitySheet.TType.ITEM:
                    case EntitySheet.TType.PLANT:
                    case EntitySheet.TType.MISSION:
                    case EntitySheet.TType.RACE_STATS:
                    case EntitySheet.TType.PACT:
                    case EntitySheet.TType.LIGHT_CYCLE:
                    case EntitySheet.TType.WEATHER_SETUP:
                    case EntitySheet.TType.CONTINENT:
                    case EntitySheet.TType.WORLD:
                    case EntitySheet.TType.WEATHER_FUNCTION_PARAMS:
                    case EntitySheet.TType.UNKNOWN:
                    case EntitySheet.TType.BOTCHAT:
                    case EntitySheet.TType.MISSION_ICON:
                    case EntitySheet.TType.SKILLS_TREE:
                    case EntitySheet.TType.UNBLOCK_TITLES:
                    case EntitySheet.TType.SUCCESS_TABLE:
                    case EntitySheet.TType.AUTOMATON_LIST:
                    case EntitySheet.TType.ANIMATION_SET_LIST:
                    case EntitySheet.TType.SPELL:
                    case EntitySheet.TType.SPELL_LIST:
                    case EntitySheet.TType.CAST_FX:
                    case EntitySheet.TType.EMOT:
                    case EntitySheet.TType.ANIMATION_FX:
                    case EntitySheet.TType.ID_TO_STRING_ARRAY:
                    case EntitySheet.TType.FORAGE_SOURCE:
                    case EntitySheet.TType.CREATURE_ATTACK:
                    case EntitySheet.TType.ANIMATION_FX_SET:
                    case EntitySheet.TType.ATTACK_LIST:
                    case EntitySheet.TType.SKY:
                    case EntitySheet.TType.TEXT_EMOT:
                    case EntitySheet.TType.OUTPOST:
                    case EntitySheet.TType.OUTPOST_SQUAD:
                    case EntitySheet.TType.OUTPOST_BUILDING:
                    case EntitySheet.TType.FACTION:
                    case EntitySheet.TType.TypeCount:
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                container.Add(s, new SheetManagerEntry() { EntitySheet = es });
            }
        }

        /// <summary>
        /// Useful for serial
        /// </summary>
        public void InitSheet(EntitySheet pES, BitStreamFile s, EntitySheet.TType type)
        {
            if (pES != null)
            {
                pES.Id.Serial(s);
                pES.Serial(s);
                pES.Type = type;
                //SheetMngr.processSheet(pES);
            }
        }


        public void Close()
        {
            _fileBytes = null;
            _filePointer = 0;
        }
    }
}