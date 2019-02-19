using UnityEditor;
using UnityEngine.UI;

namespace Bos.UI {
    
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ModulePart : GameBehaviour {

        public int moduleId;
        public Material baseMaterial;
        public Image image;
        public Vector2 hidedPosition;
        public Vector2 visiblePosition;

        private bool isAnimating = false;
        public ModulePartState State { get; private set; } = ModulePartState.Hided;


        private void Setup() {
            bool isOpened = Services.Modules.IsOpened(moduleId);
            image.material =
                Services.ResourceService.Materials.GetMaterial($"module_{moduleId}", baseMaterial);
            
            ResetImageMaterial();
            
            if (isOpened) {
                StartCoroutine(AnimateToOpenedStateImpl());
            }
        }

        private IEnumerator AnimateToOpenedStateImpl() {
            yield return new WaitForSeconds(0.1f + 0.1f * moduleId);
            AnimateToOpenedState();
        }

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.ShipModuleStateChanged += OnShipModuleStateChanged;
            Setup();
        }

        public override void OnDisable() {
            GameEvents.ShipModuleStateChanged -= OnShipModuleStateChanged;
            base.OnDisable();
        }

        private void OnShipModuleStateChanged(ShipModuleState oldState, ShipModuleState newState,
            ShipModuleInfo module) {
            if (module.Id >= moduleId) {
                ResetImageMaterial();
                AnimateToOpenedState();
            }
        }

        private void ResetImageMaterial() {
            Material material = image.material;
            material.SetFloat("_ColorSum", 4);
            material.SetFloat("_Enabled", 1);
            material.SetFloat("_MinAlpha", 1f);
            material.SetFloat("_TotalMult", 1.0f);
            material.SetFloat("_BorderAlpha", 1);
        }

        private void SetImageMaterialToOpenedState() {
            Material material = image.material;
            material.SetFloat("_ColorSum", 0);
            material.SetFloat("_Enabled", 0);
        }

        private void AnimateToOpenedState() {
            Material material = image.material;
            FloatAnimator animator = image.gameObject.GetOrAdd<FloatAnimator>();
            int colorSumId = Shader.PropertyToID("_ColorSum");

            animator.AnimateModuleToVisibleState(image, 2.0f);

            /*
            animator.StartAnimation(new FloatAnimationData() {
                StartValue = 4,
                EndValue = 0,
                Duration = 2,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = image.gameObject,
                OnStart = (v, o) => material.SetFloat(colorSumId, v),
                OnUpdate = (v, t, o) => material.SetFloat(colorSumId, v),
                OnEnd = (v, o) => {
                    material.SetFloat(colorSumId, v);
                    material.SetFloat("_Enabled", 0);
                }
            }); */
        }

        private float GetAnimateIntervalForPosition(Vector2 position, Vector2 targetPosition, float totalInterval) {
            float totalDistance = (hidedPosition - visiblePosition).magnitude;
            float currentDistance = (position - targetPosition).magnitude;
            float speed = totalDistance / totalInterval;
            float currentInterval = currentDistance / speed;
            if(currentInterval > totalInterval ) {
                currentInterval = totalInterval;
            }
            return currentInterval;
        }

        public void LerpPosition(float t) {
            if (!isAnimating) {
                RectTransform rectTransform = GetComponent<RectTransform>();
                switch (State) {
                    case ModulePartState.Hided: {
                            rectTransform.anchoredPosition = Vector2.Lerp(hidedPosition, visiblePosition, t);
                        }
                        break;
                    case ModulePartState.Visible: {
                            rectTransform.anchoredPosition = Vector2.Lerp(visiblePosition, hidedPosition, t);
                        }
                        break;
                }
            }
        }

        public void MoveToVisible(float totalInterval) {
            //if(State == ModulePartState.Hided ) {
                StartCoroutine(MoveToVisibleImpl(totalInterval));
            //}
        }

