using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Ancestor Exception class 
    /// </summary>
    [Serializable]
    public class AncestorException : Exception
    {
        public int Code { get; private set; }        

        public AncestorException(int code, string message) : base(message)
        {
            Code = code;
        }
        public AncestorException(int code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        protected AncestorException(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("code", Code);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Code = info.GetInt32("code");
            base.GetObjectData(info, context);
        }
    }
}
