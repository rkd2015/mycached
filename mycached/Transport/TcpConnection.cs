using mycached.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Transport
{
    public class TcpConnection
    {
        public delegate EventHandler<Queue<ProtocolPacket>> PacketsReceivedHandler(TcpConnection conneciton, Queue<ProtocolPacket> packets);
        
        public PacketsReceivedHandler OnPacketsReceived;

        private TcpClient client;
        private byte[] buffer;
        private NetworkStream stream;
        private Packetizer packetizer;

        public TcpConnection(TcpClient client)
        {
            this.client = client;
            this.buffer = new byte[2048];
            this.packetizer = new Packetizer();

            this.stream = this.client.GetStream();
            this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(OnReadCallback), this);
        }

        public void Close()
        {
            this.client.Close();
        }

        static public void OnReadCallback(IAsyncResult ar)
        {
            TcpConnection connection = (TcpConnection)ar.AsyncState;
            int numberOfBytesRead = connection.stream.EndRead(ar);

            connection.packetizer.Push(connection.buffer, 0, numberOfBytesRead);

            connection.OnPacketsReceived(connection, connection.packetizer.Packets);
        }
    }
}
