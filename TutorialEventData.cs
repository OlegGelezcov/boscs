namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum TutorialEventName {
        GameModeChanged,
        UnitCountChanged,
        GenerationButtonClicked,
        CopanyCashChanged,
        ViewOpened,
        ManagementViewOpenedForManager,
        ManagerHired,
        TutorialPositionObjectActivated,
        ManagerRollback,
        MiniGameOpened,
        PlayCasinoClicked,
        UpgradePurchased,
        CashUpgradesOpened,
        DailyBonusOpened,
        GeneratorResearched,
        MegaBoostActivated,
        TransferTabOpened,
        IllegalTransferCompleted,
        LegalTransferCompleted,

        StatusGoodsOpened,
        OpenPlanetClicked,
        SecretariesOpened,
        BreakLinesOpened,
        MechanicOpened,
        MegaboostStateChanged,
        PersonalProductPurchased,
        ViewHided,
        WheelCompleted,
        ChestTabOpened,
        ChestOpened
    }

    public class TutorialEventData {

        public TutorialEventData(TutorialEventName eventName, object userData = null) {
            EventName = eventName;
            UserData = userData;

           
        }

        public TutorialEventName EventName { get; private set; }
        public object UserData { get; private set; }
    }

}