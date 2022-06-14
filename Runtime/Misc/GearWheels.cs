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
            gearwheels[i].GetComponentsInChildren<MeshRenderer>()[0].materials[0].color = Constants.ToColor((Constants.Color) i);
            gearwheels[i].GetComponentsInChildren<MeshRenderer>()[1].materials[0].color = Constants.ToColor((Constants.Color) i);
            gearwheels[i].SetActive(false);
            
        }
    }

    public void SetEffect(int i,bool a)
    {
        gearwheels[i].SetActive(a);
    }
    
    // Update is called once per frame
    void Update()
    {
        
        t += Time.deltaTime;
        for (int i = 0; i < 4; i++)
        {
            gearwheels[i].transform.localRotation = Quaternion.Euler((Vector3.up+Vector3.forward+Vector3.right)* t * speeds[i]*2);
            gearwheels[i].GetComponentsInChildren<MeshRenderer>()[0].materials[0].SetFloat(Shader.PropertyToID("Rotation"),t*speeds[i]*0.05f);
            gearwheels[i].GetComponentsInChildren<MeshRenderer>()[1].materials[0].SetFloat(Shader.PropertyToID("Rotation"),t*speeds[i]*0.05f);
        }
    }
}
