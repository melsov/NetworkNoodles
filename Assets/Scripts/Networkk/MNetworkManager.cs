using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class MNetworkManager : NetworkManager
{


    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        GameObject player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        // Do nothing actually. we could revert to not sub-classing

        //Score plc = player.GetComponent<Score>();
        //plc.playerData = new MPlayerData()
        //{
        //    displayName = string.Format("FixMe{0}", conn.connectionId),
        //    color = Color.red,
        //    netID = plc.netId.Value, //duplicate data ...
        //};

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
