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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDocsLog.Utils;

namespace EDocsLogTests {
    [TestClass]
    public class UnitTestIntervals {
        [TestMethod]
        public void TestMergeTwoMerged() {
            var i = Intervals.MergeTwo(new Interval(1, 1), new Interval(1, 1));
            Assert.IsNotNull(i);
            Assert.AreEqual(1, i.a);
            Assert.AreEqual(1, i.b);

            i = Intervals.MergeTwo(new Interval(1, 2), new Interval(1, 3));
            Assert.IsNotNull(i);
            Assert.AreEqual(1, i.a);
            Assert.AreEqual(3, i.b);

            i = Intervals.MergeTwo(new Interval(1, 3), new Interval(2, 5));
            Assert.IsNotNull(i);
            Assert.AreEqual(1, i.a);
            Assert.AreEqual(5, i.b);

            i = Intervals.MergeTwo(new Interval(2, 5), new Interval(1, 3));
            Assert.IsNotNull(i);
            Assert.AreEqual(1, i.a);
            Assert.AreEqual(5, i.b);
        }

        [TestMethod]
        public void TestMergeTwoNotMerged() {
            var i = Intervals.MergeTwo(new Interval(1, 1), new Interval(2, 2));
            Assert.IsNull(i);

            i = Intervals.MergeTwo(new Interval(5, 2), new Interval(7, 6));
            Assert.IsNull(i);
        }

        [TestMethod]
        public void TestOrder() {
            var first = new Interval(7, 1);
            var second = new Interval(5, 3);
            var sorted = Intervals.Order(new Interval[] { second, first });
            Assert.AreEqual(2, sorted.Count);
            Assert.ReferenceEquals(first, sorted[0]);
            Assert.ReferenceEquals(second, sorted[1]);
        }

        [TestMethod]
        public void TestMerge() {
            var first = new Interval(5, 1);
            var second = new Interval(9, 3);
            var third = new Interval(12, 7);
            var merged = Intervals.Merge(new Interval[] { second, third, first });
            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(first.a, merged[0].a);
            Assert.AreEqual(third.b, merged[0].b);
        }
    }
}
