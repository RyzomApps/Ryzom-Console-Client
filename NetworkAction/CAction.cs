using System;

namespace RCC.NetworkAction
{
    public class CAction
    {
        public TActionCode Code { get; internal set; }
        public TActionCode PropertyCode { get; internal set; }
        public byte Slot { get; internal set; }

        public void unpack(CBitMemStream message)
        {
            return;
            //throw new System.NotImplementedException();
        }

        public int size()
        {
            return 0;
            //throw new System.NotImplementedException();
        }

        internal void reset()
        {
            //throw new NotImplementedException();
        }
    }
}