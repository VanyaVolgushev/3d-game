using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorFalling : MonoBehaviour
{
    [SerializeField] Vector3 centerOfMass;
    [SerializeField] bool fallDown;
    void Start()
    {
        
    }

    void Update()
    {
        if (fallDown) {
            FallDown();
        }
    }
    public void FallDown() {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
    }
}
