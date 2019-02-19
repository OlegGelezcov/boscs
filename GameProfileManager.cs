namespace Bos.Data
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UnityEngine;
    using System.Linq;
    using System;


    public class GameProfileManager : IProfileRepository
    {
        public List<GameProfile> profiles { get; private set; }
        public GameProfile activeProfile { get; private set; }

        public bool IsLoaded
        {
            get;
            private set;
        }

        public void Load(string file)
        {
            if (!IsLoaded)
            {
                profiles = JsonConvert.DeserializeObject<List<GameProfile>>(Resources.Load<TextAsset>(file).text);
                SelectProfile();
                IsLoaded = true;
            }
        }

        private GameProfile GetProfile(string profileName)
            => profiles.FirstOrDefault(p => p.name == profileName);

        private void SelectProfile()
        {
#if UNITY_IOS
            SelectIosProfile();
#elif UNITY_ANDROID
            activeProfile = GetProfile("android_bos2");
#endif
            if (activeProfile == null)
            {
                throw new Exception("active profile is null");
            }
        }

        private void SelectIosProfile()
        {
#if BOS
            activeProfile = GetProfile("ios_bos");
#else
            activeProfile = GetProfile("ios_bos2");
#endif
        }
    }

    public interface IProfileRepository : IRepository
    {
        GameProfile activeProfile
        {
            get;
        }
    }
}