using Hummingbird.DM.Server.Interop.PCDClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMApiHelpers {
    public class DMSqlRow {
        public object[] Values;
    }

    public class DMSqlResultSet {
        public DMSqlResultSet() {
            Rows = new DMSqlRow[RowCount];
            ColumnNames = new string[ColumnCount];
        }

        private int fColumnCount;
        public int ColumnCount {
            get { return fColumnCount; }
            set {
                if(fColumnCount != value) {
                    fColumnCount = value;
                    ColumnNames = new string[fColumnCount];
                }
            }
        }

        private int fRowCount;
        public int RowCount {
            get { return fRowCount; }
            set {
                if(fRowCount != value) {
                    fRowCount = value;
                    Rows = new DMSqlRow[fRowCount];
                }
            }
        }
        public int RowsAffected { get; set; }
        public string[] ColumnNames { get; private set; }
        public DMSqlRow[] Rows { get; private set; }
    }

    public class DMSql : DMBase {
        //Error -2147220396: Search did not return result list.
        const int ErrNoResults = -2147220396;

        public DMSqlResultSet ExecuteSql(string command) {
            var sql = new PCDSQLClass();
            sql.SetDST(Dst);
            sql.SetLibrary(Library);
            int ret = sql.Execute(command);
            if(ret != S_OK || sql.ErrNumber != 0)
                throw new DMApiException(string.Format("PCDSQL.Execute failed with SQL error {0}.", sql.GetSQLErrorCode()), sql.ErrNumber, sql.ErrDescription);

            var result = new DMSqlResultSet {
                ColumnCount = sql.GetColumnCount(),
                RowCount = sql.GetRowCount(),
                RowsAffected = sql.GetRowsAffected()
            };
            if(sql.ErrNumber != 0 && sql.ErrNumber != ErrNoResults)
                throw new DMApiException("PCDSQL failed.", sql.ErrNumber, sql.ErrDescription);

            try {
                for(int i = 0; i < result.ColumnCount; i++)
                    result.ColumnNames[i] = Convert.ToString(sql.GetColumnName(i + 1)); // hi Basic lovers :)

                for(int i = 0; i < result.RowCount; i++) {
                    var row = new DMSqlRow { Values = new object[result.ColumnCount] };
                    for(int j = 0; j < result.ColumnCount; j++)
                        row.Values[j] = sql.GetColumnValue(j + 1); // hi again...
                    result.Rows[i] = row;
                    sql.NextRow();
                }
                return result;
            }
            finally {
                sql.ReleaseResults();
            }
        }
    }
}