        public void MoveToHided(float totalInterval ) {
            //if(State == ModulePartState.Visible ) {
                StartCoroutine(MoveToHidedImpl(totalInterval));
            //}
        }

        private IEnumerator MoveToVisibleImpl(float totalInterval) {
            yield return new WaitUntil(() => !isAnimating);
            isAnimating = true;
            Vector2Animator positionAnimator = gameObject.GetOrAdd<Vector2Animator>();
            RectTransform rectTransform = GetComponent<RectTransform>();
            positionAnimator.StartAnimation(new Vector2AnimationData {
                StartValue = rectTransform.anchoredPosition,
                EndValue = visiblePosition,
                AnimationMode = BosAnimationMode.Single,
                Duration = GetAnimateIntervalForPosition(rectTransform.anchoredPosition, visiblePosition, totalInterval),
                EaseType = EaseType.EaseInOutQuintic,
                Target = gameObject,
                OnStart = rectTransform.UpdatePositionFunctor(),
                OnUpdate = rectTransform.UpdatePositionTimedFunctor(),
                OnEnd = rectTransform.UpdatePositionFunctor(() => {
                    isAnimating = false;
                    State = ModulePartState.Visible;
                    //Sounds.PlayOneShot(SoundName.click);
                })
            });
        }

        private IEnumerator MoveToHidedImpl(float totalInterval ) {
            yield return new WaitUntil(() => !isAnimating);
            isAnimating = true;
            Vector2Animator positionAnimator = gameObject.GetOrAdd<Vector2Animator>();
            RectTransform rectTransform = GetComponent<RectTransform>();
            positionAnimator.StartAnimation(new Vector2AnimationData {
                StartValue = rectTransform.anchoredPosition,
                EndValue = hidedPosition,
                AnimationMode = BosAnimationMode.Single,
                Duration = GetAnimateIntervalForPosition(rectTransform.anchoredPosition, hidedPosition, totalInterval),
                EaseType = EaseType.EaseInOutQuintic,
                Target = gameObject,
                OnStart = rectTransform.UpdatePositionFunctor(),
                OnUpdate = rectTransform.UpdatePositionTimedFunctor(),
                OnEnd = rectTransform.UpdatePositionFunctor(() => {
                    isAnimating = false;
                    State = ModulePartState.Hided;
                })
            });
        }

    }

    public enum ModulePartState {
        Visible,
        Hided
    }

    public static class ModuleViewExtensions
    {
        private const string COLOR_SUM_PROP = "_ColorSum";
        private const string ENABLED_PROP = "_Enabled";
        private const string MIN_ALPHA_PROP = "_MinAlpha";
        private const string TOTAL_MULT_PROP = "_TotalMult";
        private const string BORDER_ALPHA_PROP = "_BorderAlpha";

        public static void SetModuleToInvisible(this Image image )
        {
            if(image.material == null )
            {
                Debug.Log($"image: {image.name} doesn't have material".Attrib(bold: true, color: "red"));
                return;
            }
            image.material.SetModuleToInvisible();

        }

        public static void SetModuleToVisible(this Image image )
        {
            if (image.material == null)
            {
                Debug.Log($"image: {image.name} doesn't have material".Attrib(bold: true, color: "red"));
                return;
            }
            image.material.SetModuleToVisible();
        }

        public static void SetModuleToInvisible(this Material material )
        {
            SetVisibiltyValuesOnMaterial(material, enabled: 1.0f, colorSum: 4);
        }

        public static void SetModuleToVisible(this Material material )
        {
            SetVisibiltyValuesOnMaterial(material, enabled: 0.0f, colorSum: 0.0f);

        }

        private static void SetVisibiltyValuesOnMaterial(Material material, float enabled, float colorSum )
        {
            material.SetFloat(COLOR_SUM_PROP, colorSum);
            material.SetFloat(ENABLED_PROP, enabled);
            material.SetFloat(MIN_ALPHA_PROP, 1f);
            material.SetFloat(TOTAL_MULT_PROP, 1.0f);
            material.SetFloat(BORDER_ALPHA_PROP, 1);
        }

        public static void AnimateModuleToVisibleState(this Image image, float duration = 1.0f)
            => image.gameObject.GetOrAdd<FloatAnimator>().AnimateModuleToVisibleState(image, duration);

        public static void AnimateModuleToInvisibleState(this Image image, float duration = 1.0f)
            => image.gameObject.GetOrAdd<FloatAnimator>().AnimateModuleToInvisibleState(image, duration);

        public static void PingPongModuleVisibility(this Image image, float duration = 1.0f)
            => image.gameObject.GetOrAdd<FloatAnimator>().PingPongModuleVisibility(image, duration);


        public static void PingPongModuleVisibility(this FloatAnimator animator, Image image, float duration = 1.0f )
        {
            Material imageMaterial = image.material;
            if (imageMaterial == null)
            {
                Debug.LogError($"image: {image.name} doesn't have any material");
                return;
            }

            int propertyId = Shader.PropertyToID(COLOR_SUM_PROP);

            animator.StartAnimation(new FloatAnimationData
            {
                StartValue = 4.0f,
                EndValue = 0.0f,
                Duration = duration,
                AnimationMode = BosAnimationMode.PingPong,
                EaseType = EaseType.EaseInOutQuad,
                Target = image.gameObject,
                OnStart = (value, obj) =>
                {
                    imageMaterial.SetFloat(propertyId, value);
                    imageMaterial.SetFloat(ENABLED_PROP, 1.0f);
                },
                OnUpdate = (value, time, obj) => imageMaterial.SetFloat(propertyId, value),
                OnEnd = (value, obj) =>
                {
                    imageMaterial.SetFloat(propertyId, value);
                    imageMaterial.SetFloat(ENABLED_PROP, 1.0f);
                }
            });
        }

        public static void AnimateModuleToVisibleState(this FloatAnimator animator, Image image, float duration = 1.0f)
        {
            Material imageMaterial = image.material;
            if(imageMaterial == null )
            {
                Debug.LogError($"image: {image.name} doesn't have any material");
                return;
            }

            int propertyId = Shader.PropertyToID(COLOR_SUM_PROP);
            animator.StartAnimation(new FloatAnimationData
            {
                StartValue = 4.0f,
                EndValue = 0.0f,
                Duration = duration,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = image.gameObject,
                OnStart = (value, obj) => 
                {
                    imageMaterial.SetFloat(propertyId, value);
                    imageMaterial.SetFloat(ENABLED_PROP, 1.0f);
                },
                OnUpdate = (value, time, obj) => imageMaterial.SetFloat(propertyId, value),
                OnEnd = (value, obj) =>
                {
                    imageMaterial.SetFloat(propertyId, value);
                    imageMaterial.SetFloat(ENABLED_PROP, 0.0f);
                }
            });
        }

        public static void AnimateModuleToInvisibleState(this FloatAnimator animator, Image image, float duration = 1.0f )
        {
            Material imageMaterial = image.material;
            if (imageMaterial == null)
            {
                Debug.LogError($"image: {image.name} doesn't have any material");
                return;
            }

            int propertyId = Shader.PropertyToID(COLOR_SUM_PROP);
            animator.StartAnimation(new FloatAnimationData
            {
                StartValue = 0.0f,
                EndValue = 4.0f,
                Duration = duration,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = image.gameObject,
                OnStart = (value, obj) =>
                {
                    imageMaterial.SetFloat(propertyId, value);
                    imageMaterial.SetFloat(ENABLED_PROP, 1.0f);
                },
                OnUpdate = (value, time, obj) => imageMaterial.SetFloat(propertyId, value),
                OnEnd = (value, obj) =>
                {
                    imageMaterial.SetFloat(propertyId, value);
                    imageMaterial.SetFloat(ENABLED_PROP, 1.0f);
                }
            });
        }
    }
}