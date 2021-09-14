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

        public virtual void unpack(CBitMemStream message)
        {
            ConsoleIO.WriteLineFormatted("§c" + MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            //throw new System.NotImplementedException();
        }

        public abstract int size();

        public abstract void reset();

        public abstract void pack(CBitMemStream message);
    }
}