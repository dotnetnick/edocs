using System;
using System.Linq;
using System.Collections.Generic;
using EDocsLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace EDocsLogTests {
    [TestClass]
    public class UnitTestEventProcessor {
        [TestMethod]
        public void TestScan() {
            var scanner = new EventScanner();
            var events = new List<BaseEvent>();
            scanner.Scan(events, EventFilters.IsTrue, EventProcessors.DoNothing); 
        }

        [TestMethod]
        public void TestScanFindsSlow() {
            // TODO mock the event, no need to run the parser
            string file = TestHelper.TestLogPath + "batch.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            var scanner = new EventScanner();
            var events = parser.Events;
            scanner.Scan(events, EventFilters.IsSqlSlow, ev => Assert.ReferenceEquals(ev, parser.Events[0]));
        }

        [TestMethod]
        public void TestScanSaveSlow() {
            string file = TestHelper.TestLogPath + "single.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            var scanner = new EventScanner();
            var events = parser.Events;
            scanner.Scan(events, EventFilters.IsSqlSlow, EventProcessors.DoNothing);
        }

        private static void SaveSqlBodyToFile(RawEvent ev) {
            if(ev.Type == EventType.Sql) {
                string fileName = GetEventUniqueName(ev);
                string fullFileName = fileName; // TODO
                using(var writer = File.CreateText(fullFileName)) {

                }
            }
            
        }
        private static string GetEventUniqueName(RawEvent ev) {
            if(ev.Type == EventType.Sql) {
                return ev.Key;
            }
            return null;
        }
    }
}
