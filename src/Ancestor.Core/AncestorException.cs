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
        public static readonly int CodeUniqueConstraintDuplicated = 13001;

        private static readonly int[] KnownCodes
            = typeof(AncestorException).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static)
            .Select(field => field.GetValue(null)).Cast<int>().ToArray();

        public int Code
        {
            get
            {
                return Data.Contains("code")
                    ? (int)Data["code"] : 0;
            }
            set
            {
                if (Data.Contains("code"))
                    Data["code"] = value;
                else
                    Data.Add("code", value);
            }
        }

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

        }

        public bool IsUnknwonCode
        {
            get { return !KnownCodes.Contains(Code); }
        }


    }
}
