using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Chunk : MonoBehaviour
    {
        public long chunkId;
        private readonly float eps = 0.05f;

        private void Start()
        {
            gameObject.name = "Chunk " + chunkId;
        }

        public LevelObject GetLevelObjectAt(Vector3 position)
        {
            foreach (Transform child in transform)
                if ((child.transform.position - position).magnitude < eps)
                    return child.gameObject.GetComponent<LevelObject>();
            return null;
        }
    }
}