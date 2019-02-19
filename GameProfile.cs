namespace Bos.Data
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GameProfile
    {
        public string name { get; set; }
        public Dictionary<string, string> settings { get; set; }

        public string Appodeal
            => settings["appodeal"];

        public string AppMetrica
            => settings["appmetrica"];

    }
}
