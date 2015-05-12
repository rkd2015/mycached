using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mycached.Protocol;
using System.IO;

namespace unittests
{
    [TestClass]
    public class ProtocolTests
    {
        [TestMethod]
        public void CheckMagicNumber()
        {
            GetRequest getRequest = new GetRequest();
            Assert.AreEqual(getRequest.Header.Magic, 0x80);

            GetResponse getResponse = new GetResponse(ResponseStatus.NoError);
            Assert.AreEqual(getResponse.Header.Magic, 0x81);

            SetRequest setRequest = new SetRequest();
            Assert.AreEqual(setRequest.Header.Magic, 0x80);

            SetResponse setResponse = new SetResponse(ResponseStatus.NoError);
            Assert.AreEqual(setResponse.Header.Magic, 0x81);
        }

        /// <summary>
        ///  |0 1 2 3 4 5 6 7|0 1 2 3 4 5 6 7|0 1 2 3 4 5 6 7|0 1 2 3 4 5 6 7|
        ///  +---------------+---------------+---------------+---------------+
        /// 0| 0x80          | 0x00          | 0x00          | 0x05          |
        ///  +---------------+---------------+---------------+---------------+
        /// 4| 0x00          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        /// 8| 0x00          | 0x00          | 0x00          | 0x05          |
        ///  +---------------+---------------+---------------+---------------+
        ///12| 0x00          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        ///16| 0x00          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        ///20| 0x00          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        ///24| 0x48 ('H')    | 0x65 ('e')    | 0x6c ('l')    | 0x6c ('l')    |
        ///  +---------------+---------------+---------------+---------------+
        ///28| 0x6f ('o')    |
        ///  +---------------+        
        /// </summary>
        [TestMethod]
        public void GetRequestSerialization()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            Byte[] packet = {   
                                0x80, 0x00, 0x00, 0x05, 
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x05,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0x48, 0x65, 0x6c, 0x6c,
                                0x6f
                            };

            writer.Write(packet);

            stream.Seek(0, SeekOrigin.Begin);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                GetRequest request = (GetRequest)ProtocolPacket.ReadRequest(reader);

                Assert.AreEqual(request.Header.Magic, 0x80);
                Assert.AreEqual(request.Header.OpCode, CommandOpCode.Get);
                Assert.AreEqual(request.Header.KeyLength, 5);
                Assert.AreEqual(request.Header.ExtrasLength, 0);
                Assert.AreEqual(request.Header.DataType, 0);
                Assert.AreEqual(request.Header.Status, ResponseStatus.NoError);
                Assert.AreEqual(request.Header.TotalBodyLength, (uint)5);
                Assert.AreEqual(request.Header.Opaque, (uint)0);
                Assert.AreEqual(request.Header.CAS, (ulong)0);
                Assert.IsNull(request.Header.Extras);
                Assert.AreEqual(request.Key, "Hello");
                Assert.IsTrue(String.IsNullOrEmpty(request.Value));

                MemoryStream stream2 = new MemoryStream();
                BinaryWriter writer2 = new BinaryWriter(stream2);

                request.Write(writer2);

                writer.Close();

                byte[] packet2 = stream2.ToArray();

