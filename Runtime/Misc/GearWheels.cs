using System.Collections;
using System.Collections.Generic;
using com.mineorbit.dungeonsanddungeonscommon;
using UnityEngine;

public class GearWheels : MonoBehaviour
{
    public GameObject[] gearwheels;
    private float[] speeds = new float[4];
    private float t = 0;

    public float maxSpeed = 10f;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            speeds[i] = Random.Range(0f,maxSpeed);
           // gearwheels[i].GetComponentInChildren<MeshRenderer>().materials[0].SetColor("_color",Constants.ToColor((Constants.Color) i));
            gearwheels[i].GetComponentInChildren<MeshRenderer>().materials[0].color = Constants.ToColor((Constants.Color) i);
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        t += Time.deltaTime;
        for (int i = 0; i < 4; i++)
        {
            gearwheels[i].transform.Rotate(Vector3.up, Time.deltaTime * speeds[i]);
            gearwheels[i].GetComponentInChildren<MeshRenderer>().materials[0].SetFloat(Shader.PropertyToID("Rotation"),t*speeds[i]);
        }
    }
}
