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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMApiHelpers {
    //TODO use PCD_TRUSTEE_GROUP_TYPE etc constants
    public enum TrusteeType { Unknown = 0, Group = 1, Person = 2 }

    /*
    Value   Token                   Description
    1       %PR_VIEW                View Profile
    2       %PR_EDIT                Edit Profile
    4       %PR_CONTENT_VIEW        View Document Content
    8       %PR_CONTENT_RETRIEVE    Retrieve Document Content
    16      %PR_CONTENT_EDIT        Edit Document Content
    32      %PR_CONTENT_COPY        Copy Document Content
    64      %PR_DELETE              Delete Document
    128     %PR_ACCESS_CONTROL      Control Access to Document
    256     %RIGHT8                 Assign to File
    512     %RIGHT9                 View Only Published
    */
    [Flags]
    public enum AccessRights {
        View = 1,
        Edit = 2,
        ContentView = 4,
        ContentRetrieve = 8,
        ContentEdit = 16,
        ContentCopy = 32,
        Delete = 64,
        AccessControl = 128,

        NoAccess = 0,
        ReadOnly = View | ContentView | ContentRetrieve | ContentCopy,
        NormalAccess = View | Edit | ContentView | ContentRetrieve | ContentEdit | ContentCopy,
        FullAccess = View | Edit | ContentView | ContentRetrieve | ContentEdit | ContentCopy | Delete | AccessControl
    }

    public class TrusteeInfo {
        public TrusteeInfo() { }
        public TrusteeInfo(string trustee, TrusteeType type, AccessRights rights) {
            Trustee = trustee;
            TrusteeType = type;
            AccessRights = rights;
        }

        public string Trustee;
        public TrusteeType TrusteeType;
        public AccessRights AccessRights;

        public override bool Equals(object obj) {
            if(obj is TrusteeInfo) {
                var t = (TrusteeInfo)obj;
                
                bool namesEqual = (string.Compare(t.Trustee, this.Trustee, true) == 0);
                bool typesEqual = t.TrusteeType == TrusteeType.Unknown 
                    || this.TrusteeType == TrusteeType.Unknown 
                    || t.TrusteeType == this.TrusteeType;
                return namesEqual && typesEqual && t.AccessRights == this.AccessRights;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
