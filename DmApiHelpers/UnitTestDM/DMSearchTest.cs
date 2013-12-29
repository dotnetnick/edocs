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
    public class DMSearchTest {
        /*
        my test code to show a bug in v_components
        public int FindDocumentByComponentPath(string path) {
            var search = new PCDSearch();
            search.SetDST(DMLogin.Dst);
            search.SetSearchObject("v_components");
            search.AddSearchCriteria("PATH", path);
            search.AddReturnProperty("DOCNUMBER");
            search.AddReturnProperty("DOCUMENTPATH");

            if(search.Execute() != 0)
                throw new Exception(string.Format("Search failed: {0} {1}", search.ErrNumber, search.ErrDescription));
            try {
                int count = search.GetRowsFound();
                if(count == 0)
                    return -1;
                search.NextRow();
                object result = search.GetPropertyValue("DOCNUMBER");
                return Convert.ToInt32(result);
            }
            finally {
                search.ReleaseResults();
            }
        }


        [TestMethod]
        public void TestFindDocumentByComponentPath_DocsSupervisor() {
            DMLogin.Password = TestHelperSecure.MyPassword;

            var search = new DMSearch();
            int doc = search.FindDocumentByComponentPath("foo");
            Assert.AreEqual(-1, doc);
        }

        [TestMethod]
        public void TestFindDocumentByComponentPath_OpenUser() {
            DMLogin.UserName = TestHelperSecure.MyAdmLogin;
            DMLogin.Password = TestHelperSecure.MyAdmPassword;

            var search = new DMSearch();
            int doc = search.FindDocumentByComponentPath("foo");
            Assert.AreEqual(-1, doc);
        }
         */

        [TestMethod]
        public void TestSearchOneDoc() {
            DMLogin.ServerName = DMProdEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;

            // a known document in our library
            string expectedDocNum = "12387815";
            string expectedName = "TEST 2";

            var info = new SearchInfo {
                SearchObject = "STANDARD_S",
                Criteria = new Dictionary<string, string> {
                    { "DOCNUMBER", expectedDocNum }
                },
                ReturnProperties = new List<string> {
                    "DOCNUMBER", "DOCNAME"
                }
            };

            var search = new DMSearch() { Dst = DMLogin.Dst, Library = DMLogin.Library };
            var result = search.Search(info);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Rows);
            Assert.AreEqual(1, result.Rows.Count);
            Assert.AreEqual(expectedDocNum, result.Rows[0][info.ReturnProperties[0]]);
            Assert.AreEqual(expectedName, result.Rows[0][info.ReturnProperties[1]]);
        }

        [TestMethod]
        public void TestSearchTwoDocs() {
            DMLogin.ServerName = DMProdEnvironment.Server;
            DMLogin.Password = TestHelperSecure.MyPassword;

            // two known documents in our library
            string expectedDocNum = "12387815";
            string expectedDocNum2 = "12418906";
            string expectedName = "TEST 2";
            string expectedName2 = "DM TEST";

            var info = new SearchInfo {
                SearchObject = "STANDARD_S",
                Criteria = new Dictionary<string, string> {
                    { "DOCNUMBER", expectedDocNum + "," + expectedDocNum2 }
                },
                OrderBy = new Dictionary<string, SortOrder> {
                    { "DOCNUMBER", SortOrder.Ascending }
                },
                ReturnProperties = new List<string> {
                    "DOCNUMBER", "DOCNAME"
                }
            };

            var search = new DMSearch() { Dst = DMLogin.Dst };
            var result = search.Search(info);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Rows);
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual(expectedDocNum, result.Rows[0][info.ReturnProperties[0]]);
            Assert.AreEqual(expectedName, result.Rows[0][info.ReturnProperties[1]]);
            Assert.AreEqual(expectedDocNum2, result.Rows[1][info.ReturnProperties[0]]);
            Assert.AreEqual(expectedName2, result.Rows[1][info.ReturnProperties[1]]);
        }
    }
}
