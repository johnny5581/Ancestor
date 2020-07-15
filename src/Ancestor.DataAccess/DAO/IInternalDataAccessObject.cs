using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// Internal DataAccessObject interface
    /// </summary>
    internal interface IInternalDataAccessObject
    {
        /// <summary>
        /// Get Database server time sql
        /// </summary>
        string GetServerTime();
        /// <summary>
        /// Get dummy table name
        /// </summary>        
        string GetDummyTable();
    }
}
