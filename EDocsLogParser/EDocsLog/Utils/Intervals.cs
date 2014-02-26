using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDocsLog.Utils {
    // closed bounded interval [a, b] = { x | a <= x <= b }. Can be empty (a==b)
    public sealed class Interval {
        public readonly int a;
        public readonly int b;

        public Interval(int a, int b) {
            this.a = Math.Min(a, b);
            this.b = Math.Max(a, b);
        }

        public Interval(Interval clone) {
            a = clone.a;
            b = clone.b;
        }
    }

    public static class Intervals {
        public static List<Interval> Order(IEnumerable<Interval> intervals) {
            return intervals.OrderBy(i => i.a).ToList();
        }

        /// <summary>
        /// If intervals intersect, returns a new merged interval. Otherwise, returns null.
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static Interval MergeTwo(Interval i1, Interval i2) {
            // order intervals
            var first = i1.a <= i2.a ? i1 : i2;
            var next = (first == i1) ? i2 : i1;

            // do intervals intersect?
            if(next.a >= first.a && next.a <= first.b)
                return new Interval(first.a, Math.Max(first.b, next.b));

            return null;
        }

        public static List<Interval> Merge(IEnumerable<Interval> intervals) {
            var result = new List<Interval>();
            Interval prev = null;
            foreach(var curr in Order(intervals)) {
                if(prev == null) { 
                    prev = new Interval(curr);
                    continue;
                }
                var merged = MergeTwo(prev, curr);
                if(merged == null) {
                    result.Add(prev);
                    prev = curr;
                }
                else
                    prev = merged;
            }
            if(prev != null)
                result.Add(prev);

            return result;
        }
    }
}
