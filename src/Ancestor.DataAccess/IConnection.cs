using Ancestor.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess
{
    /// <summary>
    /// Connection object interface
    /// </summary>
    public interface IConnection : IDisposable
    {
        DBObject dBObject { get; set; }
        void SetConnectionObject(DBObject dbObject);
        IDbConnection GetConnectionObject();
    }
}
