namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ShopItemInfo : IShopItem {

        public int ItemId  { get; private set;}
        public int TargetId { get; private set; }
        public IPrice Price { get; private set; }

        public ShopItemInfo(int itemId, int targetId, IPrice price) {
            ItemId = itemId;
            TargetId = targetId;
            Price = price;
        }
    }

    public interface IShopItem {
        int ItemId { get; }
        int TargetId { get; }
        IPrice Price { get; }
    }

    public interface IPrice {
        object Value { get; }
        CoinType Type { get; }
    }

    public enum CoinType {
        Coin,
        Real
    }

    public class Price : IPrice {

        private Price() { }

        private Price(int value) {
            Value = value;
            Type = CoinType.Coin;
        }

        private Price(float value) {
            Value = value;
            Type = CoinType.Real;
        }

        public object Value {
            get;
            private set;
        }

        public CoinType Type {
            get;
            private set;
        }

        public static IPrice CreateCoins(int value) {
            return new Price(value);
        }

        public static IPrice CreateReal(float value) {
            return new Price(value);
        }
    }
}