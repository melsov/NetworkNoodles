using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Bobbing : MonoBehaviour
{
    [SerializeField] float bob = 2f;
    [SerializeField] float period = 3f;

    float baseY;
    float ang;
    private void Awake() {
        baseY = transform.position.y;
    }
    private void FixedUpdate() {
        ang += Mathf.PI * 2 * Time.deltaTime / period;
        var p = transform.position;
        p.y = baseY + Mathf.Sin(ang);
        transform.position = p;
    }
}
