using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.pdynet.odbcproxy
{
    public static class StringUtils
    {
        public static bool IsBlank(string value)
        {
            if (String.IsNullOrEmpty(value))
                return true;

            int len = value.Length;
            for (int i = 0; i < len; i++)
            {
                if (!Char.IsWhiteSpace(value, i))
                    return false;
            }

            return true;
        }
    }
}
