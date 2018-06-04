using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.Animations
{
    public struct StateInput
    {
        public Vector2 xz;
        //public bool jumping;
        //public bool shooting;
    }

    public class PlayerAnimState : MonoBehaviour
    {

        [SerializeField]
        float damping = .15f;

        private readonly int m_HashHorizontalParam = Animator.StringToHash("btHorizontal");
        private readonly int m_HashVerticalParam = Animator.StringToHash("btVertical");

        private readonly int m_HashShootingParam = Animator.StringToHash("Shooting");
        private readonly int m_HashJumpingParam = Animator.StringToHash("Jumping");

        public bool jumping {
            get {
                return animtor.GetBool(m_HashJumpingParam);
            }
            set {
                animtor.SetBool(m_HashJumpingParam, value);
            }
        }
        public bool shooting {
            get {
                return animtor.GetBool(m_HashShootingParam);
            }
            set {
                if(!jumping) {
                    animtor.SetBool(m_HashShootingParam, value);
                }
            }
        }

        private void Update() {
            
        }

        public void updateAnimator(StateInput si) {
            animtor.SetFloat(m_HashHorizontalParam, si.xz.x, damping, Time.deltaTime);
            animtor.SetFloat(m_HashVerticalParam, si.xz.y, damping, Time.deltaTime);
        }

        Animator animtor;

        public void Start() {
            animtor = GetComponent<Animator>();
        }
    }
}
