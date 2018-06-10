using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VctorExtensions;
using System.Collections;

public class Respawner : MonoBehaviour
{

    [SerializeField] Terrain terrain;

    Bounds bounds { get { return terrain.terrainData.bounds; } }

    [SerializeField] Transform respawnRoomPrefab;

    [SerializeField] float timeOutTime = 8f;

    [SerializeField, MinMaxRange(0, 1)] MinMaxRange respawnBagelArea;

    Vector3 GetRandomLocationXZ() {
        float radius = bounds.size.xz().lesserComponent() / 2f;
        radius *= .95f;
        float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        radius = UnityEngine.Random.Range(respawnBagelArea.rangeStart * radius, respawnBagelArea.rangeEnd * radius);
        var loc = new Vector3(radius * Mathf.Cos(ang), 0f, radius * Mathf.Sin(ang));
        loc += terrain.transform.position + bounds.size * .5f; // bounds.center;
        loc.y = 0f;
        return loc;
    }

    public Vector3 GetRespawnLocation() {
        var loc = GetRandomLocationXZ();

        Ray ray = new Ray(loc + Vector3.up * 1000f, Vector3.down);
        RaycastHit hit;
        //if (Physics.Raycast(ray.origin, ray.direction, out hit, 2000f)) {
        //    loc = hit.point + Vector3.up;
        //}
        //else 
        {
            loc.y = terrain.SampleHeight(loc);
        }
        return loc;
    }

    public void placeInWaitingRoom(MPlayerController mp, Action OnTimeoutDone) {
        StartCoroutine(waitingRoom(mp, OnTimeoutDone));
    }

    private IEnumerator waitingRoom(MPlayerController mp, Action onTimeoutDone) {
        var loc = GetRandomLocationXZ();
        loc.y = UnityEngine.Random.Range(300f, 10000f);
        var wRoom = Instantiate(respawnRoomPrefab);
        wRoom.transform.position = loc;


        Bounds wBounds = wRoom.GetComponent<Collider>().bounds;
        var placePl = wBounds.center; 

        mp.GetComponent<Rigidbody>().MovePosition(placePl);

        mp.thirdCam.teleportToPlayer();

        yield return new WaitForSeconds(timeOutTime);

        Destroy(wRoom.gameObject);
        if(onTimeoutDone != null) {
            onTimeoutDone();
        }
    }
}
