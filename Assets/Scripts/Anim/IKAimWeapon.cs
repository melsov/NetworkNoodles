using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.Animations
{

    [Serializable]
    public struct CurveSlider
    {
        [HideInInspector]
        public float x;
        public AnimationCurve curve;
        [HideInInspector]
        public float slideDirection;
        public float baseUpIncrement;
        public float baseDownIncrement;

        public float slide(float deltaTime) {
            if (slideDirection > 0f) {
                x = Mathf.Clamp01(x + baseUpIncrement * deltaTime * slideDirection);
            } else {
                x = Mathf.Clamp01(x + baseDownIncrement * deltaTime * slideDirection);
            }
            return curve.Evaluate(x);
        }
    }
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
        bool _shouldAim;
        internal bool shouldAim {
            get { return _shouldAim; }
            set {
                if(_shouldAim && !value) {
                    StartCoroutine(rampDownAim());
                } 
                _shouldAim = value;
            }
        }

        [SerializeField] float stayAimingSeconds = 3f;
        private IEnumerator rampDownAim() {
            yield return new WaitForSeconds(stayAimingSeconds);
            shouldRampDownAim = !_shouldAim;
        }

        bool shouldRampDownAim = true;
        Rigidbody rb;

        [SerializeField]
        Vector3 thumbUpEulers = new Vector3(90f, 0f, 0f);

        [SerializeField]
        Transform leftHandTarget;

        [SerializeField]
        Transform rightHand;


        [SerializeField]
        private float nudgeLeftHandLeftward = 1f;

        [SerializeField] bool testMode;

        [SerializeField] bool testDisable;

        [SerializeField] CurveSlider aimSlider;
        float aimWeight;

        RaycastHit rh;

        float aimWeightIndex = 0f;
        float aimWeightRampDir {
            get { return shouldAim ? 1f : -1f; }
        }

        private void Start() {
            animtor = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
        }

        private void OnAnimatorIK(int layerIndex) {
            
            if(testDisable) { return; }

            if(testMode) {
                runTestMode();
                return;
            }

            slideAimSlider();

            pointWeapon(aimTargetPos, aimWeight);

        }

        void slideAimSlider() {

            aimSlider.slideDirection = _shouldAim || !shouldRampDownAim ? 1f : -1f;
            aimWeight = aimSlider.slide(Time.deltaTime);
        }


        private void runTestMode() {
            Ray camRay = Camera.main.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if(Physics.Raycast(camRay.origin, camRay.direction, out rh, 1000f)) {
                var dir = (rh.point - transform.position).normalized;
                dir.y = 0;
                pointWeapon(dir, 1f);
            }
        }

        void pointWeapon(Vector3 target, float weight) {
            var dir = (target - rightHand.position).normalized;
            //look
            animtor.SetLookAtWeight(1f); // always look at target
            animtor.SetLookAtPosition(target);

            Quaternion ro;
            ro = Quaternion.LookRotation(dir, Vector3.up);
            Quaternion rightRo = ro * Quaternion.Euler(thumbUpEulers);
            animtor.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
            animtor.SetIKRotation(AvatarIKGoal.RightHand, rightRo);

            //right pos / ro
            animtor.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
            Vector3 rightPos = centerRef.position + dir * handRadius + transform.rotation * nudgeWeaponToShoulder;
            animtor.SetIKPosition(AvatarIKGoal.RightHand, rightPos);
            Debug.DrawLine(centerRef.position, rightPos);

            // left
            //Left hand behaves weirdly on Jasper
            //For now...mostly don't ikposition left hand (weight / 4f)
            animtor.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight / 4f);
            var lPos = rightPos + dir * .15f; //<--fudge    new Vector3(leftHandTarget.position.x, rightHand.position.y, leftHandTarget.position.z);
            animtor.SetIKPosition(AvatarIKGoal.LeftHand, lPos); // rightPos + dir * .1f); 

            //left ro?
        }
    }
}
