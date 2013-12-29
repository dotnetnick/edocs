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
using System.Runtime.Serialization;
using System.Text;

namespace DMApiHelpers {
    public class DMApiException : Exception {
        public DMApiException() { }
        public DMApiException(string message) : base(message) { }
        public DMApiException(string message, Exception innerException) : base(message, innerException) { }
        public DMApiException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public DMApiException(string message, int errNumber, string errDescription) 
            : base(string.Format("{0} Error {1}: {2}", message, errNumber, errDescription)) { }
    }

    // special exception to be able to supress an error like this:
    // Cannot unlock document# 123456 - error -2147221014: DMSERVERNAME: DMSERVERNAME: eDOCS DM has encountered an unexpected error.  Please notify your administrator.:0 byte file was created:DOCS UserID = NKHORIN:Document Number = 123456:Version ID = 987654
    public class DMApiEmptyDocumentFileException : DMApiException {
        public const int EmptyFileDMErrorCode = -2147221014;
        public DMApiEmptyDocumentFileException(string message, int errNumber, string errDescription)
            : base(message, errNumber, errDescription) { 
            if(errNumber != EmptyFileDMErrorCode)
                throw new ArgumentException(string.Format("errNumber is expected equal to {0}, but was {1}. Throw DMApiException instead.", EmptyFileDMErrorCode, errNumber));
        }
    }
}
