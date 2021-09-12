using System.Reflection;

namespace RCC.NetworkAction
{
    public class CActionImpulsion : CAction
    {
        ~CActionImpulsion() { }
        public bool AllowExceedingMaxSize;

        public CActionImpulsion()
        {
            ConsoleIO.WriteLine(MethodBase.GetCurrentMethod().Name + " called, but not implemented");
        }
    }
}
