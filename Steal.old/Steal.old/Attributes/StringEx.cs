using System;

namespace Steal.Attributes
{
    public static class StringEx
    {
        public static bool TryReplace(this string orgstring, string replaced, string replace, out string result)
        {
            result = orgstring;
            try
            {
                if (string.IsNullOrEmpty(replaced)) { return false; }
                if (string.IsNullOrEmpty(orgstring)) { return false; }
                result = orgstring.Replace(replaced, replace);

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
        }

        public static string Remove(this string orgstring, string removed)
        {
            try
            {
                if (string.IsNullOrEmpty(removed)) { return orgstring; }
                if (string.IsNullOrEmpty(orgstring)) {  return orgstring; }
                if (!orgstring.Contains(removed))
                {
                    return orgstring;
                }
                if (orgstring.TryReplace(removed, "", out string result)) 
                { 
                    return result;
                }
                else
                {
                    return orgstring;
                }
            }
            catch (Exception ex)
            {
                return orgstring;
                throw;
            }
        }

        public static bool AddIfNot(this string orgstring, string add, out string result)
        {
            result=orgstring;
            try
            {
                if(orgstring.Contains(add)) { return false; }
                result = add + orgstring;
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
        }
    }
}
