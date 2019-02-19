namespace Bos
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class PlanetConst 
    {
        public const int EARTH_ID = 0;    
        public const int MOON_ID = 1;
        public const int MARS_ID = 2;
        public const int ASTEROID_BELT_ID = 3;
        public const int EUROPE_ID = 4;
        public const int TITAN_ID = 5;

        public static readonly int[] PlanetIds = new int[] {
            EARTH_ID,
            MOON_ID,
            MARS_ID,
            ASTEROID_BELT_ID,
            EUROPE_ID,
            TITAN_ID
        };

    }

}