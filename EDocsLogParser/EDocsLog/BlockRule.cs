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
using System.Linq;
using System.Collections.Generic;

namespace EDocsLog {
    public interface IBlockRule {
        int MaxBlockSize { get; }
        BlockRuleResult Apply(string[] lines);
    }

    public abstract class BaseBlockRule : IBlockRule {
        protected virtual int GetMaxBlockSize() {
            return 20;
        }
        protected abstract BlockRuleResult DoApply(string[] lines);

        #region IBlockRule Members

        public int MaxBlockSize {
            get {
                return GetMaxBlockSize();
            }
        }

        public BlockRuleResult Apply(string[] lines) {
            if(lines == null)
                throw new ArgumentNullException("lines");
            return DoApply(lines);
        }

        #endregion
    }

    public class BlockRuleResult {
        public int ProcessedCount;
        public Dictionary<int, RuleResult> LineRuleResults;
    }
}
