using Bos;
using Bos.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MiniGamesScreen : TypedViewWithCloseButton
{
    public PrizeWheelContentView PrizeWheelContentView;
    public BreakLinerContentView BreakLinerContentView;
    public TreasureHuntContentView TreasureHuntContentView;
    
    
    public Button WheelPageButton, TresurePageButton, BreakLinerPageButton;

    public Sprite raceAvailableIconSprite;
    public Sprite raceNotAvailableIconSprite;
    public RaceUnavailableView raceUnavailableView;
    public Image tabMoonImage;
    public Image raceTabCarIconImage;
    
    public GameObject[] prizeWheelObjects;
    public GameObject[] treasureObjects;
    public GameObject[] breakLinerObjects;



    public override void OnEnable() {
        base.OnEnable();
        IPlanetService planetService = Services.GetService<IPlanetService>();
        ISoundService soundService = Services.GetService<ISoundService>();

        WheelPageButton.SetListener(() => {
            
            treasureObjects.Deactivate();
            raceUnavailableView.Deactivate();
            breakLinerObjects.Deactivate();
            
            prizeWheelObjects.Activate();
            soundService.PlayOneShot(SoundName.click);
        });

        BreakLinerPageButton.SetListener(() => {
            if(planetService.IsMoonOpened) {
                prizeWheelObjects.Deactivate();
                treasureObjects.Deactivate();
                breakLinerObjects.Activate();
                raceUnavailableView.Deactivate();
            }
            else
            {
                prizeWheelObjects.Deactivate();
                treasureObjects.Deactivate();
                breakLinerObjects.Deactivate();
                raceUnavailableView.Activate();
            }

            soundService.PlayOneShot(SoundName.click);
        });
        
        TresurePageButton.SetListener(() => {
            prizeWheelObjects.Deactivate();
            treasureObjects.Activate();
            raceUnavailableView.Deactivate();
            breakLinerObjects.Deactivate();
            soundService.PlayOneShot(SoundName.click);
            GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.ChestTabOpened));
        });

        closeButton.SetListener(() => {
            Services.ViewService.Remove(Type);
            Services.SoundService.PlayOneShot(SoundName.click);
        });
        
        GameEvents.PlanetStateChanged += OnPlanetStateChanged;
        GameEvents.OnTutorialEvent(new TutorialEventData(TutorialEventName.MiniGameOpened));
    }

    private void UpdateTabState(IPlanetService planetService) {
        //Now rocket game available also on Earth
        if (planetService.IsMoonOpened ) {
            raceTabCarIconImage.overrideSprite = raceAvailableIconSprite;
            tabMoonImage.Deactivate();
        } else {
            raceTabCarIconImage.overrideSprite = raceNotAvailableIconSprite;
            tabMoonImage.Activate();
        }
    }

    public override void OnDisable() {
        GameEvents.PlanetStateChanged -= OnPlanetStateChanged;
        base.OnDisable();
    }

    private void OnPlanetStateChanged(PlanetState oldState, PlanetState newState, PlanetInfo planetInfo ) {
        IPlanetService planetService = Services.GetService<IPlanetService>();
        
        if(planetService.IsMoonOpened) {
            //UpdateTabState(planetService);
            if(raceUnavailableView.gameObject.activeSelf) {
                raceUnavailableView.Deactivate();
                prizeWheelObjects.Deactivate();
                treasureObjects.Activate();
            }
        }
    }


    public override void Start()
    {
        GlobalRefs.MiniGamesScreen = this;
    }

    public void IncreaseTriesPaid(MiniGameType type)
    {
        switch (type)
        {
            case MiniGameType.PrizeWheel: PrizeWheelContentView.IncreaseTriesPaid();
                break;
            case MiniGameType.BreakLiner: BreakLinerContentView.IncreaseTriesPaid();
                break;
            case MiniGameType.TreasureHunt: TreasureHuntContentView.IncreaseTriesPaid();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public override CanvasType CanvasType => CanvasType.UI;
    public override bool IsModal => true;
    public override ViewType Type => ViewType.MiniGameView;
}

public enum MiniGameType
{
    PrizeWheel,
    BreakLiner,
    TreasureHunt
}
