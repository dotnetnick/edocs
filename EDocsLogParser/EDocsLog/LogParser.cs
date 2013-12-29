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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDocsLog {
    public class LogParser {
        public readonly Dictionary<string, RawEvent> IncompleteChunks = new Dictionary<string, RawEvent>();
        public readonly List<RawEvent> Chunks = new List<RawEvent>(); // the event stub - header and footer only
        
        public readonly List<RawEvent> RawEvents = new List<RawEvent>();
        public readonly List<BaseEvent> Events = new List<BaseEvent>();

        readonly List<ILineRule> headerAndFooterRules = new List<ILineRule>();
        readonly List<ILineRule> bodyRules = new List<ILineRule>();

        public LogParser() {
            InitRules();
        }

        private void InitRules() {
            headerAndFooterRules.Add(new SqlHeaderRule());
            headerAndFooterRules.Add(new SqlFooterRule());
            
            bodyRules.Add(new SqlBodyRule());
            bodyRules.Add(new SqlBodyNotifyRule());
            bodyRules.Add(new SqlBodyUselessLineRule());
            bodyRules.Add(new SqlBodyTimerReadItemRule());
            bodyRules.Add(new SqlBodyTimerIssueCommandRule());
        }

        public LogFile Log { get; set; }

        public string GetLine(int i) {
            lock(_lockLog)
                return Log.Lines[i];
        }

        public bool Parse() {
            if(Log == null)
                return false;

            CleanUp();
            CreateChunks();
            ParseChunks();
            FormatEvents();

            return true;
        }

        public void CleanUp() {
            IncompleteChunks.Clear();
            Chunks.Clear();
            RawEvents.Clear();
            Events.Clear();
        }

        private void CreateChunks() {
            for(int i = 0; i < Log.Lines.Length; i++) {
                string line = GetLine(i);
                if(string.IsNullOrWhiteSpace(line)) 
                    continue;
                foreach(var rule in headerAndFooterRules) {
                    var result = rule.Apply(line);
                    if(ProcessAsChunkHeader(i, result) || ProcessAsChunkFooter(i, result))
                        break; // go to next log line
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
                var msg = string.Format("{5}: Found a new event header with the same key {0} before the footer of the previous event\n" +
                    "Previous header at line {1}: {2}\n" +
                    "New header at line {3}: {4}", key, inc.Header.Index, GetLine(inc.Header.Index), index, GetLine(index), System.IO.Path.GetFileName(Log.FileName)); 
                //throw new Exception(msg);
                Trace.WriteLine(msg);
            }

            IncompleteChunks[key] = new RawEvent(key) { Header = new LogLine(index, result), Type = result.EventType };
            return true;
        }

        private bool ProcessAsChunkFooter(int index, RuleResult result) {
            if(result.LineType != LineType.Footer)
                return false;

            string key = result.Key;
            if(!IncompleteChunks.ContainsKey(key))
                return false;  // footer without header - strange, but we don't need it

            RawEvent inc = IncompleteChunks[key];
            IncompleteChunks.Remove(key);
            inc.Footer = new LogLine(index, result);
            Chunks.Add(inc);
            return true;
        }

        private void ParseChunks() {
            //Chunks.AsParallel().ForAll(ParseChunk);
            Parallel.ForEach(Chunks, ParseChunk);
            Debug.Assert(Chunks.Count == RawEvents.Count, 
                string.Format("Possible multi-threading issue. The number of chunks and completed events were expected to be equal, but it's not: {0} - chunks, {1} - completed events",
                Chunks.Count, RawEvents.Count));
            Chunks.Clear();
        }

        private readonly object _lockRawEvents = new object();
        private readonly object _lockLog = new object();

        private void ParseChunk(RawEvent ev) {
            int processedBodyLines;
            for(int i = ev.Header.Index + 1; i < ev.Footer.Index - 1; i++) {
                string line = GetLine(i);
                foreach(var rule in bodyRules) {
                    var result = rule.Apply(line);
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
            foreach(var raw in RawEvents) {
                switch(raw.Type) {
                    case EventType.Sql:
                        Events.Add(SqlEventFactory.GetEvent(raw));
                        break;
                }
            }
        }

        private BlockRuleResult ProcessRequiredBodyBlock(int index, RuleResult result) {
            string[] blockLines;
            lock(_lockLog) {
                int take = Math.Min(result.RequiredBlockRule.MaxBlockSize, Log.Lines.Length - index - 1);
                blockLines = Log.Lines.Skip(index + 1).Take(take).ToArray();
            }
            return result.RequiredBlockRule.Apply(blockLines);
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
}
