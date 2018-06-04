using UnityEngine;

namespace Mel.Math
{
    public struct Segment3
    {
        public Vector3 A, B;

        public Vector3 start {
            get { return A; }
            set { A = value; }
        }

        public Vector3 end {
            get { return B; }
            set { B = value; }
        }
    }
}
