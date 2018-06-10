using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

/*
 * DUKS notes: hard mode duk. turns on hard mode
 * another mode: underwater. 
 * bolders move slower (in any mode)
 */

namespace Mel.Item
{
    public class PickupSpawner : NetworkBehaviour
    {
        [SerializeField]
        Pickup pickup;

        Pickup spawnedPickup;

        [SerializeField]
        private float respawnTime = 2f;

        [SyncVar(hook = "OnAvailableChanged"), HideInInspector]
        public bool available;

        PickupTrigger pkTrigger;

        private void Start() {
            pkTrigger = GetComponentInChildren<PickupTrigger>();
            Spawn();
        }

        void Spawn() { 
            available = true;
        }


        public void OnPickupTriggerEnter(Collider coll) {
            var mp = coll.GetComponent<MPlayerController>();
            if (mp) {

                //CONSIDER: we added this method on the assumption that 
                //only components attached to the player can issue commands
                mp.handlePickup(pickup, this);

            }
        }

       
        public void Give() {
            available = false;
            StartCoroutine(WaitThenRespawn());
        }

        //
        //SyncVar callback
        //
        void OnAvailableChanged(bool _available) {
            pkTrigger.beVisible(_available);
        }

        private IEnumerator WaitThenRespawn() {
            yield return new WaitForSeconds(respawnTime);
            Spawn();
        }
    }
}