                CollectionAssert.AreEqual(packet, packet2);
            }
        }

        /// <summary>
        ///  |0 1 2 3 4 5 6 7|0 1 2 3 4 5 6 7|0 1 2 3 4 5 6 7|0 1 2 3 4 5 6 7|
        ///  +---------------+---------------+---------------+---------------+
        /// 0| 0x81          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        /// 4| 0x04          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        /// 8| 0x00          | 0x00          | 0x00          | 0x09          |
        ///  +---------------+---------------+---------------+---------------+
        ///12| 0x00          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        ///16| 0x00          | 0x00          | 0x00          | 0x00          |
        ///  +---------------+---------------+---------------+---------------+
        ///20| 0x00          | 0x00          | 0x00          | 0x01          |
        ///  +---------------+---------------+---------------+---------------+
        ///24| 0xde          | 0xad          | 0xbe          | 0xef          |
        ///  +---------------+---------------+---------------+---------------+
        ///28| 0x57 ('W')    | 0x6f ('o')    | 0x72 ('r')    | 0x6c ('l')    |
        ///  +---------------+---------------+---------------+---------------+
        ///32| 0x64 ('d')    |
        /// +---------------+
        /// </summary>
        [TestMethod]
        public void GetResponseSerialization()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            Byte[] packet = {   
                                0x81, 0x00, 0x00, 0x00, 
                                0x04, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x09,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x01,
                                0xde, 0xad, 0xbe, 0xef,
                                0x57, 0x6f, 0x72, 0x6c,
                                0x64
                            };

            writer.Write(packet);

            stream.Seek(0, SeekOrigin.Begin);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                GetResponse response = (GetResponse)ProtocolPacket.ReadRequest(reader);
                Assert.AreEqual(response.Header.Magic, 0x81);
                Assert.AreEqual(response.Header.OpCode, CommandOpCode.Get);
                Assert.AreEqual(response.Header.KeyLength, 0);
                Assert.AreEqual(response.Header.ExtrasLength, 4);
                Assert.AreEqual(response.Header.DataType, 0);
                Assert.AreEqual(response.Header.Status, ResponseStatus.NoError);
                Assert.AreEqual(response.Header.TotalBodyLength, (uint)9);
                Assert.AreEqual(response.Header.Opaque, (uint)0);
                Assert.AreEqual(response.Header.CAS, (ulong)1);
                Assert.AreEqual(response.Header.Extras.Flags, (uint)0xdeadbeef);
                Assert.AreEqual(response.Value, "World");
                Assert.IsTrue(String.IsNullOrEmpty(response.Key));

                MemoryStream stream2 = new MemoryStream();
                BinaryWriter writer2 = new BinaryWriter(stream2);

                response.Write(writer2);

                writer.Close();

                byte[] packet2 = stream2.ToArray();

                CollectionAssert.AreEqual(packet, packet2);

            }
        }

        /// <summary>
        ///    +---------------+---------------+---------------+---------------+
        ///   0| 0x80          | 0x01          | 0x00          | 0x05          |
        ///    +---------------+---------------+---------------+---------------+
        ///   4| 0x08          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///   8| 0x00          | 0x00          | 0x00          | 0x12          |
        ///    +---------------+---------------+---------------+---------------+
        ///  12| 0x00          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///  16| 0x00          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///  20| 0x00          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///  24| 0xde          | 0xad          | 0xbe          | 0xef          |
        ///    +---------------+---------------+---------------+---------------+
        ///  28| 0x00          | 0x00          | 0x0e          | 0x10          |
        ///    +---------------+---------------+---------------+---------------+
        ///  32| 0x48 ('H')    | 0x65 ('e')    | 0x6c ('l')    | 0x6c ('l')    |
        ///    +---------------+---------------+---------------+---------------+
        ///  36| 0x6f ('o')    | 0x57 ('W')    | 0x6f ('o')    | 0x72 ('r')    |
        ///    +---------------+---------------+---------------+---------------+
        ///  40| 0x6c ('l')    | 0x64 ('d')    |
        ///    +---------------+---------------+            
        /// </summary>
        [TestMethod]
        public void SetRequestSerialization()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            Byte[] packet = {   
                                0x80, 0x01, 0x00, 0x05, 
                                0x08, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x12,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0xde, 0xad, 0xbe, 0xef,
                                0x00, 0x00, 0x0e, 0x10,
                                0x48, 0x65, 0x6c, 0x6c,
                                0x6f, 0x57, 0x6f, 0x72,
                                0x6c, 0x64
                            };

            writer.Write(packet);

            stream.Seek(0, SeekOrigin.Begin);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                SetRequest request = (SetRequest)ProtocolPacket.ReadRequest(reader);
                Assert.AreEqual(request.Header.Magic, 0x80);
                Assert.AreEqual(request.Header.OpCode, CommandOpCode.Set);
                Assert.AreEqual(request.Header.KeyLength, 5);
                Assert.AreEqual(request.Header.ExtrasLength, 8);
                Assert.AreEqual(request.Header.DataType, 0);
                Assert.AreEqual(request.Header.Status, ResponseStatus.NoError);
                Assert.AreEqual(request.Header.TotalBodyLength, (uint)0x12);
                Assert.AreEqual(request.Header.Opaque, (uint)0);
                Assert.AreEqual(request.Header.CAS, (ulong)0);
                Assert.AreEqual(request.Header.Extras.Flags, (uint)0xdeadbeef);
                Assert.AreEqual(request.Key, "Hello");
                Assert.AreEqual(request.Value, "World");

                MemoryStream stream2 = new MemoryStream();
                BinaryWriter writer2 = new BinaryWriter(stream2);

                request.Write(writer2);

                writer.Close();

                byte[] packet2 = stream2.ToArray();

                CollectionAssert.AreEqual(packet, packet2);

            }
        }

        /// <summary>
        ///    +---------------+---------------+---------------+---------------+
        ///   0| 0x81          | 0x02          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///   4| 0x00          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///   8| 0x00          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///  12| 0x00          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///  16| 0x00          | 0x00          | 0x00          | 0x00          |
        ///    +---------------+---------------+---------------+---------------+
        ///  20| 0x00          | 0x00          | 0x00          | 0x01          |
        ///    +---------------+---------------+---------------+---------------+
        /// </summary>
        [TestMethod]
        public void SetResponseSerialization()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            Byte[] packet = {   
                                0x81, 0x01, 0x00, 0x00, 
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x00,
                                0x00, 0x00, 0x00, 0x01
                            };

            writer.Write(packet);

            stream.Seek(0, SeekOrigin.Begin);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                SetResponse request = (SetResponse)ProtocolPacket.ReadRequest(reader);
                Assert.AreEqual(request.Header.Magic, 0x81);
                Assert.AreEqual(request.Header.OpCode, CommandOpCode.Set);
                Assert.AreEqual(request.Header.KeyLength, 0);
                Assert.AreEqual(request.Header.ExtrasLength, 0);
                Assert.AreEqual(request.Header.DataType, 0);
                Assert.AreEqual(request.Header.Status, ResponseStatus.NoError);
                Assert.AreEqual(request.Header.TotalBodyLength, (uint)0);
                Assert.AreEqual(request.Header.Opaque, (uint)0);
                Assert.AreEqual(request.Header.CAS, (ulong)1);
                Assert.IsNull(request.Header.Extras);
                Assert.IsTrue(String.IsNullOrEmpty(request.Key));
                Assert.IsTrue(String.IsNullOrEmpty(request.Value));

                MemoryStream stream2 = new MemoryStream();
                BinaryWriter writer2 = new BinaryWriter(stream2);

                request.Write(writer2);

                writer.Close();

                byte[] packet2 = stream2.ToArray();

                CollectionAssert.AreEqual(packet, packet2);
            }

        }
    }
}
