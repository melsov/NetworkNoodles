using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.Weapons
{
    [Serializable]
    public struct AimSettings
    {
        public float aimFOV; 
    }

    public class Weapon : MonoBehaviour
    {
        public int damage = 10;
        
        public AimSettings aimSettings;

    }
}
