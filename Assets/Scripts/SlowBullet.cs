using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SlowBullet : Bullet
{
    [SerializeField] float speed;

    protected override IEnumerator flashAndDie(Vector3 dir) {
        var rb = GetComponent<Rigidbody>();

        rb.velocity = rb.velocity.normalized * speed;

        yield return new WaitForSeconds(displayTime);

        Destroy(gameObject);

    }


}
