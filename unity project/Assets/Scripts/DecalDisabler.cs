using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DecalSystem;

public class DecalDisabler : MonoBehaviour
{
    private bool hasDisabled;
    private int timer;
    void Update()
    {
        timer += 1;
        if (timer == 1)
        {
            foreach (Decal decal in GetComponents<Decal>())
            {
                decal.enabled = false;
            }
            enabled = false;
        }
    }
}
