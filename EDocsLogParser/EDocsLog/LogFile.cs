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