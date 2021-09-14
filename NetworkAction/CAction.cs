using System.Reflection;
using RCC.Helper;
using RCC.Network;

namespace RCC.NetworkAction
{
    public abstract class CAction
    {
        public TActionCode Code { get; internal set; }
        public TActionCode PropertyCode { get; internal set; }
        public byte Slot { get; internal set; }

        public abstract void unpack(CBitMemStream message);

        public abstract int size();

        public abstract void reset();

        public abstract void pack(CBitMemStream message);
    }
}