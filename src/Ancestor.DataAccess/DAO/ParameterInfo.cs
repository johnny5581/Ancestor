using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class ParameterInfo
    {
        private readonly string _valueName;
        private readonly object _value;
        private readonly bool _isSysDateConverted;
        private readonly Core.HardWordAttribute _hardword;
        private readonly string _paraName;

        public ParameterInfo(string paraName, string valueName, object value, bool isSysDateConverted, Core.HardWordAttribute hardword)
        {
            _paraName = paraName;
            _valueName = valueName;
            _value = value;
            _isSysDateConverted = isSysDateConverted;
            _hardword = hardword;
        }

        public string ValueName
        {
            get { return _valueName ?? ParameterName; }
        }
        public object Value
        {
            get
            {
                if (_isSysDateConverted)
                    return null;
                return _value;
            }
        }

        public bool IsSysDateConverted
        {
            get { return _isSysDateConverted; }
        }

        public string ParameterName
        {
            get { return _paraName; }
        }

        public bool IsHardword
        {
            get { return _hardword != null; }
        }
        public Core.HardWordAttribute Hardword
        {
            get { return _hardword; }
        }
    }
}
