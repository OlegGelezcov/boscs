namespace Bos {
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UDebug = UnityEngine.Debug;

    public class TransportUnitInfo {

        public int GeneratorId { get; private set; }
        public int LiveCount { get; private set; }
        public int BrokenedCount { get; private set; }
        public float BrokeCounter { get; private set; }

        private float constMechanicTimer = 0f;


        public void ResetByInvestors() {
            if(TotalCount > 0 ) {
                LiveCount = 1;
                BrokenedCount = 0;
                BrokeCounter = 0;
            }
        }

        public TransportUnitInfo(int generatorId, int liveCount) {
            GeneratorId = generatorId;
            LiveCount = liveCount;
            BrokenedCount = 0;
            BrokeCounter = 0f;
        }

        public TransportUnitInfo(TransportUnitInfoSave save) {
            GeneratorId = save.generatorId;
            LiveCount = save.liveCount;
            BrokenedCount = save.brokenedCount;
            BrokeCounter = save.brokeCounter;
            constMechanicTimer = save.constMechanicTimer;
        }

        public TransportUnitInfoSave GetSave()
            => new TransportUnitInfoSave {
                liveCount = LiveCount,
                brokeCounter = BrokeCounter,
                brokenedCount = BrokenedCount,
                generatorId = GeneratorId,
                constMechanicTimer = constMechanicTimer
            };


        public int TotalCount
            => LiveCount + BrokenedCount;

        public void AddLive(int count) {
            LiveCount += count;
        }

        public void SetLive(int count) {
            LiveCount = count;
            BrokenedCount = 0;

        }

        public void AddBrokened(int count) {
            BrokenedCount += count;
        }

        public bool RemoveLive(int count) {
            if (LiveCount >= count) {
                LiveCount -= count;
                return true;
            }
            return false;
        }

        public bool RemoveBrokened(int count) {
            if (BrokenedCount >= count) {
                BrokenedCount -= count;
                return true;
            }
            return false;
        }

        public bool Broke(int count) {
            if (RemoveLive(count)) {
                AddBrokened(count);
                //UDebug.Log($"Unit => {GeneratorId}, broke item => {count}, live count => {LiveCount}, brokened count => {BrokenedCount}".Colored(ConsoleTextColor.yellow));
                return true;
            }
            return false;
        }

        public int Repair(int count) {
            int actualRepaied = 0;
            for (int i = 0; i < count; i++) {
                if (RemoveBrokened(1)) {
                    AddLive(1);
                    actualRepaied++;
                } else {
                    break;
                }
            }
            //UDebug.Log($"Unit => {GeneratorId} repaired count => {actualRepaied}, live count => {LiveCount}, broken count => {BrokenedCount}".Colored(ConsoleTextColor.magenta));
            return actualRepaied;
        }

        public bool UpdateBroke(float interval, float brokeSpeed, int minLiveCount, IBosServiceCollection services) {

            BrokeCounter += interval * brokeSpeed * services.TimeChangeService.TimeMult;

            bool isBroked = false;

            if (LiveCount <= minLiveCount) {
                BrokeCounter = 0f;
            } else {

                int diff = LiveCount - minLiveCount;
                int brokeI = (int)BrokeCounter;
                int toBroke = Mathf.Min(diff, brokeI);
                if (toBroke > 0) {
                    bool isSuccess = Broke(toBroke);
                    if (isSuccess) {
                        isBroked = true;
                    }
                    BrokeCounter -= brokeI;
                }
            }

            return isBroked;
        }

        /*
        public void UpdateConstMechanicRepair(float deltaTime, int minLiveCount, IBosServiceCollection services ) {
            if(LiveCount < minLiveCount && BrokenedCount > 0 ) {
                float interval = 1f / services.MechanicService.ServiceSpeed;
                constMechanicTimer += deltaTime;
                if(constMechanicTimer > interval) {
                    int repairCount = (int)(constMechanicTimer / interval);
                    int resultCount = Repair(repairCount);
                    if(resultCount > 0 ) {
                        GameEvents.OnGeneratorUnitsCountChanged(this);
                    }
                    constMechanicTimer -= repairCount * interval;
                }

            }
        }*/
    }

    public class TransportUnitInfoSave {
        public int generatorId;
        public int liveCount;
        public int brokenedCount;
        public float brokeCounter;
        public float constMechanicTimer;
    }

}