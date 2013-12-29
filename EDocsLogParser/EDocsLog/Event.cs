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
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EDocsLog {
    public sealed class RawEvent {
        public EventType Type;
        public readonly string Key;

        public RawEvent(string key) {
            Key = key;
        }

        public LogLine Header;
        public LogLine Footer;
        public readonly List<LogLine> Body = new List<LogLine>();
    }

    [XmlInclude(typeof(SqlEvent))]
    public abstract class BaseEvent {
        public readonly RawEvent Raw;

        // for serializer
        private BaseEvent() : this(null) { }

        public BaseEvent(RawEvent raw) {
            Raw = raw;
        }

        [XmlAttribute("key")]
        public virtual string Key {
            get {
                if(Raw == null)
                    return string.Empty;
                return Raw.Key;
            }
            set { } // for serializer
        }

        public abstract string GetUniqueName();
    }
}
