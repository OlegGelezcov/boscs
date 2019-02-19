//namespace Bos {
//    using System.Collections;
//    using System.Collections.Generic;
//    using UnityEngine;

//    public class PlayerDataUpgrader : GameElement, IPlayerDataUpgrader {

//        public void Setup(object data = null) {

//        }

//        //public void Upgrade(UpgradeItem item) {
//        //    switch(item.Type) {
//        //        case UpgradeType.Profit: {
//        //                UpgradeProfit(item);
//        //            }
//        //            break;
//        //        case UpgradeType.Time: {
//        //                UpgradeTime(item);
//        //            }
//        //            break;
//        //    }
//        //}

//        /*
//        private void UpgradeTime(UpgradeItem item ) {
//            if(item.IsGlobal) {
//                if(item.IsPermanent) {
//                    Services.GenerationService.Generators.ApplyPermanent(GeneratorBonusMult.CreateTimeMult(item.Multiplier));
//                } else {
//                    Services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateTimeMult(item.Multiplier));
//                }
//            } else {
//                if(item.IsPermanent) {
//                    Services.GenerationService.Generators.ApplyPermanent(item.ItemId, GeneratorBonusMult.CreateTimeMult(item.Multiplier));
//                } else {
//                    Services.GenerationService.Generators.ApplyTime(item.ItemId, item.Multiplier);
//                }
//            }
//        }*/

//        //private void UpgradeProfit(UpgradeItem item) {
//        //    if(item.IsGlobal) {
//        //        if(item.IsPermanent) {
//        //            Services.GenerationService.Generators.ApplyPermanent(GeneratorBonusMult.CreateProfitMult(item.Multiplier));
//        //        } else {
//        //            Services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateProfitMult(item.Multiplier));
//        //        }
//        //    } else {
//        //        if(item.IsPermanent) {
//        //            Services.GenerationService.Generators.ApplyPermanent(item.ItemId, GeneratorBonusMult.CreateProfitMult(item.Multiplier));
//        //        } else {
//        //            ProfitMultInfo profitMult = new ProfitMultInfo($"profit_upgrade_{System.Guid.NewGuid().ToString().Substring(0, 5)}", item.Multiplier);
//        //            GameServices.Instance.GenerationService.Generators.ApplyProfit(item.ItemId, profitMult);
//        //        }
//        //    }
//        //}

//        /*
//        public void UpgradeTime(PlayerData playerData, UpgradeItem item) {

//            if(item.ItemId == -1 ) {
//                if(item.IsPermanent) {
//                    Services.GenerationService.Generators.ApplyPermanent(GeneratorBonusMult.CreateTimeMult(item.Multiplier));
//                } else {
//                    Services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateTimeMult(item.Multiplier));
//                }
//            } else {
//                if(item.IsPermanent) {
//                    Services.GenerationService.Generators.ApplyPermanent(item.ItemId, GeneratorBonusMult.CreateTimeMult(item.Multiplier));
//                }

//                Services.GenerationService.Generators.ApplyTime(item.ItemId, item.Multiplier);
//            }
//            playerData.Save();
//        }
 
//        public void UpgradeProfit(PlayerData playerData, UpgradeItem item) {
//            if(item.ItemId == -1 ) {
//                if(item.IsPermanent) {
//                    Services.GenerationService.Generators.ApplyPermanent(GeneratorBonusMult.CreateProfitMult(item.Multiplier));
//                } else {
//                    Services.GenerationService.Generators.ApplyGlobal(GeneratorBonusMult.CreateProfitMult(item.Multiplier));
//                }
//            } else {

//                if(item.IsPermanent) {

//                    GameServices.Instance.GenerationService.Generators.ApplyPermanent(item.ItemId, GeneratorBonusMult.CreateProfitMult(item.Multiplier));
//                }

//                ProfitMultInfo profitMult = new ProfitMultInfo($"profit_upgrade_{System.Guid.NewGuid().ToString().Substring(0, 5)}", item.Multiplier);
//                GameServices.Instance.GenerationService.Generators.ApplyProfit(item.ItemId, profitMult);
//            }

//            playerData.Save();
//        }*/
//    }

//    public interface IPlayerDataUpgrader : IGameService {
//        //void UpgradeTime(PlayerData playerData, UpgradeItem item);
//        //void UpgradeProfit(PlayerData playerData, UpgradeItem item);
//        void Upgrade(UpgradeItem item);
//    }

//    public class UpgradeItem {
//        public int ItemId { get; private set; }
//        public int Multiplier { get; private set; }
//        public bool IsPermanent { get; private set; }
//        public UpgradeType Type { get; private set; }

//        public bool IsGlobal
//            => ItemId < 0;

//        public UpgradeItem(int itemId, int multiplier, bool isPermanent, UpgradeType type) {
//            ItemId = itemId;
//            Multiplier = multiplier;
//            IsPermanent = isPermanent;
//            Type = type;
//        }
//    }
//}