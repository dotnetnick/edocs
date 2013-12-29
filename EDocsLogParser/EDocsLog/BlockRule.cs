using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
