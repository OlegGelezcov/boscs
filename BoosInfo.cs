using Newtonsoft.Json;

namespace Bos {
    public class BoostInfo  {

        public string Id { get; private set; }
        public double Value { get; private set; }     
        public bool IsPersist { get; private set; }
        public int EndTime { get; private set; }
        
        public BoostSourceType SourceType { get; private set; }


        public  BoostInfo(BoostInfoSave save)
            : this(save.id, save.value, save.isPersist, save.endTime, save.sourceType) {}

        public BoostInfo(string id, double value, bool isPersist, int endTime, BoostSourceType sourceType) {
            Setup(id, value, isPersist, endTime, sourceType);
        }

        public void Load(BoostInfoSave save) {
            Setup(save.id, save.value, save.isPersist, save.endTime, save.sourceType);
        }

        private void Setup(string id, double value, bool isPersist, int endTime, BoostSourceType sourceType) {
            Id = id;
            Value = value;
            IsPersist = isPersist;
            EndTime = endTime;
            SourceType = sourceType;
            GuardValue();
            GuardId();
        }

        public BoostInfoSave GetSave()
            => new BoostInfoSave() {
                id = Id,
                value = Value,
                isPersist = IsPersist,
                endTime = EndTime,
                sourceType = SourceType
            };

        private void GuardValue() {
            if (Value.IsZero()) {
                Value = 1.0;
            }
        }

        private void GuardId() {
            if (Id == null) {
                Id = string.Empty;
            }
        }

        public override string ToString() {
            return $"id: {Id}, value: {Value}, persist: {IsPersist}, end time: {EndTime}, source type: {SourceType}";
        }

        private static BoostInfo Create(string id, double value, bool isPersist, int endTime, int sourceType = 0) {
            return new BoostInfo(id, value, isPersist, endTime, (BoostSourceType)sourceType);
        }

        public static BoostInfo CreatePersist(string id, double value, int sourceType = 0)
            => Create(id, value, true, 0, sourceType);

        public static BoostInfo CreateTemp(string id, double value, int sourceType = 0)
            => Create(id, value, false, 0, sourceType);

        public static BoostInfo CreateTimed(string id, double value, int endTime, int sourceType = 0)
            => Create(id, value, false, endTime, sourceType);

    }


    [System.Serializable]
    public class BoostInfoSave {
        public string id;
        public double value;
        public bool isPersist;
        public int endTime;
        
        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public BoostSourceType sourceType;
    }
    
    
}