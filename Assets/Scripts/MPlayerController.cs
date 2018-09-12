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
using Mel.Weapons;
using Mel.Item;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MPlayerController))]
public class EditorMPlayerControler : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(GUILayout.Button("Debug Info")) {
            var mp = (MPlayerController)target;
            mp.dbugNetworkID();
        }
    }
}

#endif

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


    public ThirdCam thirdCam { get; private set; }


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
    [SyncVar(hook = "OnCanShootChanged")]
    private bool canShoot = true;
    private AudioSource aud;
    private Collider collidr;

    DebugHUD debugHUD;
    [SyncVar]
    bool testMute;

    [SerializeField]
    bool showDebugLineRenderer;
    [SerializeField]
    LineRenderer dbugLR;
    private Vector3 lastFramePosition;

    [SerializeField]
    Text namePlate;

    IKAimWeapon ikAimWeapon;
    [SerializeField]
    Vector3 aimOffsetEulers;

    Quaternion aimOffset {
        get { return Quaternion.Euler(aimOffsetEulers); }
    }


    public Vector2 inputXZ {
        get; private set;
    }

    [SerializeField]
    Score score;
    private AudioListener audioListenr;

    Arsenal _arsenal;
    Arsenal arsenal {
        get {
            if(!_arsenal) {
                _arsenal = GetComponentInChildren<Arsenal>();
            }
            return _arsenal;
        }
    }

    Respawner _respawner;
    private bool isDead;
    bool suspendFixedUpdateMovement;
    bool suspendControls;

    Respawner respawner {
        get {
            if (!_respawner) { _respawner = FindObjectOfType<Respawner>(); }
            return _respawner;
        }
    }

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

        //updateDbugLR();

        if(!isLocalPlayer) {
            return;
        }
        if (suspendControls) {
            return;
        }

        Vector3 lookTarget;
        Ray camRay = GetCamRay();

        ikAimWeapon.aimTargetPos = camRay.origin + camRay.direction * 100f;

        if (arsenal.isArmed) {

            Ray shootRay = new Ray();
            if(Input.GetMouseButton(0) || Input.GetMouseButtonDown(1)) {
                shootRay = shootDirection(camRay, out lookTarget);
                ikAimWeapon.aimTargetPos = lookTarget;
            }
            if (Input.GetMouseButtonDown(1)) {
                thirdCam.shouldAim.Value = true;
            }
            else if (Input.GetMouseButtonUp(1)) {
                thirdCam.shouldAim.Value = false;
            }

            if (Input.GetMouseButton(0)) {
                ikAimWeapon.shouldAim = true;
                CmdFire(bulletSpawn.position, shootRay.direction);
            }
            else {
                ikAimWeapon.shouldAim = false;
            }
        } else {
            //DEBUG
            if (Input.GetMouseButtonDown(0)) {
                DebugHUD.Debugg("Not armed " );
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            StartCoroutine(jump());
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            arsenal.nextWeapon();
        }

        //TEST
        if(Input.GetKeyDown(KeyCode.H)) {
            BeDead();
        }
        if(Input.GetKeyDown(KeyCode.M)) {
            CmdToggleTestMute();
        }

        inputXZ = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        animState.updateAnimator(new StateInput() { xz = inputXZ });
    }

    [Command]
    void CmdToggleTestMute() {
        testMute = !testMute;
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
        if(suspendFixedUpdateMovement) { return; }

        running = Input.GetAxis("LeftShift") > 0f;
        var speed = running ? runSpeed : walkSpeed;
   
        Vector3 input = new Vector3(inputXZ.x, 0f, inputXZ.y);

        Vector3 targetPos = transform.position + transform.TransformDirection(input.normalized) * speed;
        rb.MovePosition(Vector3.Lerp(transform.position, targetPos, xzLerpMultiplier * Time.deltaTime));

        lookWhereCamLooks();
    }

    private void lookWhereCamLooks() {
        Vector3 lTarget = thirdCam.transform.position + thirdCam.transform.forward * 100f;
        Vector3 dir = lTarget - transform.position;
        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        look = Quaternion.Slerp(transform.rotation, look, turnWithCamLookSpeed * Time.deltaTime);
        rb.MoveRotation(look);
    }

    Ray GetCamRay() { return Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0f)); } 

    Ray shootDirection(Ray camRay, out Vector3 lookTarget) {

        lookTarget = camRay.origin + camRay.direction * 1000f;

        //put raycast origin in front of player
        //to avoid magically shooting backwards
        var camToPlayer = collidr.bounds.center - camRay.origin;
        var dist = Vector3.Dot(camToPlayer, camRay.direction) + collidr.bounds.extents.z * 1.1f;
        var nudgedOrigin = camRay.origin + camRay.direction * dist;

        RaycastHit shootHitInfo;

        if (Physics.Raycast(nudgedOrigin, camRay.direction, out shootHitInfo, 1000f)) {
            lookTarget = shootHitInfo.point;
        }

        return new Ray(nudgedOrigin, (lookTarget - bulletSpawn.transform.position).normalized);
    }

    public struct DamageInfo
    {
        public int amount;
        //public MPlayerController source;
        public NetworkInstanceId netId;
    }


    [Command]
    private void CmdFire(Vector3 origin, Vector3 direction) {

        if(!canShoot) {
            return;
        }

        Weapon weapon = arsenal.equipedWeapon;
        Assert.IsTrue(weapon);

        //weapon.playFire();

        if (!testMute && aud) 
        {
            aud.Play();
        } else if (testMute) 
        {
            DebugHUD.Debugg("test muted");
        }

        StartCoroutine(shotTimer());

        var destination = origin + direction * 1000f;

        RaycastHit shootHitInfo;

        if (!weapon.bulletPrefab.collisionDealsDamage) 
        {
            if (Physics.Raycast(origin, direction, out shootHitInfo, 1000f)) 
            {
                var health = shootHitInfo.collider.GetComponent<Health>();
                if (health && health != localHealth) 
                {
                    health.TakeDamage(new DamageInfo()
                    {
                        amount = 10,
                        netId = netId
                    });
                }
            }
        }

        var goBullet = (GameObject) Instantiate(
                weapon.bulletPrefab.gameObject,
                bulletSpawn.position,
                bulletSpawn.rotation);

        goBullet.GetComponent<Rigidbody>().velocity = direction;
        var bullet = goBullet.GetComponent<Bullet>();
        bullet.damage = new DamageInfo()
        {
            amount = bullet. collisionDamage,
            netId = netId
        };
        goBullet.GetComponent<Bullet>().info = new Bullet.BulletInfo() { destination = destination }; 
        NetworkServer.Spawn(goBullet);

        // Destroy the bullet after 25 seconds
        Destroy(goBullet, 25.0f);
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

    private void OnCanShootChanged(bool nextCanShoot) {
        //ikAimWeapon.shouldAim = nextCanShoot;
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


    internal void BeDead() {
        if(isDead) {
            return;
        }
        DebugHUD.Debugg("start dead");
        isDead = true;
        arsenal.loseAll();
        StartCoroutine(goToWaitingRoom());
    }

    private IEnumerator goToWaitingRoom() {
        suspendFixedUpdateMovement = true;
        respawner.placeInWaitingRoom(this, () =>
        {
            DebugHUD.Debugg("wroom callback");
            teleportToRespawnLocation(() =>
            {
                isDead = false;
            });
        });
        yield return new WaitForSeconds(.1f);
        suspendFixedUpdateMovement = false;
    }

    private IEnumerator teleportTo(Vector3 pos, Action callback = null) {
        suspendFixedUpdateMovement = true;
        rb.MovePosition(pos);
        yield return new WaitForSeconds(.1f);
        suspendFixedUpdateMovement = false;
        if(callback != null) {
            callback();
        }
    }

    void teleportToRespawnLocation(Action callback = null) {
        var respos = respawner.GetRespawnLocation();
        respos.y += collidr.bounds.size.y;
        StartCoroutine(teleportTo(respos, callback));
    }

    private IEnumerator GetLoadOut() {
        var loadOutGUI = FindObjectOfType<LoadOutGUI>();
        suspendControls = true;
        thirdCam.uiMode(true);
        while(!loadOutGUI.GetLoadOut( (LoadOutGUI.LoadOutData loadOutData) =>
        {
            MPlayerData data = playerData;
            data.displayName = loadOutData.displayName;
            data.color = FindObjectOfType<MColorSets>().nextPlayerColor();
            data.netID = netId.Value;
            score.SetPlayerData(data, 0);

            thirdCam.uiMode(false);
            suspendControls = false;
            teleportToRespawnLocation();

        })) {
            dbugWithName("waiting");
            yield return new WaitForSeconds(.3f);
        }
    }

    public override void OnStartLocalPlayer() {

        name = isServer ? "PlayerServer" : "PlayerClient";

        thirdCam = FindObjectOfType<ThirdCam>();
        thirdCam.Target = thirdCamFollowTarget;
        thirdCam.m_AimSettings = AimSettings.DefaultAimSettings();

        //arsenal = GetComponentInChildren<Arsenal>();
        arsenal.Setup();

        localHealth = GetComponent<Health>();
        audioListenr = gameObject.AddComponent<AudioListener>();
        ikAimWeapon = GetComponent<IKAimWeapon>();

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

        if (!isLocalPlayer) {
            setNamePlate();
        }
    }

    public void dbugNetworkID() {
        foreach(var id in GetComponentsInChildren<NetworkIdentity>(true)) {
            Debug.Log(string.Format("{0} ", id.name));
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
        //dbugWithName("set name plate");
        setNamePlate();
    }

    public string cliServer {
        get { return string.Format("{0}{1}", isServer ? "Serv" : "", isClient ? "Cli" : ""); }
    }

    public void handlePickup(Pickup pickup, PickupSpawner pkSpawner) {
        if(!isLocalPlayer) { return; }
        pickup.getGiven(this);
        CmdToggleSpawner(pkSpawner.netId);
    }

    [Command]
    void CmdToggleSpawner(NetworkInstanceId pkSpawnerID) {
        var pks = NetworkServer.FindLocalObject(pkSpawnerID).GetComponent<PickupSpawner>();
        pks.Give();
    }

    internal void acquireWeapon(int weaponIndex) {
        arsenal.SetAvailable(weaponIndex, true);
        //if(arsenal.count == 1) {
        //    DebugHUD.Debugg("aq weap: " + weaponIndex);
        //    arsenal.Equip(weaponIndex);
        //}
    }


    public void ClientOnSwitchedWeapon(int wIndex) {
        Weapon weapon = arsenal.equipedWeapon;
        if(weapon) {
            aud.clip = weapon.fireAudio.clip;
        }

        namePlate.text = string.Format("{0} (weap: {1})", playerData.displayName, arsenal.equipedSVIndex);
        if (isLocalPlayer) {
            if (weapon) {
                thirdCam.m_AimSettings = weapon.aimSettings;
            }
            else {
                thirdCam.m_AimSettings = AimSettings.DefaultAimSettings();
            }
        }
    }

    public void loseWeapon(int weaponIndex) {
        arsenal.SetAvailable(weaponIndex, false);
    }

    public void dbugWithName(string s) {
        DebugHUD.Debugg(string.Format("{0}: {1} {2}", playerData.displayName, cliServer, s));
    }
}
