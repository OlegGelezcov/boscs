namespace Bos
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Bos.Debug;
    using Bos.UI;
    using Newtonsoft.Json;
    using UniRx;
    using UnityEngine;
    using UDBG = UnityEngine.Debug;




    public abstract class TutorialState
    {

        public virtual bool IsCompleted { get; private set; }
        public virtual bool IsActive { get; private set; }
        protected bool IsOnEntered { get; set; } = false;
        public int Stage { get; private set; } = 0;

        private readonly Dictionary<int, List<string>> lockedNames = new Dictionary<int, List<string>>();

        protected void AddLocked(int stage, IEnumerable<string> names)
        {
            foreach (string name in names)
            {
                AddLocked(stage, name);
            }
        }

        public virtual void Setup(IBosServiceCollection services)
        {

        }

        protected void AddLocked(int stage, string lockedName)
        {
            if (lockedNames.ContainsKey(stage))
            {
                lockedNames[stage].Add(lockedName);
            }
            else
            {
                lockedNames.Add(stage, new List<string> { lockedName });
            }
        }

        public bool IsElementLocked(string lockedName)
        {
            if (IsActive)
            {
                if (lockedNames.ContainsKey(Stage))
                {
                    return lockedNames[Stage].Contains(lockedName);
                }
            }
            return false;
        }

        public void SetCompleted(IBosServiceCollection context, bool value)
        {
            bool oldCompleted = IsCompleted;
            IsCompleted = value;
            if (IsCompleted)
            {
                IsActive = false;
                if (oldCompleted != IsCompleted)
                {
                    OnExit(context);
                }
            }
            if (oldCompleted != IsCompleted)
            {
                GameEvents.OnTutorialStateCompletedChanged(this);
            }
            UnityEngine.Debug.Log($"Tutorial state completed: {Name}".Attrib(bold: true, italic: true, color: "g", size: 20));
        }

        public void ForceSetCompleted()
        {
            IsCompleted = true;
            IsActive = false;
        }

        public void ForceStart(IBosServiceCollection context)
        {
            IsCompleted = false;
            IsActive = true;
            Stage = 0;
            IsOnEntered = false;
            OnEnter(context);
        }

        public abstract TutorialStateName Name { get; }

        public abstract void OnEnter(IBosServiceCollection context);

        public abstract void OnUpdate(IBosServiceCollection context, float deltaTime);

        protected abstract void OnExit(IBosServiceCollection context);

        public abstract void OnEvent(IBosServiceCollection context, TutorialEventData data);

        public abstract bool IsValid(IBosServiceCollection context);

        private readonly Subject<int> tutorialStageSubject = new Subject<int>();

        public IObservable<int> TutorialStageObservable
            => tutorialStageSubject.AsObservable();

        public bool TryActivate(IBosServiceCollection context)
        {
            if (!IsActive && !IsCompleted)
            {
                if (IsValid(context))
                {
                    bool oldActive = IsActive;
                    IsActive = true;
                    if (oldActive != IsActive)
                    {
                        OnEnter(context);
                        GameEvents.OnTutorialStaticActivityChanged(this);
                        return true;
                    }
                }
            }
            return false;
        }

        protected void CompleteSelf(IBosServiceCollection context)
            => context.TutorialService.CompleteState(Name);

        public virtual TutorialStateSave GetSave()
        {
            return new TutorialStateSave
            {
                isActive = IsActive,
                isCompleted = IsCompleted,
                stateName = Name,
                stage = Stage
            };
        }

        public virtual void Load(TutorialStateSave save)
        {
            IsActive = save.isActive;
            IsCompleted = save.isCompleted;
            Stage = save.stage;
        }

        public virtual void Reset()
        {
            IsCompleted = false;
            IsActive = false;
            Stage = 0;
        }

        public void SetStage(int newStage)
        {
            int oldStage = Stage;
            Stage = newStage;
            if (oldStage != newStage)
            {
                OnStageChanged(oldStage, newStage);
                GameEvents.OnTutorialStateStageChanged(this, oldStage, Stage);
                tutorialStageSubject.OnNext(Stage);
            }
        }

        protected virtual void OnStageChanged(int oldStage, int newStage)
        {

        }

        public void SetStage(int newStage, float delay)
        {
            GameServices.Instance.Execute(() => {
                SetStage(newStage);
            }, delay);
        }

        protected bool IsStandardDialogCondition(IBosServiceCollection context)
        {
            return context.ViewService.ModalCount == 0 &&
                context.ViewService.LegacyCount == 0 &&
                (!context.ViewService.Exists(ViewType.TutorialDialogView)) &&
                context.GameModeService.GameModeName == GameModeName.Game;
        }

        protected bool IsStandardFingerCondition(IBosServiceCollection context)
        {
            return (!context.ViewService.Exists(ViewType.TutorialDialogView)) &&
                context.GameModeService.GameModeName == GameModeName.Game;
        }

        protected void ShowTutorialDialog(IBosServiceCollection context, TutorialDialogData data, System.Func<IBosServiceCollection, bool> additionalShowPredicate = null)
        {
            context.ViewService.Show(ViewType.TutorialDialogView, () => {
                return (additionalShowPredicate == null) ? IsStandardDialogCondition(context) : (IsStandardDialogCondition(context) && (additionalShowPredicate(context)));
            }, (viewObj) => {
                UDBG.Log($"opened dialog on state => {Name}".Colored(ConsoleTextColor.lightblue));
            }, new ViewData
            {
                UserData = data
            });
        }

        protected void ForceTutorialDialog(IBosServiceCollection context, TutorialDialogData data, System.Func<IBosServiceCollection, bool> additionalShowPredicate = null)
        {
            context.ViewService.Show(ViewType.TutorialDialogView, () => {
                return (additionalShowPredicate == null) || additionalShowPredicate(context);
            }, (viewObj) => {
                UDBG.Log($"opened dialog on state => {Name}".Colored(ConsoleTextColor.lightblue));
            }, new ViewData
            {
                UserData = data
            });
        }

        protected List<string> GetLocalizationStrings(IBosServiceCollection context, params string[] keys)
        {
            List<string> result = new List<string>();
            keys.ToList().ForEach(s => result.Add(context.ResourceService.Localization.GetString(s)));
            return result;
        }

        protected void FingerTimeoutAction(IBosServiceCollection context, string positionName, float timeout = -1, Action timeOutAction = null)
        {
            context.TutorialService.CreateFinger(positionName, new TutorialFingerData
            {
                Id = positionName,
                IsTooltipVisible = false,
                Timeout = timeout,
                TimeoutAction = timeOutAction
            });
        }

        protected void Finger(IBosServiceCollection context, string positionName, float timeout = -1)
        {
            context.TutorialService.CreateFinger(positionName, new TutorialFingerData
            {
                Id = positionName,
                IsTooltipVisible = false,
                Timeout = timeout
            });
        }

        protected void Finger(IBosServiceCollection context, string positionName, string tooltipText, Vector2 tooltipPosition, float tooltipScale, float tooltipWidth, float timeout = -1)
        {
            context.TutorialService.CreateFinger(positionName, new TutorialFingerData
            {
                Id = positionName,
                IsTooltipVisible = true,
                TooltipText = tooltipText,
                Position = Vector2.zero,
                TooltipPosition = tooltipPosition,
                TooltipScale = new Vector3(tooltipScale, tooltipScale, 1),
                TooltipWidth = tooltipWidth,
                Timeout = timeout
            });
        }

        protected void FingerDelayed(IBosServiceCollection context, string positionName, string tooltipText,
            Vector2 tooltipPosition, float tooltipScale, float tooltipWidth, float timeout = -1, Action onShow = null)
        {
            context.RunCoroutine(FingerDelayedImpl(context, positionName, tooltipText, tooltipPosition, tooltipScale, tooltipWidth, timeout, onShow));
        }

        private IEnumerator FingerDelayedImpl(IBosServiceCollection context, string positionName, string tooltipText,
            Vector2 tooltipPosition, float tooltipScale, float tooltipWidth, float timeout = -1, Action onShow = null)
        {
            yield return new WaitUntil(() => IsStandardFingerCondition(context));
            Finger(context, positionName, tooltipText, tooltipPosition, tooltipScale, tooltipWidth, timeout);
            onShow?.Invoke();
        }

        protected void RemoveFinger(IBosServiceCollection context, string id)
            => context.TutorialService.RemoveFinger(id);


        public virtual void OnRollbackLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel level)
        {

        }

        public virtual void OnEfficiencyLevelChanged(int oldLevel, int newLevel, ManagerEfficiencyRollbackLevel level)
        {

        }

        public abstract string GetValidationDescription(IBosServiceCollection services);

        protected StringBuilder GetBaseValidationDescription()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Name: {Name}, Is Active: {IsActive}, Is Completed: {IsCompleted}, Stage: {Stage}");
            stringBuilder.AppendLine("IsValid() conditions:");
            return stringBuilder;
        }

    }

    [System.Serializable]
    public class TutorialStateSave
    {
        public bool isCompleted;
        public bool isActive;
        [JsonConverter(typeof(Bos.Json.StringEnumConverter))]
        public TutorialStateName stateName;
        public int stage;
    }

    public static class TutorialStateExtensions
    {
        public static StringBuilder AppendTutorialStateCompletedCondition(this StringBuilder sb, TutorialStateName stateName)
        {
            sb.AppendLine($"Is {stateName} completed: {GameServices.Instance.TutorialService.IsStateCompleted(stateName)}");
            return sb;
        }
    }

}