namespace Bos.UI
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.SceneManagement;
	using UnityEngine.UI;

    public class LoadingView : TypedView
    {
		public float fadeInterval = 0.3f;

        public override ViewType Type => ViewType.LoadingView;

        public override CanvasType CanvasType => CanvasType.UI;

		public override int ViewDepth => 100;

		public override bool IsModal =>  true;
		
		private LoadSceneData data;

		public override void Setup(ViewData indata) {
			data = indata.UserData as LoadSceneData;
			if(data == null ) {
				throw new UnityException($"Invalid parameter type to {nameof(LoadingView)}");
			}

			Image image = GetComponent<Image>();

			ColorAnimationData dataIn = new ColorAnimationData{
				StartValue = new Color(0, 0, 0, 0),
				EndValue = new Color(0, 0, 0, 1),
				Duration = fadeInterval,
				Target = gameObject,
				AnimationMode = BosAnimationMode.Single,
				EaseType = EaseType.EaseInOutQuad,
				OnStart = (c, go) => image.color = c,
				OnUpdate = (c, t, go) => image.color = c,
				OnEnd = (c, go) => { 
					image.color = c; 
					StartCoroutine(LoadImpl());
				}
			};
			GetComponent<ColorAnimator>().StartAnimation(dataIn);
		}

		private IEnumerator LoadImpl() {
			var operation = SceneManager.LoadSceneAsync(data.BuildIndex, data.Mode);
			yield return operation;
			if(data.Mode == LoadSceneMode.Additive) {
				SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(data.BuildIndex));
			}
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            data.LoadAction?.Invoke();
			Image image = GetComponent<Image>();
			ColorAnimationData dataOut = new ColorAnimationData {
				StartValue = new Color(0, 0, 0, 1),
				EndValue = new Color(0, 0, 0, 0),
				Duration = fadeInterval,
				EaseType = EaseType.EaseInOutQuad,
				Target = gameObject,
				OnStart = (c, go) => image.color = c,
				OnUpdate = (c, t, go) => image.color = c,
				OnEnd = (c, go) => {
					image.color = c;
					Services.ViewService.Remove(Type);
				}
			};
			GetComponent<ColorAnimator>().StartAnimation(dataOut);
		}
    }

	public class LoadSceneData {
		public int BuildIndex { get; set; }

        /*
        public string SceneName {
            set {
                BuildIndex = SceneManager.GetSceneByName(value).buildIndex;
            }
            get {
                return SceneManager.GetSceneByBuildIndex(BuildIndex).name;
            }
        }*/

		public LoadSceneMode Mode { get; set;}

		public System.Action LoadAction { get; set; }
	}
}

