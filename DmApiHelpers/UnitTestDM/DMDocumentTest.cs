using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DMApiHelpers;
using System.Collections.Generic;
using System.IO;

namespace UnitTestDM {
    [TestClass]
    public class DMDocumentTest {
        [TestMethod]
        public void TestCreateAndDeleteEmptyCustomProfile() {
            DMLogin.ServerName = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            string newDocName = Guid.NewGuid().ToString();
            string dst = DMLogin.Dst;

            ProfileInfo profile = new ProfileInfo() { FormName = "CUSTOM_PROFILE_FORM" };
            profile.Properties = new Dictionary<string, string>() { 
                { "DOCNAME", newDocName },
                { "APP_ID", "MS WORD" },
                { "AUTHOR_ID", "JDOE" },
                { "TYPIST_ID", "JDOE" },
                { "TYPE_ID", "REPORT" },
                { "CUSTOM_PROP", "NEW VAL" }
            };

            DMDocument doc = new DMDocument() { Dst = dst, Library = DMLogin.Library };

            // create
            var docInfo = doc.CreateProfile(profile);
            Assert.IsNotNull(docInfo);
            Assert.IsTrue(docInfo.DocNumber > 0);
            Assert.IsTrue(docInfo.VersionID > 0);

            // unlock (check-in). We are okay with creating a profile without a document file
            try {
                doc.UnlockDocument(docInfo);
            }
            catch(DMApiEmptyDocumentFileException) {
            }

            // delete
            doc.DeleteProfile(docInfo.DocNumber);
        }
        
        [TestMethod]
        public void TestCreateAndDeleteFolder() {
            DMLogin.ServerName = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            string newDocName = Guid.NewGuid().ToString();
            string dst = DMLogin.Dst;

            ProfileInfo profile = new ProfileInfo() { FormName = "STANDARD_P" };
            profile.Properties = new Dictionary<string, string>() { 
                    { "DOCNAME", newDocName },
                    { "APP_ID", "FOLDER" },
                    { "AUTHOR_ID", "JDOE" },
                    { "TYPIST_ID", "JDOE" }
                };
            profile.Trustees = new List<TrusteeInfo>() {
                new TrusteeInfo("DOCS_USERS", TrusteeType.Group, AccessRights.ReadOnly)
            };

            DMDocument doc = new DMDocument() { Dst = dst, Library = DMLogin.Library };

            // create
            var docInfo = doc.CreateProfile(profile);
            Assert.IsNotNull(docInfo);
            Assert.IsTrue(docInfo.DocNumber > 0);
            Assert.IsTrue(docInfo.VersionID > 0);

            // no need to unlock a folder

            // delete
            doc.DeleteProfile(docInfo.DocNumber);
        }

        [TestMethod]
        public void TestCreateFolder_LinkToFolder_DeleteFolder() {
            DMLogin.ServerName = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            string newDocName = Guid.NewGuid().ToString();
            string dst = DMLogin.Dst;

            ProfileInfo profile = new ProfileInfo() { FormName = "STANDARD_P" };
            profile.Properties = new Dictionary<string, string>() { 
                    { "DOCNAME", newDocName },
                    { "APP_ID", "FOLDER" },
                    { "AUTHOR_ID", "JDOE" },
                    { "TYPIST_ID", "NKHORIN" }
                };

            DMDocument doc = new DMDocument() { Dst = dst, Library = DMLogin.Library };

            // create
            var docInfo = doc.CreateProfile(profile);
            Assert.IsNotNull(docInfo);
            Assert.IsTrue(docInfo.DocNumber > 0);
            Assert.IsTrue(docInfo.VersionID > 0);

            try {
                // no need to unlock a folder

                // link to an existing folder (we looked up its DOCNUMBER and VERSIONID)
                var parent = new DocumentInfo { DocNumber = 12070617, VersionID = 12681993 };
                doc.AddLink(docInfo.DocNumber, parent);
            }
            finally {
                // clear links and delete 
                doc.DeleteProfile(docInfo.DocNumber, true);
            }
        }

