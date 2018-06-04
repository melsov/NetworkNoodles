using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Mel.Item
{
    public class PickupSpawner : NetworkBehaviour
    {
        [SerializeField]
        Pickup pickup;

        [SerializeField]
        Transform spawnLocation;
        [SerializeField]
        private float respawnTime = 3f;

        private void Start() {
            if(!spawnLocation) {
                spawnLocation = transform;
            }
            Spawn();
        }


        void Spawn() {
            if(!isServer) { return; }

            var next = Instantiate<Pickup>(pickup);
            next.subscribe(OnPickedCallback);
            next.transform.position = spawnLocation.position;

            NetworkServer.Spawn(next.gameObject);
        }

        public void OnPickedCallback(Pickup p) {
            StartCoroutine(WaitThenRespawn());
        }

        private IEnumerator WaitThenRespawn() {
            yield return new WaitForSeconds(respawnTime);
            Spawn();
        }
    }
}
