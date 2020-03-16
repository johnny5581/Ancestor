using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// DataAcceesObject update mode
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>Legency mode (Update when reference exist)</summary>
        Value,
        /// <summary>Full row update mode (Update all fields include null)</summary>
        All,
    }
}
