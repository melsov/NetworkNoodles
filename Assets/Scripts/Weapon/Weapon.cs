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
        public bool canAim;
        public float aimFOV; 

        public static AimSettings DefaultAimSettings() {
            return new AimSettings();
        }
    }

    public class Weapon : MonoBehaviour
    {
        public int damage = 10;
        
        public AimSettings aimSettings;

        [SerializeField] public AudioSource fireAudio;

        [SerializeField] public AudioSource reloadAudio;

        [SerializeField] Bullet _bulletPrefab;
        public Bullet bulletPrefab {
            get {
                if(!_bulletPrefab) {
                    _bulletPrefab = Resources.Load<Bullet>("Prefab/Bullet");
                }
                return _bulletPrefab;
            }
        }

        private void Start() {
            if(!fireAudio)
                fireAudio = transform.Find("FireAudio").GetComponent<AudioSource>();
            if(!reloadAudio)
                reloadAudio = transform.Find("ReloadAudio").GetComponent<AudioSource>();
        }



        public void playFire() {
            fireAudio.Play();
        }

        public void playReload() {
            reloadAudio.Play();
        }


    }
}
