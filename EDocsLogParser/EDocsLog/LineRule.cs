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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDocsLog {
    public interface ILineRule {
        RuleResult Apply(string line);
    }

    public abstract class BaseLineRule : ILineRule {
        protected abstract RuleResult DoApply(string line);
        protected RuleResult CreateResult() {
            return CreateResult(LineType.Unknown, null);
        }
        protected RuleResult CreateResult(LineType lineType) {
            return CreateResult(lineType, null);
        }
        protected virtual RuleResult CreateResult(LineType lineType, string key) {
            return new RuleResult { RuleType = this.GetType(), LineType = lineType, Key = key };
        }

        #region ILineRule Members

        public RuleResult Apply(string line) {
            return DoApply(line);
        }
        #endregion
    }

    public abstract class RegExRule : BaseLineRule {
        public string Pattern {
            get {
                return GetPattern();
            }
        }

        protected override RuleResult DoApply(string line) {
            if(string.IsNullOrWhiteSpace(line))
                return CreateResult(LineType.Empty);

            string pattern = Pattern;
            var match = Regex.Match(line, pattern, 
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            if(match.Success)
                return ProcessMatch(match);

            return CreateResult();
        }

        protected abstract string GetPattern();
        protected abstract RuleResult ProcessMatch(Match match);

    }
}
