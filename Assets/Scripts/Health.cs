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

        if(!isServer) {
            DebugHUD.Debugg("TDmg not SRV");
            return;
        }

        if(Invulnerable) {
            DebugHUD.Debugg("Invul");
            return;
        }

        currentHealth -= damageInfo.amount;
        var testLocalObj = NetworkServer.FindLocalObject(damageInfo.netId);
        DebugHUD.Debugg("damage: " + damageInfo.amount + "src: " + (testLocalObj ? testLocalObj.name : "null"));

        if (currentHealth <= 0) {
            currentHealth = maxHealth;
            var damageSource = NetworkServer.FindLocalObject(damageInfo.netId);
            var player = damageSource.GetComponent<MPlayerController>();
            if(player) {
                player.RpcGetAKill();
            }
            //damageInfo.source.RpcGetAKill();
            RpcRespawn();
        }
    }

    [ClientRpc]
    void RpcRespawn() {
        if(isLocalPlayer) {
            GetComponent<MPlayerController>().BeDead();
        }
    }

    //
    // currentHealth callback
    //
    void OnChangeHealth(int currHealth) {
        healthBar.sizeDelta = new Vector2(currHealth, healthBar.sizeDelta.y);
    }

}
