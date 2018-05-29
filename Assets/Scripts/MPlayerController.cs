using Mel.Animations;
using Mel.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using VctorExtensions;
using UnityEngine.UI;
using Mel.Math;

public struct MPlayerData
{
    public string displayName;
    public Color color;
    public uint netID;

    public override string ToString() {
        return string.Format("MPlayerData: {0} | netID {1} ", displayName, netID);
    }
}

public class MPlayerController : NetworkBehaviour {

    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform bulletSpawn;

    //movement
    [SerializeField]
    float walkSpeed = 3.1f;
    [SerializeField]
    float runSpeed = 6f;
    PlayerAnimState animState;

    bool running;

    ThirdCam thirdCam;
    [SerializeField]
    float turnWithCamLookSpeed = 10f;
    [SerializeField]
    Transform thirdCamFollowTarget;

    [SerializeField]
    float xzLerpMultiplier = 1f;

    private Rigidbody rb;

    [SerializeField]
    private float jumpIntervalSeconds = 1.2f;
    [SerializeField]
    private float jumpForce = 12f;

    [SerializeField]
    private string[] shootableLayers;
    Health localHealth;

    [SerializeField]
    float timeBetweenShots = 2f;
    private bool canShoot = true;
    private AudioSource aud;
    private Collider collidr;

    DebugHUD debugHUD;


    [SerializeField]
    bool showDebugLineRenderer;
    [SerializeField]
    LineRenderer dbugLR;
    private Vector3 lastFramePosition;

    [SerializeField]
    Text namePlate;


    public Vector2 inputXZ {
        get; private set;
    }

    [SerializeField]
    Score score;
    private AudioListener audioListenr;

    public MPlayerData playerData {
        get {
            return score.playerData;
        }
        set {
            score.playerData = value;
        }
    }

    void setNameLocal(string _newName) {
        MPlayerData pd = playerData;
        pd.displayName = _newName;
        score.SetPlayerDataLocal(pd);
    }


    void updateDbugLR() {
        if(!showDebugLineRenderer) {
            dbugLR.SetPosition(0, Vector3.zero);
            dbugLR.SetPosition(1, Vector3.zero);
            return;
        }
        dbugLR.SetPosition(0, bulletSpawn.transform.position);
        var camRay = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        dbugLR.SetPosition(1, camRay.origin + camRay.direction * 40f);
    }

    void Update()
    {

        updateDbugLR();
        if(!isLocalPlayer) {
            return;
        }

        if(Input.GetMouseButton(0)) {
            Ray shootRay = shootDirection();
            //CmdFire(shootRay.origin, shootRay.direction); // Think this broke shooting on client !!
            CmdFire(bulletSpawn.position, shootRay.direction);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(jump());
        }

        inputXZ = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        animState.updateAnimator(new StateInput() { xz = inputXZ });
    }

    private IEnumerator jump() {
        if(!animState.jumping) {

            animState.jumping = true;
            rb.AddForce(0f, jumpForce * rb.mass, 0f);
            yield return new WaitForSeconds(jumpIntervalSeconds);
            animState.jumping = false;

        }
    }

    private void FixedUpdate() {

        if(!isLocalPlayer) {
            return;
        }

        running = Input.GetAxis("LeftShift") > 0f;
        var speed = running ? runSpeed : walkSpeed;
   
        Vector3 input = new Vector3(inputXZ.x, 0f, inputXZ.y);

        Vector3 targetPos = transform.position + transform.TransformDirection(input.normalized) * speed;
        Vector3 nextPos = Vector3.Lerp(transform.position, targetPos, xzLerpMultiplier * Time.deltaTime);
        rb.MovePosition(nextPos);

        lookWhereCamLooks();
    }

    private void lookWhereCamLooks() {
        Vector3 lTarget = thirdCam.transform.position + thirdCam.transform.forward * 100f;
        Vector3 dir = lTarget - transform.position;
        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        look = Quaternion.Slerp(transform.rotation, look, turnWithCamLookSpeed * Time.deltaTime);
        rb.MoveRotation(look);
    }

    Ray shootDirection() {
        var camRay = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        var destination = camRay.origin + camRay.direction * 1000f;

        //put raycast origin in front of player
        var camToPlayer = collidr.bounds.center - camRay.origin;
        var dist = Vector3.Dot(camToPlayer, camRay.direction) + collidr.bounds.extents.z * 1.1f;
        var nudgedOrigin = camRay.origin + camRay.direction * dist;

        RaycastHit shootHitInfo;

        if (Physics.Raycast(nudgedOrigin, camRay.direction, out shootHitInfo, 10000f)) {
            destination = shootHitInfo.point;
        }

        return new Ray(nudgedOrigin, (destination - bulletSpawn.transform.position).normalized);
        //return (destination - bulletSpawn.transform.position).normalized;
    }

