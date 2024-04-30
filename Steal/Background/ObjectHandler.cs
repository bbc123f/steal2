using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Background
{
    internal class ObjectHandler
    {
        static List<UnityEngine.Object> cachedObjects = new List<UnityEngine.Object>();

        static List<UnityEngine.Object[]> searchedObjects = new List<UnityEngine.Object[]>();

        static List<Component[]> components = new List<Component[]>();

        public static UnityEngine.Object[] GetObjectsOfType<T>()
        {
            var list = GameObject.FindObjectsOfType(typeof(T));
            searchedObjects.Add(list);
            return list;
        }

    }
}
