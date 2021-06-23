using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



namespace com.mineorbit.dungeonsanddungeonscommon
{
    
    
    public class GameConsole : MonoBehaviour
    {
        public static GameConsole instance;
        public UnityEngine.Object logLine;
        public Transform content;
        public CanvasGroup canvasGroup;

        public int maxViewable = 500;
        void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
            }

            instance = this;
        }

        public static void Log(string text)
        {
            if(instance != null)
            instance.CreateLine("> "+text);
            
            Debug.Log(text);
        }

        private Queue<GameObject> q = new Queue<GameObject>();

        void CreateLine(string line)
        {
            var instantiate = Instantiate(logLine,content) as GameObject;
            instantiate.GetComponentInChildren<TextMeshProUGUI>().text = line;
            q.Enqueue(instantiate);
            if (q.Count > maxViewable)
            {
                Destroy(q.Dequeue());
            }
        }

        private bool open = false;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                if (open)
                {
                    canvasGroup.alpha = 0;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    open = false;
                }
                else
                {
                    canvasGroup.alpha = 1;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    open = true;
                }
            }
        }
    }
}