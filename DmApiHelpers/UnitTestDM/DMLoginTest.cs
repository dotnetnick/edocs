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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DMApiHelpers;
using System.Collections.Generic;

namespace UnitTestDM {
    [TestClass]
    public class DMLoginTest {
        [TestMethod]
        public void TestDst() {
            DMLogin.Password = TestHelperSecure.MyPassword;
            string dst = DMLogin.Dst;
            Assert.IsFalse(string.IsNullOrWhiteSpace(dst));
        }

        [TestMethod]
        public void TestUserName() {
            string expected = @"mylogin";
            string user = DMLogin.UserName;
            Assert.AreEqual(expected, user);
        }
    }
}
