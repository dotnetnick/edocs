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
using EDocsLog.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDocsLog {
    public class LogParser {
        public readonly Dictionary<string, RawEvent> IncompleteChunks = new Dictionary<string, RawEvent>();
        public readonly List<RawEvent> Chunks = new List<RawEvent>(); // the event stub - header and footer only
        private readonly ConcurrentDictionary<ILineRule, ConcurrentDictionary<int, RuleResult>> CachedResults = new ConcurrentDictionary<ILineRule, ConcurrentDictionary<int, RuleResult>>();
        
        public readonly List<RawEvent> RawEvents = new List<RawEvent>();
        public readonly List<BaseEvent> Events = new List<BaseEvent>();

        readonly List<ILineRule> headerRules = new List<ILineRule>();
        readonly List<ILineRule> footerRules = new List<ILineRule>();
        readonly List<ILineRule> bodyRules = new List<ILineRule>();

        public LogParser() {
            InitRules();
        }

        private void InitRules() {
            headerRules.Add(new SqlHeaderRule());
            footerRules.Add(new SqlFooterRule());
            
            bodyRules.Add(new SqlBodyRule());
            bodyRules.Add(new SqlBodyNotifyRule());
            bodyRules.Add(new SqlBodyUselessLineRule());
            bodyRules.Add(new SqlBodyTimerReadItemRule());
            bodyRules.Add(new SqlBodyTimerIssueCommandRule());
        }

        public LogFile Log { get; set; }

        public string GetLine(int i) {
            //lock(_lockLog)
                return Log.Lines[i];
        }

        public bool Parse() {
            if(Log == null)
                return false;

            CleanUp();
            CreateChunks();
            OptimizeLogMemory();
            ParseChunks();
            FormatEvents();

            return true;
        }

        public int ProgressReportStep = 5;
        int ParseChunkCounter = 0;
        int XPerCents;

        public event ParserProgressEventHandler Progress;
        
        protected void RaiseProgress(ParserProgressStage stage, int percent) {
            if(Progress != null)
                Progress(this, new ParserProgressEventArgs(stage, percent));
        }

        public void CleanUp() {
            IncompleteChunks.Clear();
            Chunks.Clear();
            RawEvents.Clear();
            Events.Clear();
            CachedResults.Clear();
        }

        private void CreateChunks() {
            XPerCents = Log.Lines.Length * ProgressReportStep / 100;
            for(int i = 0; i < Log.Lines.Length; i++) {
                if(XPerCents > 0 && i % XPerCents == 0)
                    RaiseProgress(ParserProgressStage.CreatingChunks, i * ProgressReportStep / XPerCents);

                string line = GetLine(i);
                if(string.IsNullOrWhiteSpace(line)) 
                    continue;
                foreach(var rule in headerRules) {
                    var result = rule.Apply(line);
                    ProcessAsChunkHeader(i, result);
                }

                if(IncompleteChunks.Count > 0)
                    foreach(var rule in footerRules) {
                        var result = rule.Apply(line);
                        ProcessAsChunkFooter(i, result);
                    }
            }
        }

        private bool ProcessAsChunkHeader(int index, RuleResult result) {
            if(result.LineType != LineType.Header)
                return false;

            string key = result.Key;
            if(IncompleteChunks.ContainsKey(key)) {
                //IncompleteChunks.Remove(key);
                //I've modified regexps. Let's give edocs another try...
                RawEvent inc = IncompleteChunks[key];

                LogMessage("{5}: Found a new event header with the same key {0} before the footer of the previous event\n" +
                    "\tPrevious header at line {1}: {2}\n" +
                    "\tNew header at line {3}: {4}", 
                    key, inc.Header.Index, GetLine(inc.Header.Index), index, GetLine(index), System.IO.Path.GetFileName(Log.FileName)); 
            }

            IncompleteChunks[key] = new RawEvent(key) { Header = new LogLine(index, result), Type = result.EventType };
            return true;
        }

        private void LogMessage(string format, params object[] args) {
            var msg = string.Format(format, args);
            Trace.WriteLine(msg);
        }

        private bool ProcessAsChunkFooter(int index, RuleResult result) {
            if(result.LineType != LineType.Footer)
                return false;

            string key = result.Key;
            if(!IncompleteChunks.ContainsKey(key)) {
                LogMessage("{2}: Footer without header at {0}: {1}", index, GetLine(index), System.IO.Path.GetFileName(Log.FileName));
                return false;  // footer without header - strange, but we don't need it
            }

            RawEvent inc = IncompleteChunks[key];
            IncompleteChunks.Remove(key);
            if(inc.Header.Index >= index) {
                LogMessage("{4}: Footer above header at {0}: {1}\n\tHeader at {2}: {3}", index, GetLine(index), inc.Header.Index, GetLine(inc.Header.Index), System.IO.Path.GetFileName(Log.FileName));
                return false;
            }

            inc.Footer = new LogLine(index, result);
            Chunks.Add(inc);
            return true;
        }

        // clears unused log lines to free some memory before chunks parsing
        private void OptimizeLogMemory() {
            Stopwatch sw = Stopwatch.StartNew();

            var intervals = Chunks.Select(c => new Interval(c.Header.Index, c.Footer.Index));
            var merged = Intervals.Merge(intervals);
            int counter = 0;
            // trim middles
            for(int i = 0; i < merged.Count - 1; i++)
                for(int idx = merged[i].b; idx <= merged[i + 1].a; idx++) {
                    Log.FreeLine(i);
                    counter++;
                }

            Trace.WriteLine(string.Format("{0} lines freed in {1}.", counter, sw.Elapsed));
            GC.Collect();
        }

        private void ParseChunks() {
            ParseChunkCounter = 0;
            XPerCents = Chunks.Count * ProgressReportStep / 100;

            //Chunks.ForEach(ParseChunk);
            Parallel.ForEach(Chunks, ParseChunk);
            Debug.Assert(Chunks.Count == RawEvents.Count, 
                string.Format("Possible multi-threading issue. The number of chunks and completed events were expected to be equal, but it's not: {0} - chunks, {1} - completed events",
                Chunks.Count, RawEvents.Count));
            Chunks.Clear();
        }

        private readonly object _lockRawEvents = new object();
        private readonly object _lockEvents = new object();
        private readonly object _lockLog = new object();

        private void ParseChunk(RawEvent ev) {
            if(XPerCents > 0 && ++ParseChunkCounter % XPerCents == 0)
                RaiseProgress(ParserProgressStage.ParsingChunks, ParseChunkCounter * ProgressReportStep / XPerCents);
            
            int processedBodyLines;
            for(int i = ev.Header.Index + 1; i < ev.Footer.Index - 1; i++) {
                string line = GetLine(i);
                foreach(var rule in bodyRules) {
                    var result = rule.Apply(line); //RuleApplyCached(rule, i, line);
                    if(ProcessAsChunkBody(i, result, ev, out processedBodyLines)) {
                        i += processedBodyLines;
                        break;
                    }
                }
            }
            lock(_lockRawEvents)
                RawEvents.Add(ev);  //TODO rewrite with ReaderWriterLockSlim 
        }

        private bool ProcessAsChunkBody(int index, RuleResult result, RawEvent raw, out int processedBodyLines) {
            processedBodyLines = 0;
            if(result.LineType != LineType.Body)
                return false;

            if(result.Key != raw.Key)
                return false;

            string key = result.Key;
            raw.Body.Add(new LogLine(index, result));

            //TODO require the RequiredRule
            if(result.RequiredBlockRule != null) {
                var blockResult = ProcessRequiredBodyBlock(index, result);
                if(blockResult != null) {
                    foreach(var pair in blockResult.LineRuleResults) {
                        int idx = pair.Key + index + 1;
                        raw.Body.Add(new LogLine(idx, pair.Value) { Content = GetLine(idx) });
                    }
                    processedBodyLines = blockResult.ProcessedCount;
                }
            }
            return true;
        }

        private void FormatEvents() {
            int counter = 0;
            XPerCents = RawEvents.Count * ProgressReportStep / 100;

            foreach(var raw in RawEvents) {
                if(XPerCents > 0 && ++counter % XPerCents == 0)
                    RaiseProgress(ParserProgressStage.Formatting, counter * ProgressReportStep / XPerCents);

                FormatEvent(raw);
            }
        }

        private void FormatEvent(RawEvent raw) {
            switch(raw.Type) {
                case EventType.Sql:
                    lock(_lockEvents)
                        Events.Add(SqlEventFactory.GetEvent(raw));
                    break;
            }
        }
        private BlockRuleResult ProcessRequiredBodyBlock(int index, RuleResult result) {
            string[] blockLines;
            int take = Math.Min(result.RequiredBlockRule.MaxBlockSize, Log.Lines.Length - index - 1);
            blockLines = Log.Lines.Skip(index + 1).Take(take).ToArray();
            return result.RequiredBlockRule.Apply(blockLines);
        }

        private RuleResult RuleApplyCached(ILineRule rule, int index, string line) {
            RuleResult result;
            ConcurrentDictionary<int, RuleResult> results;
            if(CachedResults.ContainsKey(rule)) {
                results = CachedResults[rule];
                if(results.ContainsKey(index))
                    return results[index];
            }
            else {
                results = new ConcurrentDictionary<int, RuleResult>();
                CachedResults.AddOrUpdate(rule, results, (ru, rs) => rs);
            }

            result = rule.Apply(line);
            results.AddOrUpdate(index, result, (i, r) => r);
            return result;
        }
    }

    public class LogLine {
        public LogLine(int idx, RuleResult rule) {
            Index = idx;
            if(rule != null) {
                RuleType = rule.RuleType;
                if(rule.Values != null && rule.Values.Count > 0)
                    Values = rule.Values;
            }
        }

        public readonly int Index;
        public string Content;
        public Type RuleType;
        public readonly Dictionary<string, string> Values;
    }

    public delegate void ParserProgressEventHandler(object sender, ParserProgressEventArgs e);

    public enum ParserProgressStage { Unknown, CreatingChunks, ParsingChunks, Formatting, Done }

    public class ParserProgressEventArgs : EventArgs {
        public int Percentage { get; private set; }
        public ParserProgressStage Stage { get; private set; }

        public ParserProgressEventArgs(ParserProgressStage stage, int percent) {
            Stage = stage;
            Percentage = percent;
        }
    }
}
