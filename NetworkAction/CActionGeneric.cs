using System;
using System.Reflection;

namespace RCC.NetworkAction
{
    public class CActionGeneric : CActionImpulsion
    {
        internal CBitMemStream get()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
            //throw new NotImplementedException();
            return null;
        }
    }
}