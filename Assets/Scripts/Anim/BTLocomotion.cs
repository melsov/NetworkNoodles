using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Mel.Animations
{
    public class BTLocomotion : StateMachineBehaviour
    {
        [SerializeField]
        float damping = .15f;

        private readonly int m_HashHorizontalParam = Animator.StringToHash("btHorizontal");
        private readonly int m_HashVerticalParam = Animator.StringToHash("btVertical");

        [SerializeField]
        bool DebugDontAnimate;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {

            if(DebugDontAnimate) { return; }

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector2 input = new Vector2(h, v).normalized;

            animator.SetFloat(m_HashHorizontalParam, input.x, damping, Time.deltaTime);
            animator.SetFloat(m_HashVerticalParam, input.y, damping, Time.deltaTime);
        }
    }
}
