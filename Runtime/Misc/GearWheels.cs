using System.Collections;
using System.Collections.Generic;
using com.mineorbit.dungeonsanddungeonscommon;
using UnityEngine;

public class GearWheels : MonoBehaviour
{
    public GameObject[] gearwheels;
    private float[] speeds = new float[4];
    private float t = 0;

    public float maxSpeed = 8f;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            speeds[i] = Random.Range(0f,maxSpeed);
            gearwheels[i].GetComponent<MeshRenderer>().sharedMaterial.color = Constants.ToColor((Constants.Color) i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        t += Time.deltaTime;
        for(int i = 0;i<4;i++)
            gearwheels[i].transform.Rotate(Vector3.up, Time.deltaTime * speeds[i]);
    }
}
