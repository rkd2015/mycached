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
    }
}
