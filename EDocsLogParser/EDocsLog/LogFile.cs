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
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

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

        private byte[] ReadHeader() {
            const int ReadCount = 6;
            var result = new byte[ReadCount] {1, 1, 1, 1, 1, 1 };
            using(var stream = File.OpenRead(FileName)) {
                stream.Read(result, 0, ReadCount);
                return result;
            }
        }

        // it's not perfect, but okay for DM logs
        // thus, DM 5.2 logs are ANSI with non-zero bytes
        // DM 5.3 logs are Unicode with 00 xx 00 xx pattern
        // if you want to make it perfect, check for solutions online 
        public Encoding GetEncoding() {
            byte[] bytes = ReadHeader();
            if(bytes[0] != 0 && bytes[1] == 0 && bytes[2] != 0 && bytes[3] == 0)
                return Encoding.Unicode;
            return Encoding.ASCII;
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
            return File.ReadAllLines(FileName, GetEncoding());
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