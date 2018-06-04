using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.Weapons
{
    public class Arsenal : NetworkBehaviour
    {

        [SerializeField] Transform weaponParent;

        List<WeaponEnable> _weapons;
        List<WeaponEnable> weapons {
            get {
                if(_weapons == null) {
                    _weapons = new List<WeaponEnable>(weaponParent.GetComponentsInChildren<WeaponEnable>());
                }
                return _weapons;
            }
        }

        [SyncVar(hook ="OnEquipedIndexChanged")]
        int _equipedIndex;

        public int equipedIndex { get { return _equipedIndex; } }

        public bool isArmed {
            get {
                if(_equipedIndex < 0 || _equipedIndex >= weapons.Count) { return false; }
                return count > 0 && weapons[_equipedIndex].available;
            }
        }

        [SerializeField, Header("<0 means start w/o a weapon")]
        int defaultWeaponIndex = -1;

        public int count { get; private set; }

        public Weapon equipedWeapon {
            get {
                if(!isArmed) { return null; }
                return weapons[_equipedIndex].weapon;
            }
        }

        private void Start() {
            SetWeaponLocal(defaultWeaponIndex);
        }


        public void Equip(int wIndex) {
            CmdEquip(wIndex);
        }

        [Command]
        void CmdEquip(int wIndex) {
            _equipedIndex = wIndex;
        }

        //
        // equipedIndex syncvar hook
        //
        void OnEquipedIndexChanged(int wIndex) {
            if (isServer) {
                RpcEnable(wIndex);
            }
        }

        [ClientRpc]
        void RpcEnable(int wIndex) {
            SetWeaponLocal(wIndex);
            GetComponent<MPlayerController>().ClientOnSwitchedWeapon(wIndex);
        }

        void SetWeaponLocal(int wIndex) {
            for(int i = 0; i< weapons.Count; ++i) {
                weapons[i].enabledd = i == wIndex;
            }
        }

        public void setAvailable(int wIndex, bool isAvailable) {
            weapons[wIndex].available = isAvailable;
            count = numAvailable;
        }

        public void loseAll() {
            foreach(var weap in weapons) {
                weap.available = false;
            }
            CmdEquip(-1);
        }

        internal void nextWeapon() {
            int next;
            for(int i = 1;  i < weapons.Count; ++i) {
                next = (_equipedIndex + i) % weapons.Count;
                if(weapons[next].available) {
                    CmdEquip(next);
                    break;
                }
            }
        }

        int numAvailable {
            get {
                int i = 0;
                foreach (var wen in weapons) {
                    if (wen.available) i++;
                }
                return i;
            }
        }
    }
}
