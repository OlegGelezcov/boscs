namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;




    public interface IAchievment {
        int Id { get; }
        string Name { get;  }
    }

    public class AchievmentInfo : IAchievment {

        public int Id { get; private set; }
        public string Name { get; private set; }

        public AchievmentInfo(int id, string name) {
            Id = id;
            Name = name;
        }
    }

}