using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour
{

    public RectTransform healthBar;

    public const int maxHealth = 100;

    [SyncVar(hook ="OnChangeHealth")]
    public int currentHealth = maxHealth;

    [HideInInspector]
    public bool Invulnerable;

    public void TakeDamage(MPlayerController.DamageInfo damageInfo) {

        if(!isServer) { return; }

        if(Invulnerable) {
            return;
        }

        currentHealth -= damageInfo.amount;
        if (currentHealth <= 0) {
            currentHealth = maxHealth;
            damageInfo.source.RpcGetAKill();
            RpcRespawn();
        }
    }

    [ClientRpc]
    void RpcRespawn() {
        if(isLocalPlayer) {
            GetComponent<MPlayerController>().BeDead();
        }
    }

    void OnChangeHealth(int currHealth) {
        healthBar.sizeDelta = new Vector2(currHealth, healthBar.sizeDelta.y);

    }
}
