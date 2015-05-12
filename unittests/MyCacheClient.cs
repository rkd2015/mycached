using mycached.Protocol;
using mycached.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace mycached
{
    public class MyCacheClient
    {
        private TcpClient client;
        private NetworkStream stream;

        public MyCacheClient()
        {
            this.client = new TcpClient();
        }

        public async Task Connect()
        {
            await this.client.ConnectAsync(IPAddress.Loopback, Configuration.ListenPort);

            this.stream = this.client.GetStream();
        }

        public async Task<string> Get(String key, CommandOpCode opCode = CommandOpCode.Get)
        {
            GetRequest getRequest = new GetRequest(opCode);

            getRequest.Key = key;

            byte[] packet = getRequest.Serialize();

            await this.stream.WriteAsync(packet, 0, packet.Length);

            byte[] response = new byte[2048];
            int bytesRead = await this.stream.ReadAsync(response, 0, response.Length);

            GetResponse getResponse = (GetResponse)ProtocolPacket.Construct(response, 0, bytesRead);

            return getResponse.Value;
        }

        public async Task<ResponseStatus> Set(String key, String value, CommandOpCode opCode = CommandOpCode.Set)
        {
            SetRequest setRequest = new SetRequest(opCode);

            setRequest.Key = key;
            setRequest.Value = value;

            byte[] packet = setRequest.Serialize();

            await this.stream.WriteAsync(packet, 0, packet.Length);

            byte[] response = new byte[2048];
            int bytesRead = await this.stream.ReadAsync(response, 0, response.Length);

            SetResponse setResponse = (SetResponse)ProtocolPacket.Construct(response, 0, bytesRead);

            return setResponse.Header.Status;
        }
    }
}
