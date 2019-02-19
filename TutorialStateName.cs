namespace Bos {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum TutorialStateName : int {
        None = 0,
        HelloText = 1,
        HintBuyRickshaw = 2,
        ClickGenerateRickshaw = 3,
        WaitForCashOnSecondRickshaw = 4,
        WaitCashForFirstManager = 5,
        MakeRollbackState = 6,
        BuyTaxi = 7,
        PlaySlot = 8,
        BuyUpgrade = 9,
        DailyBonus = 10,
        BuyBus = 11,
        MegaBoost = 12,

        [Obsolete("Don't used anymore")]
        TransferPersonalCash = 13,

        [Obsolete("Don't used anumore")]
        BuyPersonalProduct = 14,

        
        TransportInfoView = 15,
        Bank = 16,
        Moon = 17,
        PlanetTransport = 18,
        Report = 19,
        BreakLines = 20,
        Mars = 21,
        Teleport = 22,
        Mechanic = 23,
        SpaceShip = 24,
        ShowTransferAnsPersonalPurchasesState = 25,
        ShowUpgradeEfficiencyState = 26,
        BuyTransportCountState = 27,
        EnhanceManager = 1000
    }

}