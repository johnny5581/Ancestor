using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Ancestor.Core
{
    /// <summary>
    /// Encoding attribute
    /// </summary>
    public class HardWordAttribute : Attribute
    {
        /// <summary>
        /// Global scope encoding
        /// </summary>
        public static Encoding DefaultEncoding
        {
            get { return HardWordManager.Encoding; }
            set { HardWordManager.Encoding = value; }
        }

        private Encoding _encoding;

        public HardWordAttribute()
        {
        }
        public HardWordAttribute(int codepage)
        {
            CodePage = codepage;
        }
        public HardWordAttribute(string codename)
        {
            CodeName = codename;
        }
        /// <summary>
        /// Encoding CodePage
        /// </summary>
        public int CodePage
        {
            get { return Encoding.CodePage; }
            set { _encoding = Encoding.GetEncoding(value); }
        }
        /// <summary>
        /// Encoding CodeName
        /// </summary>
        public string CodeName
        {
            get { return Encoding.EncodingName; }
            set { _encoding = Encoding.GetEncoding(value); }
        }
        /// <summary>
        /// Current Encoding
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding ?? DefaultEncoding; }
        }
    }
}
