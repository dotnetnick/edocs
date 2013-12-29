using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
