#region Copyright (c) 2014 Nick Khorin
/*
{*******************************************************************}
{                                                                   }
{       Tools and examples for OpenText eDOCS DM                    }
{       by Nick Khorin                                              }
{                                                                   }
{       Copyright (c) 2013-2014 Nick Khorin                         }
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
#endregion Copyright (c) 2014 Nick Khorin
using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDocsLog;

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

        [TestMethod]
        public void TestGetEncodingUnicode() {
            string file = TestHelper.TestLogPath + "single.log";
            var log = new LogFile { FileName = file };
            var result = log.GetEncoding();
            Assert.AreEqual(Encoding.Unicode, result);
        }

        [TestMethod]
        public void TestGetEncodingAscii() {
            string file = TestHelper.TestLogPath + "DM521.log";
            var log = new LogFile { FileName = file };
            var result = log.GetEncoding();
            Assert.AreEqual(Encoding.ASCII, result);
        }
    }
}
