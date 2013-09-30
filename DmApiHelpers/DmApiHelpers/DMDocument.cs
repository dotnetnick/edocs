using Hummingbird.DM.Server.Interop.PCDClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DMApiHelpers {
    public class ProfileInfo : ICloneable {
        public string FormName { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public List<TrusteeInfo> Trustees { get; set; }

        public object Clone() {
            var result = new ProfileInfo { FormName = this.FormName };
            if(Properties != null)
                result.Properties = new Dictionary<string, string>(Properties);
            if(Trustees != null)
                result.Trustees = new List<TrusteeInfo>(Trustees);
            return result;
        }
    }

    public class DocumentInfo {
        public int DocNumber { get; set; }
        public int VersionID { get; set; }
    }

    public class DMDocument : DMBase {
        public const string ObjectFormDefaultProfile = "v_defprof";
        public const string ObjectContentItem = "ContentItem";
        public const string ObjectContentsCollection = "ContentsCollection";
        public const string PropertyTargetLibrary = "%TARGET_LIBRARY";
        public const string PropertyObjectIdentifier = "%OBJECT_IDENTIFIER";
        public const string PropertyVersionID = "%VERSION_ID";
        public const string PropertyDocNumber = "DOCNUMBER";
        public const string PropertyParentFolder = "PARENT";
        public const string PropertyParentFolderVer = "PARENT_VERSION";
        public const string PropertyParentFolderLib = "%FOLDERITEM_LIBRARY_NAME";

        public DocumentInfo CreateDocument(ProfileInfo profile, string localPath) {
            if(string.IsNullOrWhiteSpace(localPath))
                throw new ArgumentNullException(localPath);
            bool isDir = (File.GetAttributes(localPath) & FileAttributes.Directory) == FileAttributes.Directory;
            if(isDir)
                return CreateProfile(profile);
            else
                using(var source = new FileStream(localPath, FileMode.Open, FileAccess.Read)) {
                    return CreateDocument(profile, source);
                }
        }

        public DocumentInfo CreateDocument(ProfileInfo profile, Stream source) {
            var doc = CreateProfile(profile);
            if(source != null) {
                try {
                    var target = GetPcdPutStream(doc);
                    UploadContent(source, target);
                    UnlockDocument(doc);
                }
                catch {
                    // TODO delete profile
                    throw;
                }
            }
            return doc;
        }

        /// <summary>
        /// Creates a document profile
        /// </summary>
        /// <param name="profileInfo"></param>
        /// <returns>Returns the document number and the version of newly created profile.</returns>
        public DocumentInfo CreateProfile(ProfileInfo profileInfo) {
            if(profileInfo == null)
                throw new ArgumentNullException("profileInfo");
            if(profileInfo.Properties == null)
                throw new ArgumentNullException("profileInfo.Properties");

            var doc = new PCDDocObjectClass();
            doc.SetDST(Dst);
            doc.SetObjectType(profileInfo.FormName);
            doc.SetProperty(PropertyTargetLibrary, Library);
            foreach (var pair in profileInfo.Properties)
                doc.SetProperty(pair.Key, pair.Value);

            if(profileInfo.Trustees != null)
                foreach(TrusteeInfo t in profileInfo.Trustees)
                    doc.SetTrustee(t.Trustee, (int)t.TrusteeType, (int)t.AccessRights);
           
            int result = doc.Create();

            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException("Cannot create profile.", doc.ErrNumber, doc.ErrDescription);
            
            object docNum = doc.GetReturnProperty(PropertyObjectIdentifier);
            object verID = doc.GetReturnProperty(PropertyVersionID);
            return new DocumentInfo { DocNumber = Convert.ToInt32(docNum), VersionID = Convert.ToInt32(verID) }; 
        }

        public void DeleteProfile(int docNumber) {
            DeleteProfile(docNumber, false);
        }

        public void DeleteProfile(int docNumber, bool clearLinks) {
            if(clearLinks)
                ClearLinks(docNumber);

            var doc = new PCDDocObjectClass();
            doc.SetDST(Dst);
            doc.SetObjectType(ObjectFormDefaultProfile);
            doc.SetProperty(PropertyTargetLibrary, Library);
            doc.SetProperty(PropertyObjectIdentifier, docNumber);

            int result = doc.Delete();
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException(string.Format("Cannot delete document# {0}.", docNumber), doc.ErrNumber, doc.ErrDescription);
        }

        /*private void CreateNewVersion(string dst, int docNumber, string userID) {
            var docObject = new PCDDocObjectClass();
            docObject.SetDST(dst);
            docObject.SetObjectType("ENG_DRAW_P");//set profile form name
            docObject.SetProperty("%TARGET_LIBRARY", dApp.CurrentLibrary.Name);//database
            docObject.SetProperty("%OBJECT_IDENTIFIER", docNumber);
            docObject.SetProperty("%VERSION_DIRECTIVE", "%PCD_NEW_VERSION");
            docObject.SetProperty("%VERSION_TYPIST", userID);
            int result = docObject.Update();

            if(result != S_OK || docObject.ErrNumber != 0)
                throw new DMApiException(string.Format("PCDDocObjectClass.Update failed with error {0}: {1}", docObject.ErrNumber, docObject.ErrDescription));
            //versionID = docObject.GetReturnProperty("%VERSION_ID").ToString();
        }*/

        public PCDPutStream GetPcdPutStream(DocumentInfo doc) {
            // TODO Check if checked-out / locked
            var pcdPutDoc = new PCDPutDocClass();
            pcdPutDoc.SetDST(Dst);//log into DM - set security token
            pcdPutDoc.AddSearchCriteria(PropertyTargetLibrary, Library);
            pcdPutDoc.AddSearchCriteria("%DOCUMENT_NUMBER", doc.DocNumber);
            pcdPutDoc.AddSearchCriteria(PropertyVersionID, doc.VersionID);
            int result = pcdPutDoc.Execute();
            if(result != S_OK || pcdPutDoc.ErrNumber != 0)
                throw new DMApiException("PCDPutDocClass.Execute has failed.", pcdPutDoc.ErrNumber, pcdPutDoc.ErrDescription);

            // TODO check result count
            pcdPutDoc.NextRow();
            return (PCDPutStream)pcdPutDoc.GetPropertyValue("%CONTENT");
        }

        public void UnlockDocument(DocumentInfo docInfo) {
            var doc = new PCDDocObjectClass();
            doc.SetDST(Dst);
            doc.SetObjectType(ObjectFormDefaultProfile);
            doc.SetProperty(PropertyTargetLibrary, Library);
            doc.SetProperty(PropertyObjectIdentifier, docInfo.DocNumber);
            doc.SetProperty(PropertyVersionID, docInfo.VersionID);
            doc.SetProperty("%STATUS", "%UNLOCK");

            int result = doc.Update();
            if(result != S_OK || doc.ErrNumber != 0)
                if(doc.ErrNumber == DMApiEmptyDocumentFileException.EmptyFileDMErrorCode)
                    throw new DMApiEmptyDocumentFileException(string.Format("An attempt to check-in an empty document (document# {0})", docInfo.DocNumber), doc.ErrNumber, doc.ErrDescription);
                else
                    throw new DMApiException(string.Format("Cannot unlock document# {0}", docInfo.DocNumber), doc.ErrNumber, doc.ErrDescription);
        }

        public void AddLink(int docNumber, DocumentInfo folder) {
            if(folder == null)
                throw new ArgumentNullException("folder");

            var doc = new PCDDocObjectClass();
            doc.SetDST(Dst);
            doc.SetObjectType(ObjectContentItem);
            doc.SetProperty(PropertyTargetLibrary, Library);
            doc.SetProperty(PropertyParentFolder, folder.DocNumber.ToString());
            doc.SetProperty(PropertyParentFolderVer, folder.VersionID.ToString());
            doc.SetProperty(PropertyDocNumber, docNumber.ToString());
            doc.SetProperty(PropertyParentFolderLib, Library);  // we save to one library only
            //doc.SetProperty("DISPLAYNAME", docName);
            doc.SetProperty("VERSION_TYPE", "R");

            int result = doc.Create();
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException(string.Format("Cannot link document# {0} to folder {1}", docNumber, folder.DocNumber), doc.ErrNumber, doc.ErrDescription);
        }

        public DMSqlResultSet FindLinks(int docNumber) {
            var sql = new DMSql { Dst = this.Dst, Library = this.Library };
            var cmd = string.Format("select system_id from docsadm.folder_item where docnumber = {0}", docNumber);
            var result = sql.ExecuteSql(cmd);
            return result;
        }

        public void ClearLinks(int docNumber) {
            var links = FindLinks(docNumber);
            if(links.RowCount == 0) return;

            foreach(var r in links.Rows)
                DeleteLink((string)r.Values[0]);
        }

        private void DeleteLink(string linkId) {
            var doc = new PCDDocObjectClass();
            doc.SetDST(Dst);
            doc.SetObjectType(ObjectContentItem);
            doc.SetProperty(PropertyTargetLibrary, Library);
            doc.SetProperty("SYSTEM_ID", Convert.ToInt32(linkId));

            int result = doc.Delete();
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException(string.Format("Cannot delete link {0}", linkId), doc.ErrNumber, doc.ErrDescription);
        }

        public void UploadContent(Stream sourceStream, PCDPutStream pcdStream) {
            const int ChunkSize = 64 * 1024;

            int writtenCount;
            byte[] data = new byte[ChunkSize];
            
            using(var br = new BinaryReader(sourceStream)) {
                while(true) {
                    int readCount = br.Read(data, 0, ChunkSize);
                    if(readCount == 0) break;
                    pcdStream.Write(data, readCount, out writtenCount);
                    if(writtenCount != readCount)
                        throw new DMApiException(string.Format("PCDPutStream.Write failed: {0} bytes have been written out of {1}", writtenCount, readCount));
                }
            }
            int result = pcdStream.SetComplete();
            if(result != S_OK || pcdStream.ErrNumber != 0)
                throw new DMApiException(string.Format("PCDPutStream.SetComplete failed with error {0}: {1}", pcdStream.ErrNumber, pcdStream.ErrDescription));
        }

        public TrusteeInfo[] FetchTrustees(int docNumber) {
            var doc = new PCDDocObjectClass();
            doc.SetDST(Dst);
            doc.SetObjectType(ObjectFormDefaultProfile);
            doc.SetProperty(PropertyTargetLibrary, Library);
            doc.SetProperty(PropertyObjectIdentifier, docNumber);

            int result = doc.FetchTrustees();
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException(string.Format("Cannot fetch trustees for document# {0}.", docNumber), doc.ErrNumber, doc.ErrDescription);

            var trustees = doc.GetTrustees();
            if(doc.ErrNumber != 0)
                throw new DMApiException(string.Format("Cannot get trustees for document# {0}.", docNumber), doc.ErrNumber, doc.ErrDescription);

            var list = new List<TrusteeInfo>();
            var count = trustees.GetSize();
            if(count > 0) {
                trustees.BeginIter();
                for(int i = 0; i < count; i++) {
                    list.Add(new TrusteeInfo(
                        trustees.GetCurrentTrusteeName(),
                        (TrusteeType)trustees.GetCurrentTrusteeFlags(),
                        (AccessRights)trustees.GetCurrentTrusteeRights()));
                    trustees.NextTrustee();

                }
            }
            return list.ToArray();
        }

        public void UpdateTrustees(int docNumber, TrusteeInfo[] trusteesToSet) {
            if(trusteesToSet == null)
                throw new ArgumentNullException("trusteesToSet");
            if(trusteesToSet.Length == 0)
                throw new ArgumentException("trusteesToSet is empty");

            var doc = new PCDDocObjectClass();
            doc.SetDST(Dst);
            doc.SetObjectType(ObjectFormDefaultProfile);
            doc.SetProperty(PropertyTargetLibrary, Library);
            doc.SetProperty(PropertyObjectIdentifier, docNumber);
            
            int result = doc.FetchTrustees();
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException(string.Format("Cannot fetch trustees for document# {0}.", docNumber), doc.ErrNumber, doc.ErrDescription);

            // season greetings, DM API developers...
            var trustees = doc.GetTrustees();
            if(doc.ErrNumber != 0)
                throw new DMApiException("GetTrustees failed.", doc.ErrNumber, doc.ErrDescription);
            
            int size = trustees.GetSize();
            foreach(var t in trusteesToSet) {
                int idx = trustees.GetTrusteeIndex(t.Trustee, (int)t.TrusteeType);
                // funking DM API returns index equal GetSize() if trustee is not found 
                if(idx == size) {
                    if(t.AccessRights != AccessRights.NoAccess)
                        trustees.AddTrustee(t.Trustee, (int)t.TrusteeType, (int)t.AccessRights);
                }
                else  // update existing
                    if(t.AccessRights == AccessRights.NoAccess)
                        trustees.DeleteTrustee(idx);
                    else
                        trustees.SetTrusteeRights(idx, (int)t.AccessRights);
            }
            result = doc.SetTrustees(trustees);
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException("SetTrustees failed.", doc.ErrNumber, doc.ErrDescription);

            result = doc.UpdateTrustees();
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException("UpdateTrustees failed.", doc.ErrNumber, doc.ErrDescription);
            
            result = doc.Update();
            if(result != S_OK || doc.ErrNumber != 0)
                throw new DMApiException(string.Format("Cannot update document# {0}.", docNumber), doc.ErrNumber, doc.ErrDescription);
        }
    }

        
}
