#region Copyright (c) 2014 Nick Khorin
/*
{*******************************************************************}
{                                                                   }
{       Tools and examples for OpenText eDOCS DM                    }
{       by Nick Khorin                                              }
{                                                                   }
{       Copyright (c) 2013-2014 Nick Khorin                         }
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
#endregion Copyright (c) 2014 Nick Khorin
using System;
using System.Linq;
using System.Collections.Generic;

namespace EDocsLog {
    public enum EventType { Unknown, Sql, Call }
    public enum LineType { Unknown, Empty, Body, Header, Footer }
 
    public class RuleResult {
        public EventType EventType;
        public LineType LineType;
        public Type RuleType;
        public IBlockRule RequiredBlockRule;

        public string Key;
        public Dictionary<string, string> Values;
    }
}
