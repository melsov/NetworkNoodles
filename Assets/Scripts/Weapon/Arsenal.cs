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

        public struct SVWeaponData
        {
            public bool available;
            public bool equiped;
            public int arsenalIndex;

            public SVWeaponData CloneEquiped(bool _equiped) {
                return new SVWeaponData()
                {
                    available = _equiped ? true : available,
                    equiped = _equiped,
                    arsenalIndex = arsenalIndex
                };
            }

            public SVWeaponData CloneAvailable(bool _available) {
                return new SVWeaponData()
                {
                    available = _available,
                    equiped = equiped,
                    arsenalIndex = arsenalIndex
                };
            }

            public SVWeaponData CloneLoseWeapon() {
                return new SVWeaponData()
                {
                    available = false,
                    equiped = false,
                    arsenalIndex = arsenalIndex
                };
            }
        }

        public class SVLookup : SyncListStruct<SVWeaponData> { }
        SVLookup _svLookup = new SVLookup();

        [Command]
        void CmdInitSVLookup() {
            for(int i = 0; i < weapons.Count; ++i) {
                _svLookup.Add(new SVWeaponData()
                {
                    available  = weapons[i].available,
                    equiped = false,
                    arsenalIndex = i
                });
            }
            _svLookup.Callback = OnSVLookupChanged;
        }

        //
        // _svLookup callback
        //
        void OnSVLookupChanged(SVLookup.Operation op, int index) {
            // CONSIDER: correct way to propagate the change?
            if(isServer) {
                RpcChangeWeaponLocal(index);
            } else {
                ChangeWeaponLocal(index);
            }
        }

        [ClientRpc]
        void RpcChangeWeaponLocal(int index) {
            ChangeWeaponLocal(index);
        }

        void ChangeWeaponLocal(int index) {
            weapons[_svLookup[index].arsenalIndex].isEnabled = _svLookup[index].equiped;
            isArmed = equipedSVIndex >= 0;
            GetComponent<MPlayerController>().ClientOnSwitchedWeapon(index);
        }

        public int equipedSVIndex {
            get {
                for(int i=0;i<_svLookup.Count; ++i) {
                    if(_svLookup[i].equiped) { return i; }
                }
                return -1;
            }
        }

        public bool isArmed { get; private set; }

        [SerializeField, Header("<0 means start w/o a weapon")]
        int defaultWeaponIndex = -1;

        public Weapon equipedWeapon {
            get {
                if(!isArmed) { return null;}
                return weapons[equipedSVIndex].weapon;
            }
        }

        private void Start() {
            EnableAllLocal(false);
        }

        public override void OnStartLocalPlayer() {
        }

        public void Setup() {
            CmdInitSVLookup();
        }

        public void Equip(int wIndex) {
            CmdEquip(wIndex);
        }

        [Command]
        void CmdEquip(int wIndex) {
            DebugHUD.Debugg("cmd equip: " + wIndex);
            for(int i = 0; i < _svLookup.Count; ++i) {
                _svLookup[i] = _svLookup[i].CloneEquiped(i == wIndex);
                _svLookup.Dirty(i);
            }
           
        }

        public void SetAvailable(int wIndex, bool isAvailable) {
            CmdSetAvailable(wIndex, isAvailable);
        }

        [Command]
        void CmdSetAvailable(int wIndex, bool isAvailable) {
            bool shouldAutoEquip = numAvailable == 0 && isAvailable;
            if(shouldAutoEquip) {
                _svLookup[wIndex] = _svLookup[wIndex].CloneEquiped(true);
            } else {
                _svLookup[wIndex] = _svLookup[wIndex].CloneAvailable(isAvailable);
            }
            _svLookup.Dirty(wIndex); // NEED?
        }

        public void loseAll() {
            CmdLoseAll();
        }

        [Command]
        void CmdLoseAll() {
            for (int i = 0; i < _svLookup.Count; ++i) {
                _svLookup[i] = _svLookup[i].CloneLoseWeapon();
                _svLookup.Dirty(i);
            }
        }

        void EnableAllLocal(bool enable, bool available = false) {
            foreach(var we in weapons) {
                we.isEnabled = enable;
                we.available = available;
            }
        }


        public void nextWeapon() {
            int next;
            int _equipedIndex = equipedSVIndex;
            if(_equipedIndex < 0) {
                _equipedIndex = _svLookup.Count;
            }
            for(int i = 1;  i < _svLookup.Count; ++i) {
                next = (_equipedIndex + i) % _svLookup.Count;
                if(_svLookup[next].available) {
                    CmdEquip(next);
                    break;
                }
            }
        }

        int numAvailable {
            get {
                int i = 0;
                foreach (var wd in _svLookup) {
                    if (wd.available) i++;
                }
                return i;
            }
        }
    }
}
