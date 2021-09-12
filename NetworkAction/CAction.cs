using System;
using System.Reflection;

namespace RCC.NetworkAction
{
    public class CAction
    {
        public TActionCode Code { get; internal set; }
        public TActionCode PropertyCode { get; internal set; }
        public byte Slot { get; internal set; }

        public void unpack(CBitMemStream message)
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            return;
            //throw new System.NotImplementedException();
        }

        public int size()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            return 0;
            //throw new System.NotImplementedException();
        }

        internal void reset()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            //throw new NotImplementedException();
        }
    }
}