        [TestMethod]
        public void TestCreateAndDeleteDocument() {
            DMLogin.ServerName = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            string dst = DMLogin.Dst;
            string newDocName = Guid.NewGuid().ToString();

            ProfileInfo profile = new ProfileInfo() { FormName = "STANDARD_P" };
            profile.Properties = new Dictionary<string, string>() { 
            		{ "DOCNAME", newDocName },
                    { "APP_ID", "NOTEPAD" },
                    { "AUTHOR_ID", "JDOE" },
                    { "TYPIST_ID", "JDOE" }
                };
            DMDocument doc = new DMDocument() { Dst = dst, Library = DMLogin.Library };

            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, "hello world!\r\n");
            try {
                var docInfo = doc.CreateDocument(profile, fileName);
                Assert.IsNotNull(docInfo);
                Assert.IsTrue(docInfo.DocNumber > 0);
                Assert.IsTrue(docInfo.VersionID > 0);
                //TODO download content and check

                doc.DeleteProfile(docInfo.DocNumber);
            }
            finally {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void TestFindLinks() {
            DMLogin.ServerName = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            string dst = DMLogin.Dst;

            // we know an existing document-folder link in our library
            int docNumber = 12071903;
            string expectedLink = "799470242";
            DMDocument doc = new DMDocument() { Dst = dst, Library = DMLogin.Library };
            var res = doc.FindLinks(docNumber);
            Assert.IsNotNull(res);
            Assert.AreEqual(1, res.RowCount);
            Assert.AreEqual(expectedLink, res.Rows[0].Values[0]);
        }

        [TestMethod]
        public void TestFetchTrustees() {
            DMLogin.ServerName = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            string dst = DMLogin.Dst;

            // we know the trustees of this existing document
            int docNumber = 123456;
            TrusteeInfo[] expected = new TrusteeInfo[] {
                new TrusteeInfo("DOCS_USERS", TrusteeType.Group, AccessRights.ReadOnly),
                new TrusteeInfo("HR_USERS", TrusteeType.Group, AccessRights.NormalAccess),
                new TrusteeInfo("JDOE", TrusteeType.Person, AccessRights.View),
                new TrusteeInfo("NKHORIN", TrusteeType.Person, AccessRights.FullAccess),
            };

            DMDocument doc = new DMDocument() { Dst = dst, Library = DMLogin.Library };
            var actual = doc.FetchTrustees(docNumber);
            Assert.AreEqual(expected.Length, actual.Length);
            foreach(var exp in expected)
                Assert.IsTrue(actual.Contains(exp), "Not found " + exp.Trustee);
        }

        [TestMethod]
        public void TestUpdateTrustees_RemoveAndAddBack() {
            DMLogin.ServerName = DMTestEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;
            string dst = DMLogin.Dst;


            // we know the trustees of this existing document
            int docNumber = 123457;
            TrusteeInfo docsUsersInitial = new TrusteeInfo("DOCS_USERS", TrusteeType.Group, AccessRights.ReadOnly);
            TrusteeInfo docsUsersToSet = new TrusteeInfo("DOCS_USERS", TrusteeType.Group, AccessRights.NoAccess);
            List<TrusteeInfo> expected = new List<TrusteeInfo>() {
                docsUsersInitial,
                new TrusteeInfo("HR_USERS", TrusteeType.Group, AccessRights.NormalAccess),
                new TrusteeInfo("JDOE", TrusteeType.Person, AccessRights.View),
                new TrusteeInfo("NKHORIN", TrusteeType.Person, AccessRights.FullAccess),
            };

            DMDocument doc = new DMDocument() { Dst = dst, Library = DMLogin.Library };
            var actual = doc.FetchTrustees(docNumber);
            Assert.AreEqual(expected.Count, actual.Length);
            foreach(var exp in expected)
                Assert.IsTrue(actual.Contains(exp), "Not found " + exp.Trustee);

            // Remove DOCS_USERS. We use NoAccess to remove a truestee. 
            // This is the way we implemented DMDocument.UpdateTrustees
            doc.UpdateTrustees(docNumber, new TrusteeInfo[] { docsUsersToSet });

            // check
            expected.Remove(docsUsersInitial);
            actual = doc.FetchTrustees(docNumber);
            Assert.AreEqual(expected.Count, actual.Length);
            foreach(var exp in expected)
                Assert.IsTrue(actual.Contains(exp), "Not found " + exp.Trustee);
            
            // Add DOCS_USERS back
            doc.UpdateTrustees(docNumber, new TrusteeInfo[] { docsUsersInitial });

            // check
            expected.Add(docsUsersInitial);
            actual = doc.FetchTrustees(docNumber);
            Assert.AreEqual(expected.Count, actual.Length);
            foreach(var exp in expected)
                Assert.IsTrue(actual.Contains(exp), "Not found " + exp.Trustee);
        }
    }
}
