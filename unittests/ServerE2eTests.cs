using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mycached;
using mycached.Protocol;
using System.Threading.Tasks;

namespace unittests
{
    [TestClass]
    public class ServerE2eTests
    {
        private MyCached mycached;

        [TestInitialize]
        public void Initialize()
        {
            this.mycached = new MyCached();

            this.mycached.Run(false);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.mycached.Stop();
        }

        [TestMethod]
        public async Task SimpleSetAndGet()
        {
            MyCacheClient client = new MyCacheClient();

            await client.Connect();

            ResponseStatus status = await client.Set("Hello", "World");

            Assert.AreEqual(ResponseStatus.NoError, status);

            String value = await client.Get("Hello");

            Assert.AreEqual("World", value);
        }

        [TestMethod]
        public async Task MultiSetAndGet()
        {
            MyCacheClient client = new MyCacheClient();

            await client.Connect();

            ResponseStatus status = await client.Set("1", "One");
            Assert.AreEqual(ResponseStatus.NoError, status);

            status = await client.Set("2", "Two");
            Assert.AreEqual(ResponseStatus.NoError, status);

            status = await client.Set("3", "Three");
            Assert.AreEqual(ResponseStatus.NoError, status);

            String value = await client.Get("1");
            Assert.AreEqual("One", value);

            value = await client.Get("2");
            Assert.AreEqual("Two", value);

            value = await client.Get("3");
            Assert.AreEqual("Three", value);
        }
    }
}
