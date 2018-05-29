using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class Billboard : MonoBehaviour
{
    private void Update() {
        transform.LookAt(Camera.main.transform);
    }
}
