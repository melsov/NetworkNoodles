using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Mel.Item
{
    [RequireComponent(typeof(Collider))]
    public class PickupTrigger : MonoBehaviour
    {

        PickupSpawner spawner;

        private void Start() {
            spawner = GetComponentInParent<PickupSpawner>();
        }

        private void OnTriggerEnter(Collider other) {
            spawner.OnPickupTriggerEnter(other);
        }

        public void beVisible(bool isVisible) {
            GetComponent<Renderer>().enabled = isVisible;
            GetComponent<Collider>().enabled = isVisible;
        }
    }
}
