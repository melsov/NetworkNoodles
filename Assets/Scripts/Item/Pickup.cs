using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System;
using System.Collections;

namespace Mel.Item
{
    [System.Serializable]
    public class PickupEvent : UnityEvent<Pickup> { }

    [RequireComponent(typeof(Collider))]
    public abstract class Pickup : MonoBehaviour
    {
        [SerializeField]
        PickupEvent OnWasPickedUp;

        [SerializeField]
        float unavailableOnCreationForSeconds = 1f;
        bool available;

        private void Start() {
            StartCoroutine(waitThenBecomeAvailable());
        }

        private IEnumerator waitThenBecomeAvailable() {
            yield return new WaitForSeconds(unavailableOnCreationForSeconds);
            available = true;
        }

        public void subscribe(UnityAction<Pickup> pue) {
            OnWasPickedUp.AddListener(pue);
        }

        //private void OnTriggerEnter(Collider other) {
        //    //if(!available) { return; }
        //    MPlayerController mp = other.GetComponent<MPlayerController>();
        //    if (mp) {
        //        GetComponentInParent<PickupSpawner>().OnPlayerPickedup(mp, this);
        //    }
        //    //Destroy(gameObject);
        //}

        public abstract void getGiven(MPlayerController mp);

    }
}
