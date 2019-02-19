namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class BosConst {

        public const int kRickshawId = 0;
        public const int kTaxiId = 1;
        public const int kTeleportId = 9;
        
        public const int kEarthId = 0;
        public const int kMoonId = 1;
        public const int kMarsId = 2;


        public static bool IsRickshawOrTaxi(this int generatorId)
            => generatorId == kRickshawId || generatorId == kTaxiId;
    }

    

}