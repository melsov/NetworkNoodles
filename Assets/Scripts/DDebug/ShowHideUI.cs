using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



public class ShowHideUI : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey;

    [SerializeField] RectTransform target;

    bool en;

    Dictionary<CanvasRenderer, float> alphas = new Dictionary<CanvasRenderer, float>();

    void addIfNot(CanvasRenderer cr) {
        if (!alphas.ContainsKey(cr)) {
            alphas.Add(cr, cr.GetAlpha());
        }
    }

    private void Start() {
        if(!target) {
            target = GetComponent<RectTransform>();
        }
        foreach(var cr in target.GetComponentsInChildren<CanvasRenderer>()) {
            addIfNot(cr);
        }
    }

    private void Update() {
        if(Input.GetKeyDown(toggleKey)) {
            toggle();
        }
    }

    private void toggle() {
        en = !en;
        foreach(var cr in alphas.Keys) {
            cr.SetAlpha(en ? alphas[cr] : 0f);
        }
    }
}
