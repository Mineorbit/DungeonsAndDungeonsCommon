using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Instantiable : ScriptableObject
    {
        public UnityEngine.Object prefab;


        public virtual GameObject Create(Vector3 location, Quaternion rotation, Transform parent)
        {
            GameObject g = Instantiate(prefab) as GameObject;
            g.transform.SetParent(parent);
            g.transform.position = location;
            g.transform.rotation = rotation;
            return g;
        }

        //For UI
        public virtual GameObject Create(Vector2 location, Transform parent)
        {
            GameObject g = Instantiate(prefab) as GameObject;
            g.SetActive(true);
            g.transform.SetParent(parent);
            RectTransform rt = g.GetComponent<RectTransform>();
            rt.offsetMax = location;
            rt.offsetMin = location;
            return g;
        }
    }
}
