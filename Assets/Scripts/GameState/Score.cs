using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Score : NetworkBehaviour
{
    [SyncVar(hook = "OnScored")]
    private Scoreboard.LedgerEntry _ledgerEntry;

    [SerializeField]
    GameObject scoreBomb;

    public int score { get { return _ledgerEntry.score; } }

    public MPlayerData playerData {
        get { return _ledgerEntry.playerData; }
        set {
            _ledgerEntry = new Scoreboard.LedgerEntry()
            {
                playerData = value,
                score = _ledgerEntry.score
            };
        }
    }

    Scoreboard _scoreboard;
    Scoreboard scoreboard { 
        get {
            if(!_scoreboard) {
                _scoreboard = FindObjectOfType<Scoreboard>();
            }
            return _scoreboard;
        }
    }

    [SerializeField]
    MPlayerController player;

    public void SetPlayerData(MPlayerData pdata, int _newScore) {
        CmdSetLedger(pdata, _newScore);
    }

    public void AddOne() { SubmitScore(score + 1); }

    public void ResetScore() { SubmitScore(0); }

    public void PingScore() { SubmitScore(score); }

    private void SubmitScore(int newScore) { CmdSetLedger(_ledgerEntry.playerData, newScore); }


    [Command]
    private void CmdSetLedger(MPlayerData pdata, int _newScore) {
        _ledgerEntry = new Scoreboard.LedgerEntry()
        {
            playerData = pdata,
            score = _newScore
        };
    }

    public void SetPlayerDataLocal(MPlayerData data) {
        _ledgerEntry.playerData = data;
    }

    void setLedgerLocal(Scoreboard.LedgerEntry le) {
        _ledgerEntry.playerData = le.playerData;
        _ledgerEntry.score = le.score;
    }

    public void PingLedgerToServer() {
        CmdSetLedger(_ledgerEntry.playerData, _ledgerEntry.score);
    }

    public void PingScoreboardLocal() {
        scoreboard.UpdateDisplay(_ledgerEntry);
    }

    //
    //SyncVar hook callback
    //
    void OnScored(Scoreboard.LedgerEntry currentLe) {
        if (isServer) {
            RpcRecordScore(currentLe);
        }
        //CmdSendScoreBomb(currentLe);
    }

    [ClientRpc]
    void RpcRecordScore(Scoreboard.LedgerEntry le) {
        setLedgerLocal(le);
        scoreboard.UpdateDisplay(le);

        DebugHUD.Debugg("record Score: " + le.ToString() + " is cli: " + isClient);
        player.testGetNewScore(le);
    }

    [ClientRpc]
    void RpcSendScoreBomb(Scoreboard.LedgerEntry le) {
        GameObject sb = (GameObject)Instantiate(scoreBomb, Vector3.one * 3f, Quaternion.identity);

        sb.GetComponent<ScoreBomb>().le = le;
        sb.GetComponent<Renderer>().material.color = isServer ? Color.blue : Color.red;
        NetworkServer.Spawn(sb); 
    }

    private void debugLE(string s) {
        debugLE(s, _ledgerEntry);
    }

    private void debugLE(string s, Scoreboard.LedgerEntry le) {
        DebugHUD.Debugg(string.Format("{0} : {1} {2}", (isClient ? "CLI" : "SRV"), s, le.ToString()));
    }


}

