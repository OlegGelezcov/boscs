using Bos;
using Bos.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Purchasing;

public class LevelTracker : GameBehaviour {
    public Sprite LevelUpIcon;
    public AchievementTracker AchiTracker;

    public override void OnEnable() {
        base.OnEnable();
        GameEvents.GeneratorLevelUpdated += OnGeneratorLevelUpdated;
    }

    public override void OnDisable() {
        GameEvents.GeneratorLevelUpdated -= OnGeneratorLevelUpdated;
        base.OnDisable();
    }

    private void OnGeneratorLevelUpdated(int generatorId ) {
        UpdateState();
    }

    private IAchievmentServcie achievmentService;

    private IAchievmentServcie AchievmentService {
        get {
            if(achievmentService == null ) {
                achievmentService = GameServices.Instance.GetService<IAchievmentServcie>();
            }
            return achievmentService;
        }
    }

    private void LogBonusLevel(Level level) {
        var gameServices = GameServices.Instance;
        var consoleService = gameServices?.GetService<IConsoleService>();
        consoleService?.AddOutput(level.ToString(), ConsoleTextColor.grey, isDuplicateAtUnityLog: true);

    }

    private void UpdateState() {
        AchievmentService?.UpdateAchievments(Services.GenerationService.Generators.Generators.Values.ToArray());
    }

}
