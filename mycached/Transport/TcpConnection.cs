using mycached.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Transport
{
    public class TcpConnection
    {
        public event EventHandler<Queue<ProtocolPacket>> OnPacketsReceived;

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
            this.stream.BeginRead(this.buffer, 
                                  0, 
                                  this.buffer.Length, 
                                  new AsyncCallback(OnReadCallback), 
                                  this);
        }

        public void Close()
        {
            this.client.Close();
        }

        public void SendResponse(byte[] response, int start, int length)
        {
            try
            {
                this.stream.BeginWrite(response, start, length, null, null);
            }
            catch(Exception e)
            {
                Trace.TraceError("SendResponse: Caught error {0}", e.ToString());
            }
        }

        static public void OnReadCallback(IAsyncResult ar)
        {
            TcpConnection connection = (TcpConnection)ar.AsyncState;

            try
            {
                int numberOfBytesRead = connection.stream.EndRead(ar);

                if (numberOfBytesRead != 0)
                {
                    connection.packetizer.Push(connection.buffer, 0, numberOfBytesRead);

                    connection.OnPacketsReceived(connection, connection.packetizer.Packets);

                    connection.stream.BeginRead(connection.buffer,
                                                0,
                                                connection.buffer.Length,
                                                new AsyncCallback(OnReadCallback),
                                                connection);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("SendResponse: Caught error {0}", e.ToString());
            }
        }
    }
}
