using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenResolution : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Switch to 640 x 480 full-screen
        Screen.SetResolution(640, 480, true);
    }

}
