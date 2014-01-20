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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace EDocsLog {
    public static class SqlEventFactory {
        public static DateTime EDocsTimeToDateTime(string time) {
            //sample: 15:01:02.787
            return Convert.ToDateTime(time, CultureInfo.InvariantCulture);
            //return DateTime.ParseExact(time, "HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        public static TimeSpan EDocsSecondsToTimeSpan(string seconds) {
            var sec = Convert.ToDouble(seconds, CultureInfo.InvariantCulture);
            return TimeSpan.FromSeconds(sec);
        }

        public static BaseEvent GetEvent(RawEvent raw) {
            if(raw.Type != EventType.Sql)
                throw new ArgumentException("Sql event expected");

            var ev = new SqlEvent(raw);
            ev.ConnectionIndex = Convert.ToInt32(raw.Header.Values[ValueKeys.Index]);
            ev.IsNew = raw.Header.Values[ValueKeys.IsNew];

            // TODO: fucking edocs... Body is sometimes missing...
            SqlQueryEvent query = new SqlQueryEvent();
            foreach(var bodyLine in raw.Body) {
                if(bodyLine.RuleType == typeof(SqlBodyRule) || bodyLine.RuleType == typeof(SqlBodyNotifyRule)) {
                    query = new SqlQueryEvent();
                    query.Time = bodyLine.Values[ValueKeys.Time];
                    ev.Queries.Add(query);
                    //TODO: line index
                }
                /*else if(bodyLine.RuleType == typeof(SqlStatementRequiredRule.AutoCommitLineRule) {
                    query.AutoCommit = bodyLine.Content; // TODO: On or Off only
                }*/
                else if(bodyLine.RuleType == typeof(SqlStatementRequiredRule.StatementLineRule))
                    query.Command = bodyLine.Content;
                else if(bodyLine.RuleType == typeof(SqlBodyTimerReadItemRule))
                    query.DurationReadItem = bodyLine.Values[ValueKeys.Duration];
                else if(bodyLine.RuleType == typeof(SqlBodyTimerIssueCommandRule))
                    query.DurationIssueCommand = bodyLine.Values[ValueKeys.Duration];
            }
            return ev;
        }
    }

    [XmlRoot("Sql")]
    public class SqlEvent : BaseEvent {
        readonly List<SqlQueryEvent> queries = new List<SqlQueryEvent>();

        private SqlEvent() : this(null) { } // for serializer

        public SqlEvent(RawEvent raw) : base(raw) { }

        [XmlAttribute("connection")]
        public int ConnectionIndex { get; set; }
        [XmlAttribute("isNew")]
        public string IsNew { get; set; }

        public List<SqlQueryEvent> Queries {
            get { return queries; }
            set {
                // for serializer
            }
        }

        public override string GetUniqueName() {
            throw new NotImplementedException();
        }
    }

    [XmlRoot("Query")]
    public class SqlQueryEvent {
        [XmlAttribute("time")]
        public string Time { get; set; }
        //public string AutoCommit { get; set; }
        [XmlElement("Command")]
        public string Command { get; set; }
        [XmlAttribute("readItemDuration")]
        public string DurationReadItem { get; set; }
        [XmlAttribute("issueCommandDuration")]
        public string DurationIssueCommand { get; set; }
        //TODO: RowCount
        //TODO: Error
    }


    public static class ValueKeys {
        public const string Sec = "sec";
        public const string IsNew = "new";
        public const string Index = "pool";
        public const string Time = "time";
        public const string Duration = "dur";
    }

    public abstract class SqlBaseRule : RegExRule {
        protected override RuleResult CreateResult(LineType lineType, string key) {
            RuleResult result = base.CreateResult(lineType, key);
            if(lineType != LineType.Unknown && lineType != LineType.Empty)
                result.EventType = EventType.Sql;
            return result;
        }
    }

    public class SqlHeaderRule : SqlBaseRule {
        
        protected override RuleResult DoApply(string line) {
            return base.DoApply(line);
        }
        protected override string GetPattern() {
            // sample line:
            // DOCSSQL: [0F050918] SQLObject at 0F1E45F8 acquired existing connection from pool #1  -- from pool
            // DOCSSQL: [13C59BC0] SQLObject at 11378CE0 acquired new connection for existing Pool #6 -- new created
            return @"DOCSSQL: \[(?<key>[0-9A-F]{8})\] SQLObject at ([0-9A-F]{8}) acquired (existing|new).+ool #([\d])";
        }
        protected override RuleResult ProcessMatch(Match match) {
            return new RuleResult { 
                EventType = EventType.Sql, LineType = LineType.Header, Key = match.Groups["key"].Value,
                Values = new Dictionary<string, string>() { 
                    { ValueKeys.Sec, match.Groups[1].Value },  
                    { ValueKeys.IsNew, (match.Groups[2].Value == "new").ToString() },
                    { ValueKeys.Index, match.Groups[3].Value } }
            };
        }
    }

    public class SqlBodyRule : SqlBaseRule {
        const int MaxBodySize = 5;

        protected override RuleResult DoApply(string line) {
            return base.DoApply(line);
        }
        protected override string GetPattern() {
            // sample line:
            // ********** 09:33:17.392  [13C595F0] DOCSSQL: EXECute SQL Statement on Library:MYLIB - MYDB  (Oracle7) **********
            return @"^\*+ (?<time>\d\d\:\d\d\:\d\d\.\d{3})  \[(?<key>[0-9A-F]{8})\] DOCSSQL: EXECute SQL Statement.+$";
        }
        protected override RuleResult ProcessMatch(Match match) {
            RuleResult result = CreateResult(LineType.Body, match.Groups["key"].Value);
            result.Values = new Dictionary<string, string>() { { ValueKeys.Time, match.Groups[ValueKeys.Time].Value } };
            result.RequiredBlockRule = new SqlStatementRequiredRule();
            return result;
        }
    }

    public abstract class StartsWithBodyLineRule : BaseLineRule {
        protected abstract string[] StartsWith { get; }
        protected override RuleResult DoApply(string line) {
            foreach(var sw in StartsWith) {
                if(line.StartsWith(sw))
                    return CreateResult(LineType.Body);
            }
            return CreateResult(LineType.Unknown);
        }
    }

    public class SqlStatementRequiredRule : BaseBlockRule {
        class AutoCommitLineRule : StartsWithBodyLineRule {
            protected override string[] StartsWith {
                get { return new string[] {"Autocommit is "}; }
            }
        }
        class StatementTitleLineRule : StartsWithBodyLineRule {
            protected override string[] StartsWith {
                get { return new string[] { "STATEMENT:" }; }
            }
        }
        public class StatementLineRule : StartsWithBodyLineRule {
            //TODO: capture FOR ALL statements as well?
            protected override string[] StartsWith {
                get { return new string[] { "SELECT ", "UPDATE ", "INSERT ", "DELETE " }; }
            }
        }

        //TODO: move the RequiredLines list to base class
        private readonly List<ILineRule> requiredLines = new List<ILineRule>() {
            new AutoCommitLineRule(), new StatementTitleLineRule(), new StatementLineRule()
        };

        private List<ILineRule> RequiredLineRules {
            get {
                return requiredLines;
            }
        }

        protected override BlockRuleResult DoApply(string[] lines) {
            var extractedLines = new List<int>();

            var ruleEnumerator = RequiredLineRules.GetEnumerator();
            if(!ruleEnumerator.MoveNext())
                throw new InvalidProgramException("RequiredLineRules list seems to be empty.");

            var lineRuleResults = new Dictionary<int, RuleResult>();

            for(int i = 0; i < lines.Length; i++) {
                string line = lines[i];
                if(string.IsNullOrWhiteSpace(line))
                    continue;
                
                var lineResult = ruleEnumerator.Current.Apply(line);
                if(lineResult.LineType == LineType.Body) {
                    extractedLines.Add(i);
                    lineRuleResults.Add(i, lineResult);
                }
                else
                    return null;  // Sequence failed, processed nothing

                // we processed all rules
                if(!ruleEnumerator.MoveNext())
                    break;
            }

            return new BlockRuleResult { 
                ProcessedCount = extractedLines.Max() + 1,
                LineRuleResults = lineRuleResults
            };
        }
    }

    /// <summary>
    /// Various notification statements, we take into account, but which are rather useless
    /// </summary>
    public class SqlBodyNotifyRule : SqlBaseRule {
        protected override RuleResult DoApply(string line) {
            return base.DoApply(line);
        }
        protected override string GetPattern() {
            // sample lines:
            // ********** 15:01:02.787  [0F050918] DOCSSQL: Performing CANCEL on Library:MYLIB - MYDB  (Oracle7) **********
            // ********** 09:33:44.096  [0F050918] DOCSSQL: DOCSODBC::Batched commands not supported on Library:MYLIB - MYDB  (Oracle7) **********
            // ********** 09:33:44.128  [0F050918] DOCSSQL: performing COMMIT on Library:MYLIB - MYDB  (Oracle7) **********
            // new in DM 5.3.1 Patch 5 - SQL batching
            // ********** 15:54:12.065  [00B97940] DOCSSQL: Adding batch command on Library:MYLIB - MYDB  (Oracle7) **********
            // ********** 15:54:12.066  [00B97940] DOCSSQL: Executing saved batched commands on Library:MYLIB - MYDB  (Oracle7) **********
            // ********** 15:54:12.066  [00B97940] DOCSSQL: Preparing long data on Library:MYLIB - MYDB  (Oracle7) **********
            // ********** 15:54:12.066  [00B97940] DOCSSQL: Running SQLExecDirect on Library:MYLIB - MYDB  (Oracle7) **********
            return @"^\*+ (?<time>\d\d\:\d\d\:\d\d\.\d{3})  \[(?<key>[0-9A-F]{8})\] DOCSSQL: (Performing CANCEL|DOCSODBC\:\:Batched commands|performing COMMIT|Adding batch command|Executing saved batched commands|Preparing long data|Running SQLExecDirect).+$";
        }
        protected override RuleResult ProcessMatch(Match match) {
            RuleResult result = CreateResult(LineType.Body, match.Groups["key"].Value);
            result.Values = new Dictionary<string, string>() { { ValueKeys.Time, match.Groups[ValueKeys.Time].Value } };
            return result;
        }
    }

    public abstract class SqlBodyTimerBaseRule : SqlBaseRule {
        protected override RuleResult DoApply(string line) {
            return base.DoApply(line);
        }

        private static string GetDuration(string matchDuration) {
            // sample matchDuration:
            // 0 milliseconds
            // 21.860 seconds
            // 15 milliseconds  
            var durationAndUnit = matchDuration.Split(' ');
            string duration = durationAndUnit[0];
            if(durationAndUnit[1] == "milliseconds")
                duration = "0." + duration.PadLeft(3, '0');
            return duration;
        }

        protected override RuleResult ProcessMatch(Match match) {
            string matchDuration = match.Groups[1].Value;
            string duration = GetDuration(matchDuration);

            RuleResult result = CreateResult(LineType.Body, match.Groups["key"].Value);
            result.Values = new Dictionary<string, string>() { { ValueKeys.Duration, duration } };
            return result;
        }
    }

    public class SqlBodyTimerReadItemRule : SqlBodyTimerBaseRule {
        protected override string GetPattern() {
            // sample lines:
            // TIMER:   [105277B8] ODBCHandle::ReadItem(): 0 milliseconds  Fetched first row
            // TIMER:   [10528070] ODBCHandle::ReadItem(): 21.860 seconds  Fetched first row
            // note: sometimes another log entry appears in front of "Fetch first...", so we are not checking to the end of line
            return @"^TIMER:   \[(?<key>[0-9A-F]{8})\] ODBCHandle::ReadItem\(\)\: (\d+ milliseconds|\d+\.\d+ seconds)";
        }
    }

    public class SqlBodyTimerIssueCommandRule : SqlBodyTimerBaseRule {
        protected override string GetPattern() {
            // sample lines:
            // TIMER:   [105277B8] ODBCHandle::IssueCommand(): 0 milliseconds  
            // TIMER:   [10528070] ODBCHandle::IssueCommand(): 3.765 seconds  
            return @"^TIMER:   \[(?<key>[0-9A-F]{8})\] ODBCHandle::IssueCommand\(\)\: (\d+ milliseconds|\d+\.\d+ seconds)";
        }
    }

    public class SqlBodyUselessLineRule : SqlBaseRule {
        protected override RuleResult DoApply(string line) {
            return base.DoApply(line);
        }
        protected override string GetPattern() {
            // sample lines:
            // DOCSSQL: [0F050918] ODBCHandle::IssueCommand(): Statement returned no results
            // DOCSSQL: [105277B8] ODBCHandle::IssueCommand(): Statement returned results
            // DOCSSQL: [00B97940] ODBCHandle::IssueCommand(): Statement returned results.  32 rows per fetch.  11232 bytes allocated for result sets.
            // DOCSSQL: [10528070] ODBCHandle::ClearResults(): 23 row(s) fetched
            // DOCSSQL: [00B97940] ODBCHandle::BatchExecute(): Batched command #1 affected 1 row(s)
            return @"^DOCSSQL: \[(?<key>[0-9A-F]{8})\] ODBCHandle::.+$";
        }
        protected override RuleResult ProcessMatch(Match match) {
            return CreateResult(LineType.Body, match.Groups["key"].Value);
        }
    }

    public class SqlFooterRule : SqlBaseRule {
        protected override RuleResult DoApply(string line) {
            return base.DoApply(line);
        }
        protected override string GetPattern() {
            // sample line:
            // DOCSSQL: [13C59BC0] SQLObject at 11378CE0 released connection back to pool
            return @"DOCSSQL: \[(?<key>[0-9A-F]{8})\] SQLObject at ([0-9A-F]{8}) released";
        }
        protected override RuleResult ProcessMatch(Match match) {
            RuleResult result = CreateResult(LineType.Footer, match.Groups["key"].Value);
            result.Values = new Dictionary<string, string>() { { ValueKeys.Sec, match.Groups[1].Value } }; 
            return result;
        }
    }
}
