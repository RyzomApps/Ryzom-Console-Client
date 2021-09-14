using System.Reflection;
using RCC.Network;

namespace RCC.NetworkAction
{
    public abstract class CActionImpulsion : CAction
    {
        public bool AllowExceedingMaxSize;

        public override void reset()
        {
            AllowExceedingMaxSize = false;
        }
    }
}
