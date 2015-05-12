using mycached.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mycached.Transport
{
    public class Packetizer
    {
        public Queue<ProtocolPacket> Packets;
        private byte[] workingBuffer;
        private int workingBufferUsed;

        public Packetizer()
        {
            this.Packets = new Queue<ProtocolPacket>();
            this.workingBuffer = new byte[8192];
        }

        public void Push(byte[] data, int start, int length)
        {
            using (MemoryStream memoryStream = new MemoryStream(data, start, length))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {

                    ProtocolHeader header = new ProtocolHeader();

                    header.Read(reader);

                    ProtocolPacket packet = ProtocolPacket.ReadRequest(reader, header);

                    this.Packets.Enqueue(packet);
                }
            }
        }
    }
}
