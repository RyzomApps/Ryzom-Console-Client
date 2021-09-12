using System.Reflection;

namespace RCC.NetworkAction
{
    public class CActionGenericMultiPart : CActionImpulsion
    {
        public CActionGenericMultiPart()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
        }
    }
}
