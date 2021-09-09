using System;
using System.Net;
using System.Net.Sockets;

namespace RCC
{
    class UdpSimSock
    {
        private UdpClient udpMain;

        public void connect(string frontendAddress)
        {
            ParseHostString(frontendAddress, out var host, out var port);

            udpMain = new UdpClient();
            udpMain.Connect(host, port);

            //udpMain.BeginReceive(Recv, udpMain);

            //udpMain.Receive();
        }

        public void ParseHostString(string hostString, out string hostName, out int port)
        {
            hostName = hostString;
            port = -1;

            if (!hostString.Contains(":")) return;

            var hostParts = hostString.Split(':');

            if (hostParts.Length != 2) return;

            hostName = hostParts[0];
            int.TryParse(hostParts[1], out port);
        }

        private void Recv(IAsyncResult ar)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var client = (UdpClient)ar.AsyncState;

            if (client == null) return;

            var received = client.EndReceive(ar, ref remoteIpEndPoint);

            // TODO: interpret result
            // decode header
            // msgin.serial(_CurrentReceivedNumber);
            // msgin.serial(_SystemMode);


            // msgin.serial(message);

            // SYSTEM_SYNC_CODE=1 or others
            // receiveSystemSync

            // SYSTEM_STALLED_CODE
            // receiveSystemStalled

            // SYSTEM_PROBE_CODE
            // _Changes.push_back(CChange(0, ProbeReceived));
            // receiveSystemProbe

            // SYSTEM_SERVER_DOWN_CODE

            foreach (var b in received)
            {
                Console.Write(Convert.ToString(b, 2).PadLeft(8, '0') + " ");
            }

            //Console.WriteLine(Encoding.UTF8.GetString(received));
        }

        public bool connected()
        {
            return udpMain.Client.Connected;
        }

        public void send(byte[] buffer, in int length)
        {
            udpMain.Send(buffer, length);
        }

        public bool dataAvailable()
        {
            return udpMain.Client.Available > 0;
        }

        public bool receive(ref Byte[] _ReceiveBuffer, int len, bool throw_exception)
        {
            var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                _ReceiveBuffer = udpMain.Receive(ref remoteIpEndPoint);
            }
            catch
            {
                if (throw_exception)
                    throw;

                return false;
            }

            return true;
        }

        public void disconnect()
        {
            udpMain.Close();
        }
    }
}
