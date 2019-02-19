using System;

namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class BosMath {

        public const double kEpsilon = 1e-7;
        
        public static double Clamp01(double value) {
            if(value < 0 ) {
                value = 0;
            } else if(value > 1 ) {
                value = 1;
            }
            return value;
        }

        public static Vector2 Orthogonal(this Vector2 input) {
            if (!Mathf.Approximately(input.y, 0)) {
                return new Vector2( 1, -input.x / input.y).normalized;
            }
            return new Vector2(0, 1);
        }

        public static Vector2 ControlPoint(Vector2 first, Vector2 second, float linearFactor, float orthoFactor, int side) {
            Vector2 direction = second - first;
            float length = direction.magnitude;
            Vector2 directionNormalized = direction.normalized;
            Vector2 orthogonal = direction.Orthogonal();
            if (side < 0) {
                orthogonal = -orthogonal;
            }
            return first + directionNormalized * length * linearFactor + orthogonal * length * orthoFactor;
        }

        public static bool Approximately(double first, double second) {
            return Math.Abs(first - second) < kEpsilon;
        }

        public static bool IsZero(this double val)
            => Approximately(val, 0.0);
    }

}