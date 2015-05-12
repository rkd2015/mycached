using mycached.Protocol;
using mycached.Storage;
using mycached.Transport;
using mycached.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mycached
{
    /// <summary>
    /// MyCache daemon class. Holds the listener and cache.
    /// </summary>
    public class MyCached
    {
        private TcpListener listener;
        private Dictionary<IPEndPoint, TcpConnection> clientConnections;
        private MemCache cache;
        private ManualResetEvent exitEvent;
        private Queue<byte[]> queuedResponses;

        public MyCached()
        {

            this.listener = new TcpListener(IPAddress.Any, Configuration.ListenPort);
            this.clientConnections = new Dictionary<IPEndPoint, TcpConnection>();
            this.cache = new MemCache();
            this.queuedResponses = new Queue<byte[]>();
        }

        /// <summary>
        /// Run an dblock if necessary.
        /// </summary>
        /// <param name="shouldBlock">block if true</param>
        public void Run(bool shouldBlock = true)
        {
            Trace.TraceInformation("Listening on port {0}", Configuration.ListenPort);
            this.listener.Start();

            this.listener.BeginAcceptTcpClient(
                                            new AsyncCallback(OnAcceptTcpClientCallback),
                                            this);
            this.exitEvent = new ManualResetEvent(false);

            if (shouldBlock)
            {
                exitEvent.WaitOne();
            }
        }

        /// <summary>
        /// Stop the daemon
        /// </summary>
        public void Stop()
        {
            this.listener.Stop();

            lock (this.clientConnections)
            {
                foreach (KeyValuePair<IPEndPoint, TcpConnection> pair in this.clientConnections)
                {
                    pair.Value.Close();
                }
            }

            this.exitEvent.Set();
        }

        /// <summary>
        /// Handle deserialized packets
        /// </summary>
        /// <param name="sender">the tcp connection taht received the packet</param>
        /// <param name="packets">packet queue(queue will preserve order)</param>
        public void OnPacketsReceived(object sender, Queue<ProtocolPacket> packets)
        {
            TcpConnection connection = (TcpConnection)sender;

            while(packets.Count != 0)
            {
                ProtocolPacket packet = packets.Dequeue();

                if (packet is GetRequest)
                {
                    this.HandleGet(packet as GetRequest, connection);
                }
                else if (packet is SetRequest)
                {
                    this.HandleSet(packet as SetRequest, connection);
                }
                else
                {
                    Trace.TraceInformation("Unknown request {0}", packet.GetType().FullName);
                }
            }
        }

        /// <summary>
        /// Handle a get request. Lookup cache and return response
        /// </summary>
        /// <param name="getRequest">GET request</param>
        /// <param name="connection">TcpConnection</param>
        private void HandleGet(GetRequest getRequest, TcpConnection connection)
        {
            String value = String.Empty;
            uint flags = 0;

            CacheStatus status = this.cache.Get(getRequest.Key, out value, out flags);
            ResponseStatus responseStatus = ResponseStatus.NoError;
            switch (status)
            {
                case CacheStatus.KeyDoesNotExist:
                    responseStatus = ResponseStatus.KeyNotFound;
                    break;
            }

            GetResponse getResponse = new GetResponse(responseStatus);

            if (getRequest.Header.OpCode == CommandOpCode.GetK ||
                getRequest.Header.OpCode == CommandOpCode.GetKQ)
            {
                getResponse.Key = getRequest.Key;
            }

            getResponse.Value = value;
            getResponse.Header.Extras = new CommandExtras(4);
            getResponse.Header.Extras.Flags = flags;
            getResponse.Header.Opaque = getRequest.Header.Opaque;

            byte[] responsePacket = getResponse.Serialize();

            if (getRequest.Header.OpCode == CommandOpCode.GetQ ||
                getRequest.Header.OpCode == CommandOpCode.GetKQ)
            {
                Trace.TraceInformation("Queuing Get request");
                this.queuedResponses.Enqueue(responsePacket);
            }
            else
            {
                Trace.TraceInformation("Flushing request queue");
                while (this.queuedResponses.Count != 0)
                {
                    byte[] queuedPacket = this.queuedResponses.Dequeue();
                    connection.SendResponse(queuedPacket, 0, queuedPacket.Length);
                }

                connection.SendResponse(responsePacket, 0, responsePacket.Length);
            }
        }

        /// <summary>
        /// Handle a set request. Modify the record for the given key
        /// </summary>
        /// <param name="setRequest">SET request</param>
        /// <param name="connection">Tcp Connection</param>
        private void HandleSet(SetRequest setRequest, TcpConnection connection)
        {
            UInt32 flags = (setRequest.Header.Extras != null) ? setRequest.Header.Extras.Flags : 0;
            UInt32 expiry = (setRequest.Header.Extras != null) ? setRequest.Header.Extras.Expiry : 0;
            UInt64 newCas = 0;

            CacheStatus status = this.cache.Set(setRequest.Key,
                                                setRequest.Value,
                                                flags,
                                                expiry,
                                                setRequest.Header.CAS,
                                                out newCas);

            ResponseStatus responseStatus = ResponseStatus.NoError;
            switch (status)
            {
                case CacheStatus.CasDosentMatch:
                    responseStatus = ResponseStatus.InvalidArguments;
                    break;
            }

            SetResponse setResponse = new SetResponse(responseStatus);
            setRequest.Header.CAS = newCas;
            setResponse.Header.Opaque = setRequest.Header.Opaque;

            byte[] responsePacket = setResponse.Serialize();

            if (setRequest.Header.OpCode == CommandOpCode.SetQ)
            {
                Trace.TraceInformation("Queuing set request");
                this.queuedResponses.Enqueue(responsePacket);
            }
            else
            {
                Trace.TraceInformation("Flushing request queue");
                while (this.queuedResponses.Count != 0)
                {
                    byte[] queuedPacket = this.queuedResponses.Dequeue();
                    connection.SendResponse(queuedPacket, 0, queuedPacket.Length);
                }

                connection.SendResponse(responsePacket, 0, responsePacket.Length);
            }
        }

        /// <summary>
        /// Accept a incoming TCP conneciton
        /// </summary>
        /// <param name="ar">async result</param>
        static public void OnAcceptTcpClientCallback(IAsyncResult ar)
        {
            MyCached myCached = (MyCached)ar.AsyncState;
            Trace.TraceInformation("OnAcceptTcpClientCallback");

            try
            {
                TcpClient client = myCached.listener.EndAcceptTcpClient(ar);
                Trace.TraceInformation("Accepting connection from : {0}", client.Client.RemoteEndPoint.ToString());

                TcpConnection connection = new TcpConnection(client);

                connection.OnPacketsReceived += myCached.OnPacketsReceived;

                lock (myCached.clientConnections)
                {
                    myCached.clientConnections.Add((IPEndPoint)client.Client.RemoteEndPoint, connection);
                }

                myCached.listener.BeginAcceptTcpClient(
                                                new AsyncCallback(OnAcceptTcpClientCallback),
                                                myCached);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("OnAcceptTcpClientCallback: Caught exception {0}", e.ToString());
            }
        }

    }
}
