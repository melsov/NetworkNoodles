using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.Animations
{

    //TODO: ik too awkward to handle in networked play?
    //Send data as an anim param?? (sounds dicey)

    public class IKAimWeapon : MonoBehaviour
    {
        Animator animtor;

        [SerializeField]
        float handRadius = 1f;
        [SerializeField]
        Transform centerRef;
        [SerializeField]
        Vector3 nudgeWeaponToShoulder = new Vector3(.2f, 0f, -.2f);

        private readonly int shootingParamHash = Animator.StringToHash("Shooting");

        public Vector3 aimTargetPos;
        internal bool shouldAim;

        [SerializeField]
        Vector3 thumbUpEulers = new Vector3(90f, 0f, 0f);

        [SerializeField]
        Transform leftHandTarget;

        [SerializeField]
        Transform rightHand;

        [SerializeField] bool testMode;

        [SerializeField] bool testDisable;

        private void Start() {
            animtor = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex) {
            
            if(testDisable) { return; }

            if(testMode) {
                runTestMode();
                return;
            }
            if(shouldAim) {

                pointWeapon(aimTargetPos);

                //animtor.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                //animtor.SetIKPosition(AvatarIKGoal.RightHand, transform.position + aim * handRadius);

                //    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                //animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                //animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
            }
            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else {
                animtor.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animtor.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animtor.SetLookAtWeight(0);
            }
        }

        RaycastHit rh;
        private void runTestMode() {
            Ray camRay = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if(Physics.Raycast(camRay.origin, camRay.direction, out rh, 1000f)) {
                var dir = (rh.point - transform.position).normalized;
                dir.y = 0;
                pointWeapon(dir);
            }
        }

       

        void pointWeapon(Vector3 target) {
            var dir = (target - rightHand.position).normalized;
            //look
            animtor.SetLookAtWeight(1f);
            animtor.SetLookAtPosition(target);

            Quaternion ro;
            ro = Quaternion.LookRotation(dir, Vector3.up);
            Quaternion rightRo = ro * Quaternion.Euler(thumbUpEulers);
            animtor.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            animtor.SetIKRotation(AvatarIKGoal.RightHand, rightRo);

            //right pos / ro
            animtor.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            Vector3 rightPos = centerRef.position + dir * handRadius + transform.rotation * nudgeWeaponToShoulder;
            animtor.SetIKPosition(AvatarIKGoal.RightHand, rightPos);
            Debug.DrawLine(centerRef.position, rightPos);


            animtor.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animtor.SetIKPosition(AvatarIKGoal.LeftHand, rightPos + dir * .1f); // leftHandTarget.position);
        }
    }
}
