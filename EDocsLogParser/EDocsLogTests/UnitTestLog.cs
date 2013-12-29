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
