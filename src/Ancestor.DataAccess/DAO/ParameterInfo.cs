using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    public class ParameterInfo
    {
        private readonly string _name;
        private readonly object _value;
        private readonly bool _isSysDateConverted;


        public ParameterInfo(string name, object value, bool isSysDateConverted)
        {
            _name = name;
            _value = value;
            _isSysDateConverted = isSysDateConverted;
        }

        public string ValueName
        {
            get { return _name; }
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
    }
}
