using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WristMenu.Components
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
    }
}
