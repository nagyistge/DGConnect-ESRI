using System;
using System.Collections.Generic;

namespace DataInterop
{
    public class DoubleTenDecimalPlaceComparer : IEqualityComparer<double>
    {
        public bool Equals(double x, double y)
        {
            return Math.Abs(x - y) < 0.0000000001;
        }

        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }
}