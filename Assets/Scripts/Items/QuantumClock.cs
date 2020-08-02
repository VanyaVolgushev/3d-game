using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumClock : MonoBehaviour
{
    bool visible;
    float clock;
    #region VISIBILITY
    private void OnBecameInvisible()
    {
        visible = false;
    }
    private void OnBecameVisible()
    {
        visible = true;
    }
    #endregion

    void Update()
    {
        if (visible)
        {
            if (clock > 1) {
                AudioManager.instance.PlaySound("tick");
                clock -= 1;
            }

            clock += Time.deltaTime;
        }

        if(!visible)
        {
            
            
            
        }
    }
}
