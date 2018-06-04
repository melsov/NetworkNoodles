using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MelStandardAssets.CrossPlatformInput;
using UnityEngine.UI;
using System.Collections;

namespace Mel.DDebug
{
    public struct CurMax
    {
        public float _current, max;

        public float current {
            get { return _current; }
            set {
                _current = value;
                if (Mathf.Abs( _current) > max) {
                    max = Mathf.Abs(_current);
                }
            }
        }

        public void reset() {
            _current = 0;
            max = 0;
        }
    }
    public class MouseInputDebugInfo : MonoBehaviour
    {
        [SerializeField]
        Text text;

        CurMax mouseX = new CurMax(), mouseY = new CurMax(), recentX = new CurMax(), recentY = new CurMax();

        private void Start() {
            if(!text) {
                text = GetComponent<Text>();
            }
            if(!text) {
                foreach(var _text in FindObjectsOfType<Text>()) {
                    if(_text.name.Equals("MouseInfo")) {
                        text = _text;
                    }
                }
            }
            StartCoroutine(updateCurrent());
        }

        private IEnumerator updateCurrent() {
            while(true) {
                yield return new WaitForSeconds(.1f);
                mouseX.current = recentX.max;
                mouseY.current = recentY.max;
                recentX.reset();
                recentY.reset();
            }
        }

        private void Update() {
            recentX.current = CrossPlatformInputManager.GetAxis("Mouse X");
            recentY.current = CrossPlatformInputManager.GetAxis("Mouse Y");

            text.text = string.Format("X: {0:+0.00;-0.00} max: {1}. Y: {2:+0.00;-0.00} max: {3}", mouseX.current, mouseX.max, mouseY.current, mouseY.max);
        }
    }
}
