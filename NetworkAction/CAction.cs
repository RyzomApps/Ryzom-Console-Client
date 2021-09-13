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

        virtual public void unpack(CBitMemStream message)
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            //throw new System.NotImplementedException();
        }

        public abstract int size();

        internal void reset()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            //throw new NotImplementedException();
        }
    }
}