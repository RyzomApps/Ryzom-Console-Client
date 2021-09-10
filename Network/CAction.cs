namespace RCC.Network
{
    public class CAction
    {
        public TActionCode Code { get; internal set; }

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
    }
}