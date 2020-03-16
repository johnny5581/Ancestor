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
            AddRowid = true;
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
        public int? FetchSize
        {
            get { return Get<int?>(nameof(FetchSize)); }
            set { this[nameof(FetchSize)] = value; }
        }
        public bool AddRowid
        {
            get { return Get<bool>(nameof(AddRowid)); }
            set { this[nameof(AddRowid)] = value; }
        }
    }

}
