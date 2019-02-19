namespace  Bos.SplitLiner {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SplitLinerCollider : MonoBehaviour {
        public SplitLinerColliderType colliderType;
    }

    public enum SplitLinerColliderType {
        red1,
        red2,
        red3,
        yellow,
        blue1,
        blue2,
        blue3,
        green
    }
}