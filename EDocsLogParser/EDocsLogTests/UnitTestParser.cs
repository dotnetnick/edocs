#region Copyright (c) 2013 Nick Khorin
/*
{*******************************************************************}
{                                                                   }
{       Tools and examples for OpenText eDOCS DM                    }
{       by Nick Khorin                                              }
{                                                                   }
{       Copyright (c) 2013 Nick Khorin                              }
{       http://softinclinations.blogspot.com                        }
{       ALL RIGHTS RESERVED                                         }
{                                                                   }
{   Usage or redistribution of all or any portion of the code       }
{   contained in this file is strictly prohibited unless this       }
{   Copiright note is maintained intact and also redistributed      }
{   with the original and modified code.                            }
{                                                                   }
{*******************************************************************}
*/
#endregion Copyright (c) 2013 Nick Khorin
using System;
using System.Globalization;
using EDocsLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace EDocsLogTests {
    [TestClass]
    public class UnitTestParser {
        [TestMethod]
        public void DateTimeParseExact() {
            string time = "15:20:12.123";
            var dt = SqlEventFactory.EDocsTimeToDateTime(time);
            //Assert.AreEqual(1, dt.Year);
            Assert.AreEqual(15, dt.Hour);
            Assert.AreEqual(20, dt.Minute);
            Assert.AreEqual(12, dt.Second);
            Assert.AreEqual(123, dt.Millisecond);
        }
        [TestMethod]
        public void TestSampleLogsExist() {
            string file = TestHelper.TestLogPath + "single.log";
            Assert.IsTrue(File.Exists(file));
        }

        [TestMethod]
        public void TestParseSingle() {
            string file = TestHelper.TestLogPath + "single.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
            Assert.AreEqual("0.000", parser.RawEvents[0].Body[4].Values[ValueKeys.Duration]);
            Assert.AreEqual("3.765", parser.RawEvents[0].Body[6].Values[ValueKeys.Duration]);
            Assert.AreEqual(1, parser.Events.Count);
            Assert.IsInstanceOfType(parser.Events[0], typeof(SqlEvent));
            var ev = (SqlEvent)parser.Events[0];
            Assert.AreEqual(1, ev.Queries.Count);
            var expectedCommand = "SELECT A0.MAIL_ID,A0.ATTACH_NUM FROM DOCSADM.VERSIONS A0 WHERE ((A0.MAIL_ID = '350AD92BCBF6984BA5DF2B5EB61688A0') AND (A0.ATTACH_NUM = '-1'))";
            var expectedRead = "0.000";
            var expectedIssue = "3.765";
            Assert.AreEqual(expectedCommand, ev.Queries[0].Command);
            Assert.AreEqual(expectedRead, ev.Queries[0].DurationReadItem);
            Assert.AreEqual(expectedIssue, ev.Queries[0].DurationIssueCommand);
        }

        [TestMethod]
        public void TestParseTwo() {
            string file = TestHelper.TestLogPath + "two.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(2, parser.RawEvents.Count);
            Assert.AreEqual(0, parser.RawEvents[0].Body.Count); // the first SQL is empty
            Assert.IsTrue(parser.RawEvents[1].Body[3].Content.StartsWith("SELECT "));
            Assert.AreEqual("0.015", parser.RawEvents[1].Body[6].Values[ValueKeys.Duration]);
        }

        [TestMethod]
        public void TestParseBatch() {
            string file = TestHelper.TestLogPath + "batch.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
        }

        [TestMethod]
        public void TestParseCanceled() {
            string file = TestHelper.TestLogPath + "canceled.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(5, parser.RawEvents.Count);
        }

        [TestMethod]
        public void TestParseDoubleNested() {
            string file = TestHelper.TestLogPath + "double_nested.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(2, parser.RawEvents.Count);
        }

        [TestMethod]
        public void TestParseAllStatements() {
            string file = TestHelper.TestLogPath + "all_statements.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
        }

        [TestMethod]
        public void TestParseSample() {
            string file = TestHelper.TestLogPath + "sample.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
            Assert.AreEqual("0.000", parser.RawEvents[0].Body[4].Values[ValueKeys.Duration]);
            Assert.AreEqual("4.281", parser.RawEvents[0].Body[6].Values[ValueKeys.Duration]);
            Assert.AreEqual(1, parser.Events.Count);
            Assert.IsInstanceOfType(parser.Events[0], typeof(SqlEvent));
            var ev = (SqlEvent)parser.Events[0];
            Assert.AreEqual(1, ev.Queries.Count);
            var expectedCommand = "SELECT A0.SYSTEM_ID,A0.DESCRIPTION,A0.CRITERIA,A2.FORM_NAME,A0.ICONIZED,A1.PERSONORGROUP,A1.ACCESSRIGHTS FROM DOCSADM.QUICKSEARCH A0 LEFT OUTER JOIN DOCSADM.FORMS A2 ON A0.FORM_ID = A2.SYSTEM_ID INNER JOIN DOCSADM.SECURITY A1 ON A0.SYSTEM_ID = A1.THING WHERE (A1.PERSONORGROUP IN(11321201,0,21236500,30180451,37094134,37097957,52943660,175948811)) ORDER BY A0.DESCRIPTION ASC, A0.SYSTEM_ID ASC";
            var expectedRead = "0.000";
            var expectedIssue = "4.281";
            Assert.AreEqual(expectedCommand, ev.Queries[0].Command);
            Assert.AreEqual(expectedRead, ev.Queries[0].DurationReadItem);
            Assert.AreEqual(expectedIssue, ev.Queries[0].DurationIssueCommand);
        }

        [TestMethod]
        public void TestParseP5Batch() {
            string file = TestHelper.TestLogPath + "dm531p5batch.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
            Assert.AreEqual(1, parser.Events.Count);
            var ev = (SqlEvent)parser.Events[0];
            Assert.AreEqual(10, ev.Queries.Count);
            var cmd = "SELECT DOCSADM.PROFILE.LAST_EDIT_TIME, DOCSADM.PROFILE.LAST_EDITED_BY, DOCSADM.PROFILE.LAST_EDIT_DATE, DOCSADM.PROFILE.FULLTEXT, DOCSADM.PROFILE.FORM, DOCSADM.PROFILE.PREV_SERVER_LOC, DOCSADM.PROFILE.PREV_SERVER_OS, DOCSADM.PROFILE.DOCUMENTTYPE, DOCSADM.PROFILE.PATH, DOCSADM.PROFILE.DOCSERVER_LOC, DOCSADM.PROFILE.DOCSERVER_OS, DOCSADM.PROFILE.RETENTION, DOCSADM.PROFILE.BILLABLE, DOCSADM.PROFILE.EDITING_TIME, DOCSADM.PROFILE.PROCESS_DATE, DOCSADM.PROFILE.LAST_ACCESS_TIME, DOCSADM.PROFILE.LAST_ACCESS_DATE, DOCSADM.PROFILE.LAST_ACCESS_ID, DOCSADM.PROFILE.LAST_LOCKED_BY, DOCSADM.PROFILE.PD_OBJ_TYPE, DOCSADM.PROFILE.TYPIST, DOCSADM.PROFILE.AUTHOR, DOCSADM.PROFILE.APPLICATION, DOCSADM.PROFILE.STORAGETYPE, DOCSADM.PROFILE.DEFAULT_RIGHTS, DOCSADM.PROFILE.READONLY_DATE, DOCSADM.PROFILE.STATUS, DOCSADM.PROFILE.DOCNAME, DOCSADM.PROFILE.DOCNUMBER, DOCSADM.PROFILE.SYSTEM_ID FROM DOCSADM.PROFILE WHERE DOCSADM.PROFILE.DOCNUMBER=12074189";
            Assert.AreEqual(cmd, ev.Queries[0].Command);
            cmd = "SELECT DOCSADM.VERSIONS.ATTACH_NUM, DOCSADM.VERSIONS.THREAD_NUM, DOCSADM.VERSIONS.PARENTMAIL_ID, DOCSADM.VERSIONS.MAIL_ID, DOCSADM.VERSIONS.CONTAINER_TYPE, DOCSADM.VERSIONS.CACHE_STATUS, DOCSADM.VERSIONS.PREV_STATUS, DOCSADM.VERSIONS.NEXT_PUBLISH_VER, DOCSADM.VERSIONS.PUBLISH_DATE, DOCSADM.VERSIONS.READONLY_DATE, DOCSADM.VERSIONS.VERSION_LABEL, DOCSADM.VERSIONS.VERSION, DOCSADM.VERSIONS.TYPIST, DOCSADM.VERSIONS.SUBVERSION, DOCSADM.VERSIONS.STATUS, DOCSADM.VERSIONS.LASTEDITTIME, DOCSADM.VERSIONS.LASTEDITDATE, DOCSADM.VERSIONS.FORCE_VERSION_RO, DOCSADM.VERSIONS.DOCNUMBER, DOCSADM.VERSIONS.COMMENTS, DOCSADM.VERSIONS.AUTHOR, DOCSADM.VERSIONS.ARCHIVE_ID, DOCSADM.VERSIONS.VERSION_ID FROM DOCSADM.VERSIONS WHERE DOCSADM.VERSIONS.VERSION_ID=12685567";
            Assert.AreEqual(cmd, ev.Queries[1].Command);
            cmd = "SELECT DOCSADM.SEQSYSTEMKEY.NEXTVAL FROM DUAL";
            Assert.AreEqual(cmd, ev.Queries[2].Command);
            Assert.AreEqual(null, ev.Queries[3].Command);
            cmd = "INSERT INTO DOCSADM.ACTIVITYLOG (CR_IN_USE,ACTIVITY_DESC,BILLED_ON,BILLABLE,PAGES,KEYSTROKES,TYPE_TIME,ELAPSED_TIME,TYPIST,AUTHOR,START_TIME,START_DATE,ACTIVITY_TYPE,REF_DOCUMENT,REF_LIBRARY,APPLICATION,VERSION_LABEL,DOCNUMBER,SYSTEM_ID) VALUES (N'','DOCSFUSION',TO_DATE(N'01/01/1753',N'MM/DD/YYYY'),N'',0,0,0,0,799253172,799253172,TO_DATE(N'01/01/1900 15:54:12',N'MM/DD/YYYY HH24:MI:SS'),TO_DATE(N'12/24/2013',N'MM/DD/YYYY'),15,0,-1,14172732,'2',12074189,799735852)";
            Assert.AreEqual(cmd, ev.Queries[4].Command);
            Assert.AreEqual(null, ev.Queries[5].Command);
            cmd = "UPDATE DOCSADM.PROFILE SET PROCESS_DATE=TO_DATE(N'12/24/2013',N'MM/DD/YYYY'),LAST_ACCESS_TIME=TO_DATE(N'01/01/1900 15:54:12',N'MM/DD/YYYY HH24:MI:SS'),LAST_ACCESS_DATE=TO_DATE(N'12/24/2013',N'MM/DD/YYYY'),LAST_ACCESS_ID=799253172 WHERE SYSTEM_ID=799489745";
            Assert.AreEqual(cmd, ev.Queries[6].Command);
            Assert.AreEqual(null, ev.Queries[7].Command);
            Assert.AreEqual(null, ev.Queries[8].Command);
            Assert.AreEqual(null, ev.Queries[9].Command);
        }

        [TestMethod]
        public void TestParseOutsideOfPool() {
            string file = TestHelper.TestLogPath + "OutsideOfPool.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
            Assert.AreEqual("0.000", parser.RawEvents[0].Body[4].Values[ValueKeys.Duration]);
            Assert.AreEqual("0.015", parser.RawEvents[0].Body[6].Values[ValueKeys.Duration]);
            Assert.AreEqual(1, parser.Events.Count);
            Assert.IsInstanceOfType(parser.Events[0], typeof(SqlEvent));
            var ev = (SqlEvent)parser.Events[0];
            Assert.AreEqual(1, ev.Queries.Count);
            var expectedCommand = @"SELECT DOCSADM.NETWORK_ALIASES.PERSONORGROUP, PERSONORGR00.USER_ID, DOCSADM.NETWORK_ALIASES.NETWORK_ID, DOCSADM.NETWORK_ALIASES.NETWORK_TYPE FROM DOCSADM.NETWORK_ALIASES,DOCSADM.PEOPLE PERSONORGR00 WHERE DOCSADM.NETWORK_ALIASES.PERSONORGROUP=PERSONORGR00.SYSTEM_ID(+) AND DOCSADM.NETWORK_ALIASES.NETWORK_ID='HELLO\KITTY' AND DOCSADM.NETWORK_ALIASES.NETWORK_TYPE=8";
            var expectedRead = "0.000";
            var expectedIssue = "0.015";
            Assert.AreEqual(expectedCommand, ev.Queries[0].Command);
            Assert.AreEqual(expectedRead, ev.Queries[0].DurationReadItem);
            Assert.AreEqual(expectedIssue, ev.Queries[0].DurationIssueCommand);
        }

        [TestMethod]
        public void TestParseWithSmartFooter() {
            string file = TestHelper.TestLogPath + "SmartFooter.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
            Assert.AreEqual(1, parser.Events.Count);
            Assert.IsInstanceOfType(parser.Events[0], typeof(SqlEvent));
            var ev = (SqlEvent)parser.Events[0];
            Assert.AreEqual(7, ev.Queries.Count);
        }

        [TestMethod]
        public void TestParseWithSmartFooter2() {
            string file = TestHelper.TestLogPath + "SmartFooter2.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
            Assert.AreEqual(1, parser.Events.Count);
            Assert.IsInstanceOfType(parser.Events[0], typeof(SqlEvent));
            var ev = (SqlEvent)parser.Events[0];
            Assert.AreEqual(8, ev.Queries.Count);
        }

        [TestMethod]
        public void TestParseWithHeaderInMiddle() {
            string file = TestHelper.TestLogPath + "HeaderInMiddle.log";
            var log = new LogFile { FileName = file };
            var parser = new LogParser { Log = log };
            parser.Parse();

            Assert.AreEqual(1, parser.RawEvents.Count);
            Assert.AreEqual(1, parser.Events.Count);
            Assert.IsInstanceOfType(parser.Events[0], typeof(SqlEvent));
            var ev = (SqlEvent)parser.Events[0];
            Assert.AreEqual(0, ev.Queries.Count);
        }
    }
}
