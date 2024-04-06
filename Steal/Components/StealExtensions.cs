using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Components
{
    internal static class StealExtensions
    {
        public static bool TryGetComponentInParent(this Collider component, Type tosearc, out Component go)
        {
            go = null;
            try
            {
                go = component.GetComponentInParent(tosearc.GetType());
                return true;
            }
            catch { return false; }
        }

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
                if (string.IsNullOrEmpty(orgstring)) { return orgstring; }
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
            result = orgstring;
            try
            {
                if (orgstring.Contains(add)) { return false; }
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
