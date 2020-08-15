using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaManager : MonoBehaviour
{
    public static MetaManager  instance;
    public static List<GameObject> playerInventory = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
