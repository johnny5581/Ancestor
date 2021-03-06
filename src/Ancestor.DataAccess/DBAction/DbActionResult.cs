﻿using Ancestor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DBAction
{
    public class DbActionResult
    {        
        public object Result { get; set; }        
        public QueryParameter Parameter { get; set; }        
    }
    public class DbActionResult<T> : DbActionResult
    {
        public new T Result
        {
            get { return (T)base.Result; }
            set { base.Result = value; }
        }
    }
}
