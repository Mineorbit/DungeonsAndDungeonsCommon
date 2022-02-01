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
        public TextMeshProUGUI infoText;
        public int maxViewable = 500;
        void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
            }

            instance = this;
        }

        public static void Log(string text, bool inView = true, string channel = "default")
        {
            string channelName = $"[{channel}]";

            string output = channelName + " " + text;
            
            if(instance != null  && inView)
                instance.CreateLine("> "+output);
            
            Debug.Log(output);
        }

        private Queue<GameObject> q = new Queue<GameObject>();

        void CreateLine(string line)
        {
        MainCaller.Do(()=> {
            var instantiate = Instantiate(logLine,content) as GameObject;
            instantiate.GetComponentInChildren<TextMeshProUGUI>().text = line;
            q.Enqueue(instantiate);
            if (q.Count > maxViewable)
            {
                Destroy(q.Dequeue());
            }
            
        });
        }

        void UpdateText()
        {
            string fps = $"FPS: {(1 / Time.deltaTime)}\n";
            string appendage = $"Received Packets: {Client.receivedPacketCarriers}\n" +
                               $"Handled Packets: {Client.handledPackets}\n" +
                               $"Sent Packets: {Client.sentPacketCarriers} \n";
            if(NetworkManager.instance.client != null)
            {
            appendage += "UDP Queue Length: {NetworkManager.instance.client.packetOutUDPBuffer.Count} \n" +
                            $"TCP Queue Length: {NetworkManager.instance.client.packetOutTCPBuffer.Count}";
            }
            infoText.text = fps + appendage;
        }

        private bool open = false;

        public static void ToggleConsole()
        {
            instance.Toggle();
        }

        public void Toggle()
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
        
        void Update()
        {
            UpdateText();
            
        }
    }
}
