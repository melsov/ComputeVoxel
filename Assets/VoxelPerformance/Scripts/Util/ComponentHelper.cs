using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mel.Util
{
    public static class ComponentHelper 
    {
        public static T GetOrAddComponent<T>(this MonoBehaviour mb) where T : Component
        {
            T result = mb.transform.GetComponent<T>();
            if(!result)
            {
                result = mb.gameObject.AddComponent<T>();
            }
            return result;
        }

        public static T GetInChildrenOrAddComponent<T>(this MonoBehaviour mb) where T : Component
        {
            T result = mb.transform.GetComponentInChildren<T>();
            if (!result)
            {
                result = mb.gameObject.AddComponent<T>();
            }
            return result;
        }

        public static void DestroyAllChildrenImmediate(this Transform t)
        {
            Transform[] children = t.GetComponentsInChildren<Transform>();
            for(int i=0; i<children.Length; ++i) {
                if(children[i] == t) { continue; }
                children[i] = SafeDestroyGameObject(children[i]);
            }
        }

        public static T SafeDestroy<T>(T obj) where T : Object
        {
            if (Application.isEditor)
                Object.DestroyImmediate(obj);
            else
                Object.Destroy(obj);

            return null;
        }
        public static T SafeDestroyGameObject<T>(T component) where T : Component
        {
            if (component != null)
                SafeDestroy(component.gameObject);
            return null;
        }

        public static T FindOrCreateObjectOfType<T>() where T : Component
        {
            T result = GameObject.FindObjectOfType<T>();
            if(!result)
            {
                GameObject go = new GameObject();
                result = go.AddComponent<T>();
            }
            return result;
        }



    }
}
