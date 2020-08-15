using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool closed = true;
    public bool locked = false;
    public int lockId = 0;
    float marginAngle = 10;
    bool passedMarginAgnle;
    public float closeAngle = 1.5f;

    float initialYRotation;
    float angleDiff;
    #region MY COMPONENTS
    HingeJoint HingeJoint;
    Rigidbody myRigidBody;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        #region GET COMPONENTS
        HingeJoint = GetComponent<HingeJoint>();
        myRigidBody = GetComponent<Rigidbody>();
        #endregion
        initialYRotation = transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        #region CLOSING/OPENING;

        angleDiff = transform.rotation.eulerAngles.y - initialYRotation;
        float angleToMaxLimit = Mathf.Abs(Mathf.DeltaAngle(angleDiff, HingeJoint.limits.max));

        if (Mathf.Abs(HingeJoint.currentTorque.y) > 4 && angleToMaxLimit < closeAngle && closed == false && passedMarginAgnle == true) {
            Close();
            print("Closed");
        }

        if (angleToMaxLimit > marginAngle) 
        {
            passedMarginAgnle = true;
        }
        #endregion

        if (locked)
        {
            myRigidBody.isKinematic = true;
        }
        else
        {
            myRigidBody.isKinematic = false;
        }
    }
    public void Unlock()
    {
        locked = false;
    }

    public void Lock()
    {
        locked = true;
    }

    public void Open()
    {
        closed = false;
        passedMarginAgnle = false;
    }
    public void Close()
    {
        closed = true;
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player.grabbedObject == gameObject) {
            player.UnGrab();
        }
    }
    public void Jiggle()
    {
        print("jiggle");
    }

}
