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
