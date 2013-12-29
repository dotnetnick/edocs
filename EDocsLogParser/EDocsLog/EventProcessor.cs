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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EDocsLog {
    public static class EventFilters {
        public static bool IsTrue(BaseEvent ev) {
            return true;
        }

        public static TimeSpan SlowSlqThreshold = TimeSpan.FromSeconds(10); // seconds

        public static bool IsSqlSlow(BaseEvent ev) {
            if(ev is SqlEvent) {
                foreach(var qry in ((SqlEvent)ev).Queries) {
                    var durRead = SqlEventFactory.EDocsSecondsToTimeSpan(qry.DurationReadItem);
                    var durIssue = SqlEventFactory.EDocsSecondsToTimeSpan(qry.DurationIssueCommand);
                    if(durRead > SlowSlqThreshold || durIssue > SlowSlqThreshold)
                        return true;
                }
            }
            return false;
        }


    }

    public static class EventProcessors {
        public static void DoNothing(BaseEvent ev) {
        }


        public static void SaveToFile(BaseEvent ev) {
            if(ev is SqlEvent) {
                var sql = (SqlEvent)ev;
                //fileName = sql.ConnectionIndex
                
            }
        }
        

    }


    public class EventScanner {

        public void Scan(IEnumerable<BaseEvent> events, Func<BaseEvent, bool> filter, Action<BaseEvent> processEvent) {
            //]events.AsParallel().Where(filter).ForAll(processEvent);
            Parallel.ForEach(events.Where(filter), processEvent);
        }

        //TODO move
        public static void SerializeToXml<T>(T obj, string fileName) {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using(var fileStream = new FileStream(fileName, FileMode.Create)) {
                ser.Serialize(fileStream, obj);
                fileStream.Close();
            }
        }
    }

}
