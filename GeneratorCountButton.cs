namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class GeneratorCountButton : GameBehaviour {

        public Button button;
        public Text buttonText;

        private readonly Dictionary<GeneratorButtonState, string>
            buttonStateTexts = new Dictionary<GeneratorButtonState, string> {
                [GeneratorButtonState.MAX] = "MAX",
                [GeneratorButtonState.State_1] = "1",
                [GeneratorButtonState.State_10] = "10",
                [GeneratorButtonState.State_100] = "100"
            };

        public GeneratorButtonState State { get; private set; } = GeneratorButtonState.State_1;

        private int StateInt
            => (int)State;

        public int GeneratorId { get; private set; }

        private GeneratorInfo generator = null;
        public GeneratorInfo Generator
            => (generator != null) ?
            generator :
            (generator = Services.GenerationService.GetGetenerator(GeneratorId));


        public void Setup(int generatorId ) {
            this.GeneratorId = generatorId;
            //var generator = Services.GenerationService.GetGetenerator(GeneratorId);

            button.SetListener(() => {
                switch (State) {
                    case GeneratorButtonState.MAX: {
                            SetState(GeneratorButtonState.State_1);
                        }
                        break;
                    case GeneratorButtonState.State_1: {
                            SetState(GeneratorButtonState.State_10);
                        }
                        break;
                    case GeneratorButtonState.State_10: {
                            SetState(GeneratorButtonState.State_100);
                        }
                        break;
                    case GeneratorButtonState.State_100: {
                            SetState(GeneratorButtonState.MAX);
                        }
                        break;
                }
                Sounds.PlayOneShot(SoundName.click);
                GameEvents.GeneratorCountButtonClickedObservable.OnNext(generatorId);
            });
            SetState((GeneratorButtonState)Generator.BuyCountButtonState, isForceEvent: true);
        }

        private void SetState(GeneratorButtonState newState, bool isForceEvent = false) {
            UpdateButtonText(newState);
            if(State != newState || isForceEvent ) {
                State = newState;
                GameEvents.OnGeneratorButtonStateChanged(GeneratorId, GetBuyInfo());
            }
            if(Generator != null ) {
                Generator.SetBuyCountButtonState((int)State);
            }
        }

        private void UpdateButtonText(GeneratorButtonState state) {
            buttonText.text = "x".Colored("#F1FF00") + buttonStateTexts[state];
        }

        public BuyInfo GetBuyInfo() {
            int totalUnitCount = Services.TransportService.GetUnitTotalCount(Generator.GeneratorId);
            double companyCash = Services.PlayerService.CompanyCash.Value;

            if (State == GeneratorButtonState.MAX ) {         
                int count = Services.GenerationService.GetMaxNumberBuyable(companyCash, totalUnitCount, Generator);
                if(count > 0 ) {
                    return new BuyInfo {
                        Count = count,
                        Price = Services.GenerationService.CalculatePrice(count, totalUnitCount, Generator),
                        IsAllowed = true
                    };
                } else {
                    return new BuyInfo {
                        Count = 1,
                        Price = Services.GenerationService.CalculatePrice(1, totalUnitCount, Generator),
                        IsAllowed = false
                    };
                }
            } else {
                int targetUnitCount = StateInt;
                double price = Services.GenerationService.CalculatePrice(targetUnitCount, totalUnitCount, Generator);
                if(companyCash >= price ) {
                    return new BuyInfo {
                        Count = targetUnitCount,
                        Price = price,
                        IsAllowed = true
                    };
                } else {
                    return new BuyInfo {
                        Count = targetUnitCount,
                        Price = price,
                        IsAllowed = false
                    };
                }
            }
        }
    }

    public class BuyInfo {
        public int Count { get; set; }
        public double Price { get; set; }
        public bool IsAllowed { get; set; }

        public override string ToString() {
            return $"BuyInfo: Count => {Count} Price => {Price} IsAllowed => {IsAllowed}";
        }
    }

    public enum GeneratorButtonState : int {
        MAX = 0,
        State_1 = 1,
        State_10 = 10,
        State_100 = 100
    }
}