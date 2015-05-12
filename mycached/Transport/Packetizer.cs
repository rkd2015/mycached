using mycached.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mycached.Utilities;

namespace mycached.Transport
{
    public class Packetizer
    {
        public Queue<ProtocolPacket> Packets;
        private byte[] workingBuffer;
        private int pendingByteCount;
        private ProtocolHeader pendingHeader;

        public Packetizer()
        {
            this.Packets = new Queue<ProtocolPacket>();
            this.workingBuffer = new byte[8192];
            this.pendingByteCount = 0;
        }

        /// <summary>
        /// This method will perform the packet framing. Each time we receive data on the
        /// socket, we peel off packets fro mit and remember the remaining data. This handles
        /// partial packets including partial headers.
        /// </summary>
        /// <param name="data">data received on the socket</param>
        /// <param name="start">start</param>
        /// <param name="length">number of bytes</param>
        public void Push(byte[] data, int start, int length)
        {
            // If we have pending data, append the new data to it.
            if (this.pendingByteCount != 0)
            {
                data = Utilities.Utilities.Combine(this.workingBuffer, data);
                this.workingBuffer = null;
                this.pendingByteCount = 0;
            }

            using (MemoryStream memoryStream = new MemoryStream(data, start, length))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    int remainingBytes = (int)(memoryStream.Length - memoryStream.Position);

                    while (remainingBytes > ProtocolHeader.HeaderLength)
                    {
                        ProtocolHeader header = null;

                        // If we already have a header, use it. If not read a new one.
                        if (this.pendingHeader != null)
                        {
                            header = this.pendingHeader;
                            this.pendingHeader = null;
                        }
                        else
                        {
                            header = new ProtocolHeader();
                            header.Read(reader);
                        }

                        remainingBytes = (int)(memoryStream.Length - memoryStream.Position);
                            
                        if (header.TotalBodyLength - header.ExtrasLength <= remainingBytes)
                        {
                            // At this point, we have a full packet. Queue it for consumption
                            ProtocolPacket packet = ProtocolPacket.ReadRequest(reader, header);

                            this.Packets.Enqueue(packet);
                        }
                        else
                        {
                            // We have a header and a partial body. Save it for later.
                            this.pendingHeader = header;
                            this.workingBuffer = reader.ReadBytes(remainingBytes);
                            this.pendingByteCount = remainingBytes;
                        }

                        remainingBytes = (int)(memoryStream.Length - memoryStream.Position);
                    }

                    // We might have a partial header left out. Store the bytes till we get the next packet
                    remainingBytes = (int)(memoryStream.Length - memoryStream.Position);

                    if(remainingBytes != 0)
                    {
                        this.workingBuffer = reader.ReadBytes(remainingBytes);
                        this.pendingByteCount = remainingBytes;
                        this.pendingHeader = null;
                    }
                }
            }
        }

        public ProtocolHeader ReadHeader(byte[] data, int start, int length)
        {
            using (MemoryStream memoryStream = new MemoryStream(data, start, length))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {

                    ProtocolHeader header = new ProtocolHeader();

                    header.Read(reader);

                    return header;
                }
            }
        }
    }
}
