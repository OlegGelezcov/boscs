namespace Bos.Services {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;

    public class PromoService : SaveableGameElement, IPromoService {

        private List<string> Codes { get; } = new List<string>();
        private bool IsInitialized { get; set; } = false;

        #region 
        public void Setup(object obj ) {
            if(!IsInitialized) {
                GameEvents.PromoReceived.Subscribe(args => {
                    if (args.IsSuccess) {
                        if (IsValid(args.Code)) {
                            Codes.Add(args.Code);
                            Services.PlayerService.AddCoins(args.Count);
                        }
                    }
                }).AddTo(Services.Disposables);
                IsInitialized = true;
            }
        }

        public bool IsValid(string code) {
            return Codes.Contains(code) == false;
        }

        public bool IsAllowPromo() => true;

        public void RequestPromo(string code) {
            Services.GetService<INetService>().GetPromoBonus(code, (cd, quantity) => {

            }, (err) => {

            });
        }

        public void UpdateResume(bool pause) {

        }
        #endregion

        #region Saveable overrides
        public override string SaveKey => "promo_service";

        public override bool IsLoaded {
            get;
            protected set;
        }

        public override Type SaveType 
            => typeof(PromoServiceSave);

        public override object GetSave()
            => new PromoServiceSave {
                codes = Codes.Select(c => c).ToList()
            };


        public override void LoadDefaults() {
            Codes.Clear();
            IsLoaded = true;
        }

        public override void LoadSave(object obj) {
            PromoServiceSave save = obj as PromoServiceSave;
            if(save == null ) {
                LoadDefaults();
            } else {
                save.Validate();
                Codes.Clear();
                Codes.AddRange(save.codes);
                IsLoaded = true;
            }
        }

        public override void ResetByInvestors() {
            IsLoaded = true;
        }

        public override void ResetByPlanets() {
            IsLoaded = true;
        }

        public override void ResetByWinGame() {
            IsLoaded = true;
        }

        public override void ResetFull() {
            IsLoaded = true;
        }
        #endregion
    }

}