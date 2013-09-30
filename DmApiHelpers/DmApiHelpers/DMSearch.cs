using Hummingbird.DM.Server.Interop.PCDClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMApiHelpers {
    public enum SortOrder { Descending = 0, Ascending = 1 }

    public class SearchInfo {
        public string SearchObject { get; set; }
        public Dictionary<string, string> Criteria { get; set; }
        public Dictionary<string, SortOrder> OrderBy { get; set; }
        public List<string> ReturnProperties { get; set; }
    }

    public class SearchRow : Dictionary<string, object> {
    }

    //TODO Optimize this. List for every row is an overhead
    public class SearchResults {
        public readonly List<SearchRow> Rows = new List<SearchRow>();
    }

    public class DMSearch : DMBase {
        public SearchResults Search(SearchInfo info) {
            if(info == null)
                throw new ArgumentNullException();

            var search = new PCDSearch();
            search.SetDST(Dst);
            search.SetSearchObject(info.SearchObject);
            foreach(var pair in info.Criteria)
                search.AddSearchCriteria(pair.Key, pair.Value);
            foreach(var prop in info.ReturnProperties)
                search.AddReturnProperty(prop);
            if(info.OrderBy != null)
                foreach(var pair in info.OrderBy)
                    search.AddOrderByProperty(pair.Key, (int)pair.Value);

            if(search.Execute() != 0 || search.ErrNumber != 0)
                throw new DMApiException(string.Format("PCDSearch.Execute failed with error {0}: {1}", search.ErrNumber, search.ErrDescription));
            var results = new SearchResults();
            int count = search.GetRowsFound();
            if(count > 0) {
                results.Rows.Capacity = count;
                search.BeginGetBlock();
                try {
                    for(int i = 0; i < count; i++) {
                        search.NextRow();
                        var row = new SearchRow();
                        foreach(var prop in info.ReturnProperties)
                            row.Add(prop, search.GetPropertyValue(prop));
                        results.Rows.Add(row);
                    }
                }
                finally {
                    search.EndGetBlock();
                    search.ReleaseResults();
                }
            }
            return results;
        }
    }
}