    public struct DamageInfo
    {
        public int amount;
        public MPlayerController source;
    }


    [Command]
    private void CmdFire(Vector3 origin, Vector3 direction) {

        if(!canShoot) {
            return;
        }

        if (aud) {
            aud.Play();
        }

        StartCoroutine(shotTimer());

        var destination = origin + direction * 1000f;

        RaycastHit shootHitInfo;

        if (Physics.Raycast(origin, direction, out shootHitInfo, 1000f)) {

            //destination = shootHitInfo.point;

            var health = shootHitInfo.collider.GetComponent<Health>();
            if (health && health != localHealth) {
                health.TakeDamage(new DamageInfo()
                {
                    amount = 10,
                    source = this,
                });
            }
        }

        var bullet = (GameObject) Instantiate(
                bulletPrefab,
                bulletSpawn.position,
                bulletSpawn.rotation);

        bullet.GetComponent<Rigidbody>().velocity = direction * 5f;
        bullet.GetComponent<Bullet>().info = new Bullet.BulletInfo() { destination = destination }; 
        NetworkServer.Spawn(bullet);

        // Destroy the bullet after 5 seconds
        Destroy(bullet, 5.0f);
    }

    [ClientRpc]
    public void RpcGetAKill() {

        if(!isLocalPlayer) {
            dbugWithName("not lcl in getAKill");
            return;
        }
        score.AddOne();
        dbugWithName("GotAKill ? score: " + GetComponent<Score>().score + " --0r: " + score.score);
    }

    private IEnumerator shotTimer() {
        if(canShoot) {
            canShoot = false;

            // TODO: find a better shooting anim
            //animState.shooting = true;

            yield return new WaitForSeconds(timeBetweenShots);
            canShoot = true;

            //animState.shooting = false;
        }
    }

    private void Awake() {
        dbugWithName("awake");
        if (isLocalPlayer) {
            
        }
    }

    private IEnumerator GetLoadOut() {
        var loadOutGUI = FindObjectOfType<LoadOutGUI>();
        dbugWithName("get load out?");
        thirdCam.uiMode(true);
        while(!loadOutGUI.GetLoadOut( (LoadOutGUI.LoadOutData loadOutData) =>
        {
            dbugWithName("got name [" + loadOutData.displayName + "]");
            MPlayerData data = playerData;
            data.displayName = loadOutData.displayName;
            data.color = FindObjectOfType<MColorSets>().nextPlayerColor();
            data.netID = netId.Value;
            score.SetPlayerData(data, 0);
            //score.SetPlayerDataLocal(data);
            //score.PingLedgerToServer();
            thirdCam.uiMode(false);
        })) {
            dbugWithName("waiting");
            yield return new WaitForSeconds(.3f);
        }
    }

    public override void OnStartLocalPlayer() {

        name = isServer ? "PlayerServer" : "PlayerClient";

        thirdCam = FindObjectOfType<ThirdCam>();
        thirdCam.Target = thirdCamFollowTarget; 

        localHealth = GetComponent<Health>();
        audioListenr = gameObject.AddComponent<AudioListener>();

        score.SetPlayerData(new MPlayerData()
        {
            displayName = string.Format("Something{0} {1} ", (isServer ? "SRVR" : "CLI"), netId),
            color = Color.red,
            netID = netId.Value,
        }, 0);

        startLocalOrNot();
        StartCoroutine(GetLoadOut());
    }





    private void Start() {
        score.PingScoreboardLocal();
        startLocalOrNot();
        if(!isLocalPlayer) {
            setNamePlate();
        }
    }

    void setNamePlate() {
        namePlate.text = string.Format("{0}", playerData.displayName);
    }

    private void startLocalOrNot() {
        if (!animState)
            animState = GetComponent<PlayerAnimState>();
        if(!rb)
            rb = GetComponent<Rigidbody>();
        if(!collidr)
            collidr = GetComponent<Collider>();
        if(!aud)
            aud = GetComponent<AudioSource>();
        if (!debugHUD)
            debugHUD = FindObjectOfType<DebugHUD>();
       
    }

    public void testGetNewScore(Scoreboard.LedgerEntry le) {
        setNamePlate();
    }


    public void dbugWithName(string s) {
        DebugHUD.Debugg(string.Format("{0}: scr: {1} {2}", playerData.displayName, score.score, s));
    }
}
