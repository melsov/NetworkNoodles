using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.Weapons
{
    

    public class WeaponEnable : MonoBehaviour
    {
        public Weapon weapon { get; private set; }

        public virtual bool isEnabled {
            get {
                return weapon.gameObject.activeSelf;
            }
            set {
                weapon.gameObject.SetActive(value);
            }
        }

        public bool available;

        protected virtual void Awake() {
            weapon = GetComponentInChildren<Weapon>(true);
        }
    }
}
