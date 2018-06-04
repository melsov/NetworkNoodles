using System;
using UnityEngine;

namespace VctorExtensions
{
    public static class VctorExtensions
    {
		//Vec2 to Vec3
		public static Vector3 toVector3(this Vector2 v, float z) { return new Vector3(v.x, v.y, z); }

		public static Vector3 toVector3(this Vector2 v) { return v.toVector3(0f); }

        //Vec3 to Vec2
        public static Vector2 xy(this Vector3 v) { return new Vector2(v.x, v.y); }

        public static Vector2 xz(this Vector3 v) { return new Vector2(v.x, v.z); }

        public static Vector2 yz(this Vector3 v) { return new Vector2(v.y, v.z); }

        public static Vector2 toVector2(this Vector3 v) { return v.xy(); }
        
        //
        //Vec3
        //
        public static Vector3 mult(this Vector3 v, Vector3 other) { return new Vector3(v.x * other.x, v.y * other.y, v.z * other.z); }

        public static Vector3 divide(this Vector3 v, Vector3 other) { return new Vector3(v.x / other.x, v.y / other.y, v.z / other.z); }

        public static Vector3 sign(this Vector3 v) { return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z)); }

        public static Vector3 abs(this Vector3 v) { return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z)); }

        public static Vector3 min(this Vector3 v, Vector3 other) { return new Vector3(Mathf.Min(v.x, other.x), Mathf.Min(v.y, other.y), Mathf.Min(v.z, other.z)); }

        public static Vector3 max(this Vector3 v, Vector3 other) { return new Vector3(Mathf.Max(v.x, other.x), Mathf.Max(v.y, other.y), Mathf.Max(v.z, other.z)); }

        public static Vector3 average(this Vector3 v, Vector3 other) { return (v + other) / 2f; }

        public static Vector3 nonZeroToOne(this Vector3 v, float epsilon = 0.001f) {
            return BoolVector3.AGreaterThanB(v.abs(), new Vector3(epsilon, epsilon, epsilon));
        }

        public static Vector3 nonZeroToOneNegOne(this Vector3 v, float epsilon = 0.001f) {
            return Vector3.Scale(v.sign(), BoolVector3.AGreaterThanB(v.abs(), new Vector3(epsilon, epsilon, epsilon)));
        }

        //
        //Vec2
        //
        public static float dot(this Vector2 v, Vector2 other) { return v.x * other.x + v.y * other.y; }

		public static Vector2 mult(this Vector2 v, Vector2 other) { return new Vector2(v.x * other.x, v.y * other.y); }

        public static Vector2 divide(this Vector2 v, Vector2 other) { return new Vector2(v.x / other.x, v.y / other.y); }

        public static bool grThan(this Vector2 v, Vector2 other) { return v.x > other.x && v.y > other.y; }

        public static bool lessThan(this Vector2 v, Vector2 other) { return v.x < other.x && v.y < other.y; }

        public static Vector2 min(this Vector2 v, Vector2 other) { return new Vector2(Mathf.Min(v.x, other.x), Mathf.Min(v.y, other.y)); }

        public static Vector2 max(this Vector2 v, Vector2 other) { return new Vector2(Mathf.Max(v.x, other.x), Mathf.Max(v.y, other.y)); }

        public static Vector2 perp(this Vector2 v, bool turnRight = false) { return turnRight ?  new Vector2(v.y, -v.x) : new Vector2(-v.y, v.x); }

        public static float lesserComponent(this Vector2 v) { return v.x < v.y ? v.x : v.y; }

        public static float greaterComponent(this Vector2 v) { return v.x > v.y ? v.x : v.y; }

        //Bounds extensions
        public static bool Contains2D(this Bounds b, Vector2 v) {
            return b.min.xy().lessThan(v) && b.max.xy().grThan(v);
        }


        public struct BoolVector3
        {
            public bool x, y, z;

            public static BoolVector3 AGreaterThanB(Vector3 a, Vector3 b) {
                return new BoolVector3() {
                    x = a.x > b.x,
                    y = a.y > b.y,
                    z = a.z > b.z,
                };
            }
        
            public Vector3 toVector301() {
                return new Vector3(x ? 1f : 0f, y ? 1f : 0f, z ? 1f : 0f);
            }

            public static implicit operator Vector3(BoolVector3 b) { return b.toVector301(); }

        }

    }
	
}
