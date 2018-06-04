using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {
    [SyncVar] CubeState state;

    struct CubeState
    {
        public int x;
        public int y;
    }

    private void Awake() {
        initState();
    }

    [Server] private void initState() {
        state = new CubeState {
            x = 0,
            y = 0
        };
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isLocalPlayer) {
            KeyCode[] arrowKeys = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow };
            foreach (KeyCode arrowKey in arrowKeys) {
                if (!Input.GetKeyDown(arrowKey)) continue;
                CmdMoveOnServer(arrowKey); // Move(state, arrowKey);
            }
        }
        SyncState();
    }

    [Command] void CmdMoveOnServer (KeyCode arrowKey) {

    }

    private CubeState Move(CubeState previous, KeyCode arrowKey) {
        int dx = 0;
        int dy = 0;
        switch (arrowKey) {
            case KeyCode.UpArrow:
                dy = 1;
                break;
            case KeyCode.DownArrow:
                dy = -1;
                break;
            case KeyCode.RightArrow:
                dx = 1;
                break;
            case KeyCode.LeftArrow:
                dx = -1;
                break;
        }
        return new CubeState {
            x = dx + previous.x,
            y = dy + previous.y
        };
    }

    private void SyncState() {
        transform.position = new Vector2(state.x, state.y);
    }
}
