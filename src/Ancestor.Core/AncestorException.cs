using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ancestor.Core
{
    [Serializable]
    internal class AncestorException : Exception
    {
        
        public AncestorException() : base()
        {
        
        }

        public Exception Exception { get; set; }
        public string CommandText { get; set; }
        public DBParameterCollection Parameters { get; set; }        
        public object Options { get; set; }
    }
}
