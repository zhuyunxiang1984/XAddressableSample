using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XDebugSample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        XDebug.Log("GGYY", "Hello World");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            XDebug.Log("GGYY", (mutiText)=>
            {
                mutiText.AppendLine("lalala");
                mutiText.AppendLine("lalala");
                mutiText.AppendLine("lalala");
            });
        }
    }
}
