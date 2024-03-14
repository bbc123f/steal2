using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Attributes
{
    internal static class GameObjectEx
    {
        public static GameObject ToObject(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return (GameObject)obj;
        }
    }
}
