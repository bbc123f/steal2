using Steal.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Steal.Background.Mods
{
    internal class Visual : Mod
    {
        #region Visual

        public static void ResetTexure()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>())
                {
                    Object.Destroy(vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>());
                }

                for (int i = 0; i < bones.Count(); i += 2)
                {
                    if (vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>())
                    {
                        Object.Destroy(vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>());
                    }
                }
            }

            foreach (VRRig vrrig in (VRRig[])UnityEngine.Object.FindObjectsOfType(typeof(VRRig)))
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    vrrig.ChangeMaterialLocal(vrrig.currentMatIndex);
                }
            }
        }

        public static void Beacons()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<BoxCollider>());
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<Rigidbody>());
                    UnityEngine.Object.Destroy(gameObject2.GetComponent<Collider>());
                    gameObject2.transform.rotation = Quaternion.identity;
                    gameObject2.transform.localScale = new Vector3(0.04f, 200f, 0.04f);
                    gameObject2.transform.position = vrrig.transform.position;
                    gameObject2.GetComponent<MeshRenderer>().material = vrrig.mainSkin.material;
                    UnityEngine.Object.Destroy(gameObject2, Time.deltaTime);
                }
            }
        }

        public static int[] bones =
        {
            4, 3, 5, 4, 19, 18, 20, 19, 3, 18, 21, 20, 22, 21, 25, 21, 29, 21, 31, 29, 27, 25, 24, 22, 6, 5, 7, 6, 10,
            6, 14, 6, 16, 14, 12, 10, 9, 7
        };

        public static void BoneESP()
        {
            Material material = new Material(Shader.Find("GUI/Text Shader"));
            material.color = new Color(1f, 1f, 1f);

            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig)
                {
                    if (vrrig.mainSkin.material.name.Contains("fected"))
                    {
                        material = new Material(Shader.Find("GUI/Text Shader"));
                        material.color = new Color(1f, 0f, 0f);
                    }
                    else
                    {
                        material = new Material(Shader.Find("GUI/Text Shader"));
                        material.color = Color.green;
                    }


                    LineRenderer a = vrrig.head.rigTarget.gameObject.GetComponent<LineRenderer>();
                    if (!a)
                    {
                        a = vrrig.head.rigTarget.gameObject.AddComponent<LineRenderer>();
                    }

                    a.endWidth = 0.03f;
                    a.startWidth = 0.03f;
                    a.material = material;
                    a.SetPosition(0, vrrig.head.rigTarget.transform.position + new Vector3(0, 0.160f, 0));
                    a.SetPosition(1, vrrig.head.rigTarget.transform.position - new Vector3(0, 0.4f, 0));



                    for (int i = 0; i < bones.Count(); i += 2)
                    {
                        LineRenderer r = vrrig.mainSkin.bones[bones[i]].gameObject.GetComponent<LineRenderer>();
                        if (!r)
                        {
                            r = vrrig.mainSkin.bones[bones[i]].gameObject.AddComponent<LineRenderer>();
                            r.endWidth = 0.03f;
                            r.startWidth = 0.03f;

                        }

                        r.material = material;
                        r.SetPosition(0, vrrig.mainSkin.bones[bones[i]].position);
                        r.SetPosition(1, vrrig.mainSkin.bones[bones[i + 1]].position);
                    }

                }
            }
        }

        public static void Tracers()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != null && !vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    var gameobject = new GameObject("Line");
                    LineRenderer lineRenderer = gameobject.AddComponent<LineRenderer>();
                    lineRenderer.startColor = Color.green;
                    lineRenderer.endColor = Color.green;
                    lineRenderer.startWidth = 0.01f;
                    lineRenderer.endWidth = 0.01f;
                    lineRenderer.positionCount = 2;
                    lineRenderer.useWorldSpace = true;
                    lineRenderer.SetPosition(0, GorillaLocomotion.Player.Instance.rightControllerTransform.position);
                    lineRenderer.SetPosition(1, vrrig.transform.position);
                    lineRenderer.material.shader = Shader.Find("GUI/Text Shader");
                    Object.Destroy(lineRenderer, Time.deltaTime);
                }
            }
        }

        public static void BoxESP()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null && !rig.isOfflineVRRig)
                {
                    GameObject go = new GameObject("box");
                    go.transform.position = rig.transform.position;
                    GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(top.GetComponent<BoxCollider>());
                    Destroy(bottom.GetComponent<BoxCollider>());
                    Destroy(left.GetComponent<BoxCollider>());
                    Destroy(right.GetComponent<BoxCollider>());
                    top.transform.SetParent(go.transform);
                    top.transform.localPosition = new Vector3(0f, 1f / 2f - 0.02f / 2f, 0f);
                    top.transform.localScale = new Vector3(1f, 0.02f, 0.02f);
                    bottom.transform.SetParent(go.transform);
                    bottom.transform.localPosition = new Vector3(0f, (0f - 1f) / 2f + 0.02f / 2f, 0f);
                    bottom.transform.localScale = new Vector3(1f, 0.02f, 0.02f);
                    left.transform.SetParent(go.transform);
                    left.transform.localPosition = new Vector3((0f - 1f) / 2f + 0.02f / 2f, 0f, 0f);
                    left.transform.localScale = new Vector3(0.02f, 1f, 0.02f);
                    right.transform.SetParent(go.transform);
                    right.transform.localPosition = new Vector3(1f / 2f - 0.02f / 2f, 0f, 0f);
                    right.transform.localScale = new Vector3(0.02f, 1f, 0.02f);

                    top.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    bottom.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    left.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    right.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");

                    Color Espcolor;

                    if (rig.mainSkin.material.name.Contains("fected"))
                    {
                        Espcolor = Color.red;
                    }
                    else
                    {
                        Espcolor = Color.green;
                    }

                    top.GetComponent<Renderer>().material.color = Espcolor;
                    bottom.GetComponent<Renderer>().material.color = Espcolor;
                    left.GetComponent<Renderer>().material.color = Espcolor;
                    right.GetComponent<Renderer>().material.color = Espcolor;

                    go.transform.LookAt(go.transform.position + Camera.main.transform.rotation * Vector3.forward,
                        Camera.main.transform.rotation * Vector3.up);
                    Object.Destroy(go, Time.deltaTime);
                }
            }
        }

        public static void Chams()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer && vrrig.mainSkin.material.name.Contains("fected"))
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = new Color32(255, 0, 0, 90);
                }
                else if (!vrrig.isOfflineVRRig && !vrrig.isMyPlayer)
                {
                    vrrig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
                    vrrig.mainSkin.material.color = new Color32(0, 255, 0, 90);
                }
            }
        }

        public static void ESP()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != null)
                {
                    if (!rig.isOfflineVRRig && !rig.isMyPlayer)
                    {
                        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        GameObject.Destroy(beacon.GetComponent<BoxCollider>());
                        GameObject.Destroy(beacon.GetComponent<Rigidbody>());
                        beacon.transform.rotation = rig.transform.rotation;
                        beacon.transform.localScale = new Vector3(0.4f, 0.86f, 0.4f);
                        beacon.transform.position = rig.transform.position;
                        beacon.GetComponent<MeshRenderer>().material = new Material(Shader.Find("GUI/Text Shader"));
                        if (rig.mainSkin.material.name.Contains("fected"))
                        {
                            beacon.GetComponent<MeshRenderer>().material.color = Color.red;
                        }
                        else
                        {
                            beacon.GetComponent<MeshRenderer>().material.color = Color.green;
                        }

                        Object.Destroy(beacon, Time.deltaTime);
                    }
                }
            }
        }

        #endregion
        public static void StartNameTags()
        {
            if (!Steal.Patchers.VRRigPatchers.OnEnable.nameTags)
            {
                Steal.Patchers.VRRigPatchers.OnEnable.nameTags = true;
                foreach (VRRig rig in GorillaParent.instance.vrrigs)
                {
                    if (rig.GetComponent<NameTags>() == null)
                    {
                        rig.gameObject.AddComponent<NameTags>();
                    }
                }
            }
        }

        public static void DisablePost()
        {
            GameObject post = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/SoundPostForest");
            post.SetActive(!post.activeSelf);
        }

        private static Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        public static void RestoreOriginalMaterials()
        {
            foreach (KeyValuePair<Renderer, Material> entry in originalMaterials)
            {
                try
                {
                    if (entry.Key != null)
                    {
                        entry.Key.material = entry.Value;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Restore error {exception.StackTrace} - {exception.Message}");
                }
            }


            originalMaterials.Clear();
        }

        public static void FPSBoost()
        {
            Shader gorillaTagUberShader = Shader.Find("GorillaTag/UberShader");
            if (gorillaTagUberShader == null)
            {
                Debug.LogError("GorillaTag/UberShader not found.");
                return;
            }

            Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            Material replacementTemplate = new Material(gorillaTagUberShader);

            foreach (Renderer renderer in renderers)
            {
                try
                {
                    if (renderer.material.shader == gorillaTagUberShader)
                    {
                        if (!originalMaterials.ContainsKey(renderer))
                        {
                            originalMaterials[renderer] = renderer.material;
                        }

                        Material replacement = new Material(replacementTemplate) { color = Color.grey };
                        renderer.material = replacement;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"mat error {exception.StackTrace} - {exception.Message}");
                }
            }

            Object.Destroy(replacementTemplate);
        }

        public static void HorrorGame()
        {
            Shader gorillaTagUberShader = Shader.Find("GorillaTag/UberShader");
            if (gorillaTagUberShader == null)
            {
                Debug.LogError("GorillaTag/UberShader not found.");
                return;
            }

            Renderer[] renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            Material replacementTemplate = new Material(gorillaTagUberShader);

            foreach (Renderer renderer in renderers)
            {
                try
                {
                    if (renderer.material.shader == gorillaTagUberShader)
                    {
                        if (!originalMaterials.ContainsKey(renderer))
                        {
                            originalMaterials[renderer] = renderer.material;
                        }

                        Material replacement = new Material(replacementTemplate) { color = Color.black };
                        renderer.material = replacement;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"mat error {exception.StackTrace} - {exception.Message}");
                }
            }

            Object.Destroy(replacementTemplate);
        }


        public static void agreeTOS()
        {
            GameObject.Find("Miscellaneous Scripts/LegalAgreementCheck/Legal Agreements")
                .GetComponent<LegalAgreements>().testFaceButtonPress = true;
        }

        public static void HideInTrees(bool enable)
        {
            GameObject parentGameObject =
                GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Terrain/SmallTrees");
            List<GameObject> allChildGameObjects = GetAllGameObjects(parentGameObject);


            foreach (GameObject gameObject in allChildGameObjects)
            {
                MeshCollider collider = gameObject.GetComponent<MeshCollider>();
                if (collider != null)
                {
                    if (enable)
                    {
                        collider.enabled = false;
                    }
                    else
                    {
                        collider.enabled = true;
                    }
                }
            }
        }
        public static List<GameObject> GetAllGameObjects(GameObject parent)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            CollectGameObjectsRecursive(parent, gameObjects);
            return gameObjects;
        }

        private static void CollectGameObjectsRecursive(GameObject parent, List<GameObject> gameObjects)
        {
            // Add the current parent GameObject to the list
            gameObjects.Add(parent);

            // Iterate through all children and call this method recursively
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                CollectGameObjectsRecursive(parent.transform.GetChild(i).gameObject, gameObjects);
            }
        }
        public static void StopNameTags()
        {
            if (Steal.Patchers.VRRigPatchers.OnEnable.nameTags)
            {
                Steal.Patchers.VRRigPatchers.OnEnable.nameTags = false;
                foreach (VRRig rig in GorillaParent.instance.vrrigs)
                {
                    if (rig.GetComponent<NameTags>() != null)
                    {
                        Destroy(rig.gameObject.GetComponent<NameTags>());
                    }
                }
            }
        }
        public static bool oldGraphiks = false;
        public static void OldGraphics()
        {
            if (!oldGraphiks)
            {
                foreach (Renderer renderer in Resources.FindObjectsOfTypeAll<Renderer>())
                {
                    if (renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
                    {
                        string objectName = renderer.sharedMaterial.mainTexture.name;
                        string materialName = renderer.sharedMaterial.name;
                        renderer.sharedMaterial.mainTexture.filterMode = FilterMode.Bilinear;


                        renderer.sharedMaterial.mainTexture.name = objectName;
                        renderer.sharedMaterial.name = materialName;
                    }
                }

                oldGraphiks = true;
            }
        }

        public static void RevertGraphics()
        {
            foreach (Renderer renderer in Resources.FindObjectsOfTypeAll<Renderer>())
            {
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
                {
                    // Assuming you want to revert the filter mode to Point for a more pixelated look
                    renderer.sharedMaterial.mainTexture.filterMode = FilterMode.Point;

                    // The objectName and materialName assignments are not needed for reversing the filter mode change
                    // but if there were other changes made to names or properties, you'd reverse those here
                }
            }

            oldGraphiks = false;
        }

    }
}
