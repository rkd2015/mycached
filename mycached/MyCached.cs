using mycached.Protocol;
using mycached.Storage;
using mycached.Transport;
using mycached.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mycached
{
    public class MyCached
    {
        private TcpListener listener;
        private Dictionary<IPEndPoint, TcpConnection> clientConnections;
        private MemCache cache;
        private ManualResetEvent exitEvent;

        public MyCached()
        {
            this.listener = new TcpListener(IPAddress.IPv6Any, Configuration.ListenPort);
            this.clientConnections = new Dictionary<IPEndPoint, TcpConnection>();
        }

        public void Run()
        {
            this.listener.Start();

            this.listener.BeginAcceptTcpClient(
                                            new AsyncCallback(OnAcceptTcpClientCallback),
                                            this);
            this.exitEvent = new ManualResetEvent(false);

            exitEvent.WaitOne();
        }

        public void Stop()
        {
            this.listener.Stop();

            foreach (KeyValuePair<IPEndPoint, TcpConnection> pair in this.clientConnections)
            {
                pair.Value.Close();
            }

            this.exitEvent.Set();
        }

        private void OnPacketsReceived(Queue<ProtocolPacket> packets)
        {
            foreach(ProtocolPacket packet in packets)
            {
                if (packet is GetRequest)
                {
                    GetRequest getRequest = packet as GetRequest;

                }
                else if (packet is SetRequest)
                {
                    SetRequest setRequest = packet as SetRequest;
                }
            }
        }

        static public void OnAcceptTcpClientCallback(IAsyncResult ar)
        {
            MyCached myCached = (MyCached)ar.AsyncState;
            TcpClient client = myCached.listener.EndAcceptTcpClient(ar);
            TcpConnection connection = new TcpConnection(client);

            connection.OnPacketsReceived += connection.OnPacketsReceived;
            myCached.clientConnections.Add((IPEndPoint)client.Client.RemoteEndPoint, connection);

            myCached.listener.BeginAcceptTcpClient(
                                            new AsyncCallback(OnAcceptTcpClientCallback),
                                            myCached);
        }

    }
}
