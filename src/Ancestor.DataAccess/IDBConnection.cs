using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess
{
    public interface IDBConnection
    {
        IConnection GetConnectionFactory();
    }
}
