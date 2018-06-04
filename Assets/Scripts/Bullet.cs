using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour {

    [SerializeField] float displayTime = 1f;

    [SerializeField] LineRenderer lr;

    [SerializeField] float laserLength = 4f;
    [SerializeField] float laserTravel = 12f;

    public struct BulletInfo
    {
        public Vector3 destination;
    }

    public BulletInfo info;


    private void Start() {
        var dir = GetComponent<Rigidbody>().velocity;
        StartCoroutine(flashAndDie(dir.normalized)); // info.destination));
    }
    /*
     * We could piggy-back on rb velocity to infer the destination
     * (But this seems needlessly baroque. just laser forward a fixed short distance)
     */
    private IEnumerator flashAndDie(Vector3 dir) {

        //var dir = destination - transform.position;
        var start = transform.position;
        //var end = start + dir.normalized * laserTravel;
        //if (laserTravel * laserTravel > dir.sqrMagnitude) {
        //    end = destination;
        //}

        //if (!lr) { lr = GetComponent<LineRenderer>(); }

        //float maxTravel = (end - start).magnitude;
        float velocity = laserTravel / displayTime;
        int frames = (int)(displayTime / Time.deltaTime);
        //dir = dir.normalized;

        var origin = start;
        //lr.SetPosition(0, origin);
        //lr.SetPosition(1, origin + dir * (laserLength > maxTravel ? maxTravel : laserLength));

        ////Prevent having laser pierce through target
        //if (laserLength > maxTravel) {
        //    yield return new WaitForEndOfFrame();
        //}
        //else {

            //laser moves for a certain number of frames
            //or until just before it would hit the destination
        for (int i = 1; i < frames; ++i) {
            origin = start + dir * i * velocity;
            lr.SetPosition(0, origin);
            lr.SetPosition(1, origin + dir * laserLength);
            yield return new WaitForFixedUpdate();
            if ((i + 1) * velocity + laserLength > laserTravel) {
                break;
            }
        }


        Destroy(gameObject); //TEST should destroy immediately
    }

    /*
     * This approach breaks on clients because destination (from bullet info)
     * doesn't sync to the client
     */
    //private IEnumerator flashAndDieProblematicDontUse(Vector3 destination) {

    //    var dir = destination - transform.position;
    //    var start = transform.position;
    //    var end = start + dir.normalized * laserTravel;
    //    if(laserTravel * laserTravel > dir.sqrMagnitude) {
    //        end = destination;
    //    }

    //    if(!lr) { lr = GetComponent<LineRenderer>(); }

    //    float maxTravel = (end - start).magnitude;
    //    float velocity = laserTravel / displayTime;
    //    int frames = (int)(displayTime / Time.deltaTime);
    //    dir = dir.normalized;

    //    var origin = start;
    //    lr.SetPosition(0, origin);
    //    lr.SetPosition(1, origin + dir * (laserLength > maxTravel ? maxTravel : laserLength));

    //    //Prevent having laser pierce through target
    //    if (laserLength > maxTravel) {
    //        yield return new WaitForEndOfFrame();
    //    }
    //    else {

    //        //laser moves for a certain number of frames
    //        //or until just before it would hit the destination
    //        for (int i = 1; i < frames; ++i) {
    //            origin = start + dir * i * velocity;
    //            lr.SetPosition(0, origin);
    //            lr.SetPosition(1, origin + dir * laserLength);
    //            yield return new WaitForFixedUpdate();
    //            if ((i + 1) * velocity + laserLength > maxTravel) {
    //                break;
    //            }
    //        }
    //    }

    //    Destroy(gameObject); //TEST should destroy immediately
    //}


    
}
