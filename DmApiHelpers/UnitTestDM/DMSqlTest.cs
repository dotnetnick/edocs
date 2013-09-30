using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DMApiHelpers;

namespace UnitTestDM {
    [TestClass]
    public class DMSqlTest {
        [TestMethod]
        public void TestExecute() {
            DMLogin.ServerName  = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            var cmd = "select application from apps where default_extension = 'DOC'";

            var sql = new DMSql { Dst = DMLogin.Dst, Library = DMLogin.Library };
            var result = sql.ExecuteSql(cmd);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ColumnCount);
            Assert.AreEqual(1, result.RowCount);
            Assert.AreEqual(0, result.RowsAffected);
            Assert.IsNotNull(result.ColumnNames);
            Assert.AreEqual(1, result.ColumnNames.Length);
            Assert.AreEqual("APPLICATION", result.ColumnNames[0]);
            Assert.IsNotNull(result.Rows);
            Assert.AreEqual(1, result.Rows.Length);
            Assert.AreEqual(1, result.Rows[0].Values.Length);
            Assert.AreEqual("MS WORD", result.Rows[0].Values[0]);
        }
    }
}
