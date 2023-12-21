using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ancestor.DataAccess.DAO
{
    /// <summary>
    /// DBNull value for model
    /// </summary>
    public class DBNullValue : IEquatable<string>, IEquatable<short>, IEquatable<int>, IEquatable<long>, IEquatable<decimal>, IEquatable<float>, IEquatable<double>, IEquatable<DateTime>
    {
        public static readonly DBNullValue Null
            = new DBNullValue();

        #region string
        public bool Equals(string other)
        {
            return other == Null;
        }


        public static implicit operator string(DBNullValue v) => "";
        //public static explicit operator DBNullValue(string v) => Null;
        #endregion string

        #region short
        public bool Equals(short other)
        {
            return other == Null;
        }
        public static implicit operator short(DBNullValue v) => short.MinValue + 1;
        //public static explicit operator DBNullValue(short v) => Null;
        #endregion short

        #region int
        public bool Equals(int other)
        {
            return other == Null;
        }
        public static implicit operator int(DBNullValue v) => int.MinValue + 1;
        //public static explicit operator DBNullValue(int v) => Null;
        #endregion int

        #region long
        public bool Equals(long other)
        {
            return other == Null;
        }
        public static implicit operator long(DBNullValue v) => long.MinValue + 1;
        //public static explicit operator DBNullValue(long v) => Null;
        #endregion long

        #region decimal
        public bool Equals(decimal other)
        {
            return other == Null;
        }
        public static implicit operator decimal(DBNullValue v) => decimal.MinValue + 1;
        //public static explicit operator DBNullValue(decimal v) => Null;
        #endregion decimal

        #region float
        public bool Equals(float other)
        {
            return other == Null;
        }
        public static implicit operator float(DBNullValue v) => float.MinValue + 1;
        //public static explicit operator DBNullValue(float v) => Null;
        #endregion float

        #region double
        public bool Equals(double other)
        {
            return other == Null;
        }
        public static implicit operator double(DBNullValue v) => double.MinValue + 1;
        //public static explicit operator DBNullValue(double v) => Null;
        #endregion double

        #region DateTime
        public bool Equals(DateTime other)
        {
            return other == Null;
        }
        public static implicit operator DateTime(DBNullValue v) => DateTime.MinValue.AddTicks(99);
        //public static explicit operator DBNullValue(DateTime v) => Null;
        #endregion double
    }
}
