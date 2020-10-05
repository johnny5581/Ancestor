using Ancestor.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction.Options
{
    public class OracleOptions : DbActionOptions
    {
        public OracleOptions()
        {
            BindByName = true;
            AddRowid = false;

            // default value for LOB / LONG fetch size set to -1
            InitializeLOBFetchSize = -1;
            InitializeLONGFetchSize = -1;
        }
        public int? InitializeLONGFetchSize
        {
            get { return Get<int?>(nameof(InitializeLONGFetchSize)); }
            set { this[nameof(InitializeLONGFetchSize)] = value; }
        }
        public int? InitializeLOBFetchSize
        {
            get { return Get<int?>(nameof(InitializeLOBFetchSize)); }
            set { this[nameof(InitializeLOBFetchSize)] = value; }
        }

        public bool BindByName
        {
            get { return Get<bool>(nameof(BindByName)); }
            set { this[nameof(BindByName)] = value; }
        }
        public long? FetchSize
        {
            get { return Get<long?>(nameof(FetchSize)); }
            set { this[nameof(FetchSize)] = value; }
        }
        public bool AddRowid
        {
            get { return Get<bool>(nameof(AddRowid)); }
            set { this[nameof(AddRowid)] = value; }
        }
    }

}
