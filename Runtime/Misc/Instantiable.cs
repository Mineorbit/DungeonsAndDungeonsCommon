using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Instantiable : ScriptableObject
    {
        public Object prefab;

        public virtual GameObject Create(Vector3 location, Transform parent)
        {
            return Create(location, new Quaternion(0, 0, 0, 0), parent);
        }

        public virtual GameObject Create(Vector3 location, Quaternion rotation, Transform parent)
        {
            var g = Instantiate(prefab) as GameObject;
            g.transform.SetParent(parent);
            g.transform.position = location;
            g.transform.rotation = rotation;
            return g;
        }

        // For UI
        public virtual GameObject Create(Vector2 location, Transform parent)
        {
            var g = Instantiate(prefab) as GameObject;
            g.SetActive(true);
            g.transform.SetParent(parent);
            var rt = g.GetComponent<RectTransform>();
            rt.offsetMax = location;
            rt.offsetMin = location;
            return g;
        }
    }
}