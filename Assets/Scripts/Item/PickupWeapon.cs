using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mel.Weapons;

namespace Mel.Item
{
    public class PickupWeapon : Pickup
    {
        [SerializeField]
        int weaponIndex;

        public override void getGiven(MPlayerController mp) {
            Debug.Log("weapon get given");
            mp.acquireWeapon(weaponIndex);
        }
    }
}
