using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RUDPSharp
{
    public class UDPThreadSocket : UDPSocket
    {
        protected int port;
        byte[] m_Buffer;

        public UDPThreadSocket(string name = "UDPThreadSocket") : base(name)
        {
            m_Buffer = new byte[BufferSize];
        }
        public override bool Listen(int port)
        {
            var ep = new IPEndPoint(IPAddress.Any, port);
            bool result = Bind(socketIP4, ep);
            Console.WriteLine($"{this} is Listening on {ep} {result}");
            var epV6 = new IPEndPoint(IPAddress.IPv6Any, port);
            result &= Bind(socketIP6, epV6);
            this.port = port;
            Thread socket = new Thread(RunListenThread);
            socket.Start();
            return result;
        }

        private void RunListenThread()
        {
            while (!RecievedPackets.IsAddingCompleted)
            {
                if (socketIP4.Available == 0)
                    continue;
                EndPoint temptRemoteEP = GetEndPoint();
                int received = socketIP4.ReceiveFrom(m_Buffer, ref temptRemoteEP);
                byte[] newBuffer = new byte[received];
                Buffer.BlockCopy(m_Buffer, 0, newBuffer, 0, received);
                RecievedPackets.Add((temptRemoteEP, newBuffer));
            }
        }

        public override Task<bool> SendTo(EndPoint endPoint, byte[] data, CancellationToken token)
        {
            socketIP4.SendTo(data, endPoint);
            return Task.Factory.StartNew(() => {  return true; });
        }
    }
}
