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
using Hummingbird.DM.Server.Interop.PCDClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMApiHelpers {
    public static class DMLogin {
        private static string fPassword;
        private static string fUserName;
        private readonly static object objLock = new object();
        private readonly static PCDLogin Login = new PCDLogin();  // TODO Lazy init
        private static string fDst = null;

        static DMLogin() {
        }

        public static string Dst {
            get {
                if(string.IsNullOrEmpty(fDst))
                    lock(objLock) {
                        if(string.IsNullOrEmpty(fDst))
                            fDst = AcquireDst();
                    }
                return fDst;
            }
        }

        public static string UserName {
            get {
                if(string.IsNullOrEmpty(fUserName))
                    return GetCurrentUser();
                return fUserName;
            }
            set {
                if(fUserName != value) {
                    fUserName = value;
                    ReloginRequired();
                }
            }
        }

        public static string ServerName {
            get { return Login.GetServerName(); }
            set {
                if(ServerName != value) {
                    Login.SetServerName(value);
                    ReloginRequired();
                }
            }
        }
        
        public static string Library {
            get { return Login.GetLoginLibrary(); }
        }

        private static void ReloginRequired() {
            fDst = null;
        }

        public static string Password {
            get { return fPassword; }
            set {
                if(fPassword != value) {
                    fPassword = value;
                    ReloginRequired();
                }
            }
        }

        private static string GetCurrentUser() {
            return Environment.UserName;
        }

        private static string AcquireDst() {
            // TODO Refresh expired or invalid DSTs
            string dst = Login.GetDST();
            if(string.IsNullOrEmpty(dst)) {
                Login.AddLogin(0, "", UserName, Password);
                int result = Login.Execute();
                if(result != 0)
                    throw new Exception(string.Format("Login failed: {0} {1}", Login.ErrNumber, Login.ErrDescription));
                dst = Login.GetDST();
                // TODO check DST again, rerun Execute
            }
            return dst;
        }
    }
}
