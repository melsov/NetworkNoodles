using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.Animations
{
    public class PlayerRootMotion : MonoBehaviour
    {
        //Animator animtor;
        

        //[SerializeField]
        //float ForwardSpeedMultiplier = 1f;
        //[SerializeField]
        //float RightSpeedMultiplier = 1.2f;

        //MPlayerController pc;

        //private void Start() {
        //    pc = GetComponent<MPlayerController>();
        //    animtor = GetComponent<Animator>();
        //}

        private void OnAnimatorMove() {
            //Vector3 inputDir = pc.inputDirection;
            //float animFor = Mathf.Abs(animtor.GetFloat("ForwardSpeed"));
            //float animRight = Mathf.Abs(animtor.GetFloat("RightSpeed"));
            //float z = animFor /*animtor.GetFloat("ForwardSpeed")*/ * ForwardSpeedMultiplier * Time.deltaTime *inputDir.z;
            //float x = animRight /* animtor.GetFloat("RightSpeed")*/ * RightSpeedMultiplier * Time.deltaTime * inputDir.x;
            //transform.position += transform.TransformDirection(new Vector3(x, 0f, z));
            ////Vector3 forw = transform.forward * animtor.GetFloat("ForwardSpeed") * ForwardSpeedMultiplier * Time.deltaTime;
            ////Vector3 right = transform.right * animtor.GetFloat("RightSpeed") * RightSpeedMultiplier * Time.deltaTime;
            ////transform.position += forw + right;

        }
    }
}
