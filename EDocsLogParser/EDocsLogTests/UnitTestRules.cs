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
using EDocsLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EDocsLogTests {
    [TestClass]
    public class UnitTestRules {
        [TestMethod]
        public void TestSqlHeaderRule1() {
            string test = "DOCSSQL: [0F050C00] SQLObject at 1085E930 acquired existing connection from pool #2";

            var rule = new SqlHeaderRule();
            var res = rule.Apply(test);
            Assert.IsNotNull(res);
            Assert.AreEqual("0F050C00", res.Key);
            Assert.AreEqual("1085E930", res.Values[ValueKeys.Sec]);
            Assert.AreEqual("2", res.Values[ValueKeys.Index]);
            Assert.AreEqual(LineType.Header, res.LineType);
            Assert.AreEqual(EventType.Sql, res.EventType);
            Assert.IsNull(res.RequiredBlockRule);
        }

        [TestMethod]
        public void TestSqlHeaderRule2() {
            string test = "DOCSSQL: [13C59BC0] SQLObject at 11378CE0 acquired new connection for existing Pool #6";

            var rule = new SqlHeaderRule();
            var res = rule.Apply(test);
            Assert.IsNotNull(res);
            Assert.AreEqual("13C59BC0", res.Key);
            Assert.AreEqual("11378CE0", res.Values[ValueKeys.Sec]);
            Assert.AreEqual("6", res.Values[ValueKeys.Index]);
            Assert.AreEqual(LineType.Header, res.LineType);
            Assert.AreEqual(EventType.Sql, res.EventType);
            Assert.IsNull(res.RequiredBlockRule);
        }

        [TestMethod]
        public void TestSqlFooterRule() {
            string test = "DOCSSQL: [13C595F0] SQLObject at 10FC54F0 released connection back to pool";

            var rule = new SqlFooterRule();
            var res = rule.Apply(test);
            Assert.IsNotNull(res);
            Assert.AreEqual("13C595F0", res.Key);
            Assert.AreEqual("10FC54F0", res.Values[ValueKeys.Sec]);
            Assert.AreEqual(LineType.Footer, res.LineType);
            Assert.AreEqual(EventType.Sql, res.EventType);
            Assert.IsNull(res.RequiredBlockRule);
        }

        [TestMethod]
        public void TestSqlBodyRule() {
            string test = "********** 09:33:17.533  [13C595F0] DOCSSQL: EXECute SQL Statement on Library:MYLIB - MYDB  (Oracle7) **********";

            var rule = new SqlBodyRule();
            var res = rule.Apply(test);
            Assert.IsNotNull(res);
            Assert.AreEqual("13C595F0", res.Key);
            Assert.AreEqual("09:33:17.533", res.Values[ValueKeys.Time]);
            Assert.AreEqual(LineType.Body, res.LineType);
            Assert.AreEqual(EventType.Sql, res.EventType);
            Assert.IsNotNull(res.RequiredBlockRule);
        }

        [TestMethod]
        public void TestSqlBodyRule_EmptyLine() {
            string test = "  \t   \t    ";

            var rule = new SqlBodyRule();
            var res = rule.Apply(test);
            Assert.IsNotNull(res);
            Assert.AreEqual(EventType.Unknown, res.EventType);
            Assert.AreEqual(LineType.Empty, res.LineType);
        }

        [TestMethod]
        public void TestHowEnumeratorWorks() {
            var list = new string[] { "one", "two" };

            var en = list.GetEnumerator();
            var bo = en.MoveNext();
            Assert.IsTrue(bo, "why not at first?");
            Assert.AreEqual(list[0], en.Current);

            bo = en.MoveNext();
            Assert.IsTrue(bo, "why not at second?");
            Assert.AreEqual(list[1], en.Current);

            bo = en.MoveNext();
            Assert.IsFalse(bo, "why not behind the last?");
            en.Reset();
            bo = en.MoveNext();
            Assert.IsTrue(bo, "why not at first again?");
            Assert.AreEqual(list[0], en.Current, "one is expected after Reset");
        }

        [TestMethod]
        public void TestSqlInnerBodyRule() {
            string file = TestHelper.TestLogPath + "inner_sql.log";
            var log = new LogFile { FileName = file };

            var rule = new SqlStatementRequiredRule();
            var res = rule.Apply(log.Lines);
            Assert.IsNotNull(res);
            Assert.AreEqual(7, res.ProcessedCount, "processed count");  // too many empty lines in DM logs :(
            Assert.AreEqual(3, res.LineRuleResults.Count, "extracted len");
            Assert.IsNotNull(res.LineRuleResults[0], "AutoCommit");
            Assert.IsNotNull(res.LineRuleResults[4], "STATEMENT:");
            Assert.IsNotNull(res.LineRuleResults[6], "SELECT ");
        }
    }
}
