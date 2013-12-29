using System;
using EDocsLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EDocsLogTests {
    [TestClass]
    public class UnitTestLog {
        [TestMethod]
        public void TestLogFile() {
            string file = TestHelper.TestLogPath + "single.log";
            int expectedCount = 27; // 17;
            var log = new LogFile { FileName = file };
            string[] lines = log.Lines;
            Assert.AreEqual(expectedCount, lines.Length);
        }
    }
}
