using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CracklingLamp : MonoBehaviour
{
    [SerializeField] float minCrackleDelay;
    [SerializeField] float maxCrackleDelay;
    [SerializeField] float minCrackleTime;
    [SerializeField] float maxCrackleTime;
    float timeTillNextCrackle;
    float crackleTime;

    #region LIGHT
    Light myLight;
    GameObject lightCube;
    float initialIntensity;
    #endregion

    void Start()
    {
        myLight = GetComponent<Light>();
        initialIntensity = myLight.intensity;
        lightCube = transform.parent.Find("LampCube").gameObject;
    }

    void Update()
    {
        if (myLight.intensity != 0)
        {
            timeTillNextCrackle -= Time.deltaTime;
            if (timeTillNextCrackle <= 0)
            {
                timeTillNextCrackle = minCrackleDelay + (maxCrackleDelay - minCrackleDelay) * Random.value;
                crackleTime = minCrackleTime + (maxCrackleTime - minCrackleTime) * Random.value;
                myLight.intensity = 0f;
                lightCube.SetActive(false);
            }
        }
        else 
        {
            crackleTime -= Time.deltaTime;
            if(crackleTime <= 0)
            {
                myLight.intensity = initialIntensity;
                lightCube.SetActive(true);
            }
        }
    }
}
