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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDocsLog {
    public class LogFile {
        //ConcurrentBag<string> fLines;
        string[] fLines;

        public string Name {get; set;}
        public string FileName { get; set; }

        public string[] Lines {
            get {
                if(fLines == null)
                    fLines = ReadAllLines();
                return fLines;
            }
        }

        public int LineCount {
            get {
                return fLines == null ? 0 : fLines.Length;
            }
        }

        public void FreeLine(int index) {
            if(index >= 0 && index < LineCount)
                fLines[index] = null;
        }
        
        public string[] ReadAllLines() {
            // let it fail if the file doesn't exist
            return File.ReadAllLines(FileName, Encoding.Unicode);
        }

        public string LinesToText(int[] indexes) {
            if(indexes == null)
                throw new ArgumentNullException("indexes");
            StringBuilder sb = new StringBuilder();
            foreach(var idx in indexes)
                sb.AppendLine(Lines[idx]);
            return sb.ToString();
        }

        public string[] LinesToArray(int[] indexes) {
            if(indexes == null)
                throw new ArgumentNullException("indexes");
            var list = new List<string>();
            foreach(var idx in indexes)
                list.Add(Lines[idx]);
            return list.ToArray();
        }
    }
}