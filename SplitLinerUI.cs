namespace Bos.UI {
    using Bos.Data;
    using Bos.SplitLiner;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class SplitLinerUI : GameBehaviour {

        private const float kProbabilityOfCoin = 0.05f;

        public Button playButton;
        public FloatAnimator gameNameAnimator;
        public GameObject percentPrefab;
        public GameObject coinPrefab;

        public RectTransform canvasTransform;
        public Canvas canvas;
        public Transform playerTransform;
        public Text scoreText;
        public SplitLinerCompletedView rewardView;
        public SplitFooterView footerView;



        private int cashPercent = 0;
        private int coinCount = 0;

        private double startCash = 0;
        private int rewardForBlock;
        private const int scorePerBlock = 5;

        private IRocketData rocketData;
        private int coinIndex = 0;

        public override void Start() {
            base.Start();
            //for test mode
            if(Services == null  ) {
                Setup(0);
            }
        }

        public void Setup(double startCash) {

            cashPercent = 0;
            coinCount = 0;
            this.startCash = startCash;
            playButton.interactable = true;
            FindObjectOfType<PlayerMotor>()?.enableDisableButton(false);
            OnCoinsCountChanged(0);
            OnCashPercentChanged(0);
            StartCoroutine(ShowFooterImpl());

            if(Services == null) {
                rocketData = new MoqRocketData();
            } else {
                rocketData = ResourceService.RocketData;
            }
        }

        public override void OnEnable() {
            base.OnEnable();
            SplitLinerGameManager.YellowBlockHitted += OnYellowBlockHitted;
            SplitLinerGameManager.GreenBlockHitted += OnGreenBlockHitted;
            SplitLinerGameManager.GameEnd += OnGameEnd;
        }

        public override void OnDisable() {
            SplitLinerGameManager.YellowBlockHitted -= OnYellowBlockHitted;
            SplitLinerGameManager.GreenBlockHitted -= OnGreenBlockHitted;
            SplitLinerGameManager.GameEnd -= OnGameEnd;
            base.OnDisable();

        }


        private void OnYellowBlockHitted() {
                    
            var coin = Services.SplitService.GetCurrentUpgrade().coin;
            AddCoins(coin);
            SpawnCoinObject(coin);
        }
        
        private void OnGreenBlockHitted()
        {
            var cash = (int)(Services.SplitService.GetCurrentUpgrade().cash * 100);
            AddCashPercent(cash);
        }

        private void OnGameEnd(bool success) {
            rewardView.Activate();
            rewardView.Setup(startCash * (cashPercent / 100.0f), coinCount, success);
        }

        private void SpawnCoinObject(int coin) {
            GameObject coinObject = Instantiate<GameObject>(coinPrefab);
            var coinView = coinObject.GetComponent<SplitLinerCoin>();
            coinView.CoinText.text = $"+{coin}";
            
            RectTransform coinRectTransform = coinObject.GetComponent<RectTransform>();
            coinRectTransform.SetParent(canvasTransform, false);

            coinRectTransform.anchoredPosition = Services?.ViewService.Utils.WorldPosToCanvasPos(Camera.main, canvas, playerTransform.position) ?? Vector2.zero;

            FloatAnimator floatAnimator = coinObject.GetComponent<FloatAnimator>();
            Vector3Animator vec3Animator = coinObject.GetComponent<Vector3Animator>();

            CanvasGroup canvasGroup = coinObject.GetComponent<CanvasGroup>();

            FloatAnimationData alphaAnimData = new FloatAnimationData {
                StartValue = 1,
                EndValue = 0,
                Duration = 0.4f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = coinObject,
                OnStart = (v, o) => canvasGroup.alpha = v,
                OnUpdate = (v, t, o) => canvasGroup.alpha = v,
                OnEnd = (v, o) => {
                    canvasGroup.alpha = v;
                    Destroy(coinObject);
                }
            };

            Vector3AnimationData scaleAnimData = new Vector3AnimationData {
                StartValue = Vector3.one,
                EndValue = 1.4f * Vector3.one,
                Duration = 0.3f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = coinObject,
                OnStart = (s, o) => coinRectTransform.localScale = s,
                OnUpdate = (s, t, o) => coinRectTransform.localScale = s,
                OnEnd = (s, o) => {
                    coinRectTransform.localScale = s;
                    floatAnimator.StartAnimation(alphaAnimData);
                }
            };

            vec3Animator.StartAnimation(scaleAnimData);
        }

        private void SpawnPercentText(int count) {
            GameObject textInstance = Instantiate<GameObject>(percentPrefab);
            RectTransform textRectTransform = textInstance.GetComponent<RectTransform>();
            textRectTransform.SetParent(canvasTransform, false);
            
            textRectTransform.anchoredPosition = Services?.ViewService.Utils.WorldPosToCanvasPos(Camera.main, canvas, playerTransform.position) ?? Vector2.zero;

            TMPro.TextMeshProUGUI txt = textRectTransform.GetComponent<TMPro.TextMeshProUGUI>();
            txt.text = $"+{count}%";

            FloatAnimator floatAnimator = txt.GetComponent<FloatAnimator>();
            Vector3Animator vec3Animator = txt.GetComponent<Vector3Animator>();

            FloatAnimationData alphaAnimData = new FloatAnimationData {
                StartValue = 1,
                EndValue = 0,
                Duration = 0.4f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = textRectTransform.gameObject,
                OnStart = (a, o) => {
                    txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, a);
                },
                OnUpdate = (a, t, o) => txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, a),
                OnEnd = (a, o) => {
                    txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, a);
                    Destroy(textRectTransform.gameObject);
                }
            };

            Vector3AnimationData scaleAnimData = new Vector3AnimationData {
                StartValue = Vector3.one,
                EndValue = 1.4f * Vector3.one,
                Duration = 0.3f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                Target = textRectTransform.gameObject,
                OnStart = (s, o) => textRectTransform.localScale = s,
                OnUpdate = (s, t, o) => textRectTransform.localScale = s,
                OnEnd = (s, o) => {
                    textRectTransform.localScale = s;
                    floatAnimator.StartAnimation(alphaAnimData);
                }
            };
            vec3Animator.StartAnimation(scaleAnimData);

        }

        public void SetupWithDelay(double startCash, float delay) {
            StartCoroutine(SetupWithDelayImpl(startCash, delay));
        }

        private IEnumerator SetupWithDelayImpl(double startCash, float delay) {
            yield return new WaitForSeconds(delay);
            Setup(startCash);
        }

        private IEnumerator ShowFooterImpl() {
            yield return new WaitForSeconds(1.0f);
            footerView.ToggleVisibility(true);
        }



        private void AnimateScoreText() {
            var animator = scoreText.gameObject.GetOrAdd<Vector3Animator>();
            RectTransform rect = scoreText.GetComponent<RectTransform>();

            var secondScale = new Vector3AnimationData {
                StartValue = 1.5f * Vector3.one,
                EndValue = Vector3.one,
                Duration = 0.5f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                OnStart = (s, o) => rect.localScale = s,
                OnUpdate = (s, t, o) => rect.localScale = s,
                OnEnd = (s, o) => rect.localScale = s,
                Target = rect.gameObject
            };

            var firstScale = new Vector3AnimationData {
                StartValue = Vector3.one,
                EndValue = 1.5f * Vector3.one,
                Duration = 0.5f,
                AnimationMode = BosAnimationMode.Single,
                EaseType = EaseType.EaseInOutQuad,
                OnStart = (s, o) => rect.localScale = s,
                OnUpdate = (s, t, o) => rect.localScale = s,
                OnEnd = (s, o) => {
                    rect.localScale = s;
                    animator.StartAnimation(secondScale);
                },
                Target = rect.gameObject
            };
            animator.StartAnimation(firstScale);
        }

        private void AddCashPercent(int count) {
            cashPercent += count;
            OnCashPercentChanged(cashPercent);
        }

        private void AddCoins(int count) {
            coinCount += count;
            OnCoinsCountChanged(coinCount);
        }


        private void OnCashPercentChanged(int newCashPercent)
            => CashPercentChanged?.Invoke(newCashPercent);
        private void OnCoinsCountChanged(int newCoinsCount)
            => CoinsCountChanged?.Invoke(newCoinsCount);

        public static event System.Action<int> CashPercentChanged;
        public static event System.Action<int> CoinsCountChanged;


    }

}