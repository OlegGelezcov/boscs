namespace Bos {
    using Bos.Data;
    using Bos.Debug;
    using Ozh.Tools.Functional;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UDBG = UnityEngine.Debug;

    public static class BosExtensions  {

        #region String manipulations
        public static string Colored(this string str, string color) 
            => $"<color={color}>{str}</color>";

        public static string Size(this string str, int size)
            => $"<size={size}>{str}</size>";

        public static string Colored(this string str, ConsoleTextColor color) 
            => str.Colored(color.ToString());

        public static string Attrib(this string str, 
            bool bold = false, 
            string color = "", 
            bool italic = false, 
            int size = 0) {
            if(bold) {
                str = str.Bold();
            }
            if(italic) {
                str = str.Italic();
            }
            if(size > 0 ) {
                str = str.Size(size);
            }
            if(!string.IsNullOrEmpty(color.Trim())) {
                string cleanedColorText = color.Trim().ToLower();
                if(cleanedColorText.StartsWith("#")) {
                    str = str.Colored(cleanedColorText);
                } else {
                    str = str.Colored(ColorNameToHex(cleanedColorText));
                }
            }
            return str;
        }

        private static readonly Dictionary<string, string> colorNameHexMap =
            new Dictionary<string, string> {
                ["aqua"] = "#00ffffff",
                ["cyan"] = "#00ffffff",
                ["black"] = "#000000ff",
                ["blue"] = "#0000ffff",
                ["brown"] = "#a52a2aff",
                ["darkblue"] = "#0000a0ff",
                ["fuchsia"] = "#ff00ffff",
                ["magenta"] = "#ff00ffff",
                ["green"] = "#008000ff",
                ["grey"] = "#808080ff",
                ["lightblue"] = "#add8e6ff",
                ["lime"] = "#00ff00ff",
                ["maroon"] = "#800000ff",
                ["navy"] = "#000080ff",
                ["olive"] = "#808000ff",
                ["orange"] = "#ffa500ff",
                ["purple"] = "#800080ff",
                ["red"] = "#ff0000ff",
                ["silver"] = "#c0c0c0ff",
                ["teal"] = "#008080ff",
                ["white"] = "#ffffffff",
                ["yellow"] = "#ffff00ff"
            };

        private static string ColorNameToHex(string name) {
            string lowerName = name.ToLower().Trim();
            if (lowerName.StartsWith("#")) {
                return lowerName;
            }
            if(colorNameHexMap.ContainsKey(lowerName)) {
                return colorNameHexMap[lowerName];
            }
            switch (lowerName) {
                case "w": return colorNameHexMap["white"];
                case "a": return colorNameHexMap["aqua"];
                case "c": return colorNameHexMap["cyan"];
                case "g": return colorNameHexMap["green"];
                case "lb": return colorNameHexMap["lightblue"];
                case "b": return colorNameHexMap["blue"];
                case "r": return colorNameHexMap["red"];
                case "y": return colorNameHexMap["yellow"];
            }
            return "#ffff00ff";
        }

        public static string Bold(this string str) => $"<b>{str}</b>";

        public static string Italic(this string str) => $"<i>{str}</i>";

        public static string BoldItalic(this string str) => str.Bold().Italic();

        public static bool IsNonEmpty(this string str)
            => !string.IsNullOrEmpty(str);


        public static int AsInt(this string str, int defaultValue = 0) {
            if(!string.IsNullOrEmpty(str)) {
                int val = 0;
                if(int.TryParse(str, out val)) {
                    return val;
                }
            }
            return defaultValue;
        }

        public static float AsFloat(this string str, float defaultValue = 0f) {
            if(!string.IsNullOrEmpty(str)) {
                float val = 0f;
                if(float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out val)) {
                    return val;
                }
             }
            return defaultValue;
        }

        public static double AsDouble(this string str, double defaultValue = 0 ) {
            if(str.IsValid()) {
                double val = 0.0;
                if(double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out val)) {
                    return val;
                }
            }
            return defaultValue;
        }

        public static bool AsBool(this string str, bool defaultValue = false) {
            if(!string.IsNullOrEmpty(str)) {
                bool val = false;
                if(bool.TryParse(str, out val)) {
                    return val;
                }

                int iVal = 0;
                if(int.TryParse(str, out iVal)) {
                    return (iVal != 0);
                }
            }
            return defaultValue;
        }

        public static string Format(this double value, string format) {
            return string.Format(format, value);
        }

        public static bool IsValid(this string str)
            => !string.IsNullOrEmpty(str);


        #endregion

        public static CurrencyNumber ToCurrencyNumber(this double d) {
            return new CurrencyNumber(d);
        }

        public static void SetListener(this Button button, UnityAction action) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        public static void RemoveListeners(this IEnumerable<Toggle> toggles) {
            foreach (Toggle toggle in toggles) {
                toggle.RemoveListeners();
            }
        }

        public static void RemoveListeners(this Toggle toggle)
            => toggle.onValueChanged.RemoveAllListeners();



        public static void SetListener(this Toggle toggle, UnityAction<bool> action) {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(action);
        }

        public static UnityAction WithClickSound(this UnityAction action)
            => () => {
                action?.Invoke();
                GameServices.Instance.SoundService.PlayOneShot(SoundName.click);
            };

        public static void SetPointerListeners(this EventTrigger trigger, 
            UnityAction<BaseEventData> onDown = null, 
            UnityAction<BaseEventData> onClick = null, 
            UnityAction<BaseEventData> onUp = null) {
            trigger.triggers.Clear();

            if(onDown != null ) {
                EventTrigger.TriggerEvent onDownEvent = new EventTrigger.TriggerEvent();
                onDownEvent.AddListener(onDown);
                trigger.triggers.Add(new EventTrigger.Entry {
                    eventID = EventTriggerType.PointerDown,
                    callback = onDownEvent
                });
            }

            if(onClick != null ) {
                EventTrigger.TriggerEvent onClickEvent = new EventTrigger.TriggerEvent();
                onClickEvent.AddListener(onClick);
                trigger.triggers.Add(new EventTrigger.Entry {
                    eventID = EventTriggerType.PointerClick,
                    callback = onClickEvent
                });
            }

            if(onUp != null ) {
                EventTrigger.TriggerEvent onUpEvent = new EventTrigger.TriggerEvent();
                onUpEvent.AddListener(onUp);
                trigger.triggers.Add(new EventTrigger.Entry {
                    eventID = EventTriggerType.PointerUp,
                    callback = onUpEvent
                });
            }
        }

        public static uint ToJenkins(this string str) {
            return BosUtils.JenkinsOneAtATimeHash(str);
        }

        //public static CurrencyNumber ToCurrencyNumber(this double value) {
        //    return (CurrencyNumber)value;
        //}


        public static void ToggleActivity(this GameObject gameObject, System.Func<bool> predicate) {
            bool isActive = predicate();
            gameObject.ToggleActivity(isActive);
        }

        public static void ToggleActivity(this GameObject gameObject, bool isActivate) {
            if(isActivate) {
                gameObject.Activate();
            } else {
                gameObject.Deactivate();
            }
        }

        //GameObject and MonoBehaviour extensions....
        public static GameObject Deactivate(this GameObject gameObject) {
            if (gameObject) {
                if (gameObject.activeSelf) {
                    gameObject.SetActive(false);
                }
            }
            return gameObject;
        }

        public static GameObject Activate(this GameObject gameObject ) {
            if(gameObject) {
                if(false == gameObject.activeSelf) {
                    gameObject.SetActive(true);
                }
            }
            return gameObject;
        }

        public static GameObject[] Activate(this GameObject[] objs) {
            foreach(var obj in objs ) {
                obj.Activate();
            }
            return objs;
        }

        public static GameObject[] Deactivate(this GameObject[] objs) {
            foreach(var obj in objs) {
                obj.Deactivate();
            }
            return objs;
        }

        public static void DeactivateEnumerable<T>(this IEnumerable<T> behaviours) where T : MonoBehaviour
        {
            foreach(T b in behaviours)
            {
                b.Deactivate();
            }
        }

        public static void ActivateEnumerable<T>(this IEnumerable<T> behaviours) where T : MonoBehaviour
        {
            foreach(T b in behaviours )
            {
                b.Activate();
            }
        }

        public static void ToggleActivity<T>(this IEnumerable<T> behaviours, bool isActivate) where T : MonoBehaviour
        {
            if(isActivate)
            {
                behaviours.ActivateEnumerable();
            }
            else
            {
                behaviours.DeactivateEnumerable();
            }
        }

        public static T Deactivate<T>(this T monoBehaviour) where T : MonoBehaviour {
            if(monoBehaviour && monoBehaviour.gameObject ) {
                Deactivate(monoBehaviour.gameObject);
            }
            return monoBehaviour;
        }



        public static T Activate<T>(this T monoBehaviour) where T : MonoBehaviour {
            if (monoBehaviour && monoBehaviour.gameObject) {
                Activate(monoBehaviour.gameObject);
            }
            return monoBehaviour;
        }

        /*
        public static void Deactivate(this MonoBehaviour monoBehaviour) {
            if(monoBehaviour && monoBehaviour.gameObject) {
                Deactivate(monoBehaviour.gameObject);
            }
        }*/

            /*
        public static void Activate(this MonoBehaviour monoBehaviour ) {
            if(monoBehaviour && monoBehaviour.gameObject) {
                Activate(monoBehaviour.gameObject);
            }
        } */

        public static void SetInteractable(this Button button, bool value) {
            if(button.interactable != value) {
                button.interactable = value;
            }
        }

        public static void SetInteractableWithShader(this Button button, bool value ) {
            if(button == null ) { return; }

            if(button.interactable != value) {
                button.interactable = value;
            }
            if(button.image != null && button.image.material != null ) {
                button.image.material.SetFloat("_Enabled", value ? 0 : 1);
            }
        }

        public static T GetOrAdd<T>(this GameObject obj) where T : Component {
            T comp = obj.GetComponent<T>();
            if(!comp) {
                comp = obj.AddComponent<T>();
            }
            return comp;
        }

        private static NumberFormatInfo doubleFloatNumberFormat = null;

        private static NumberFormatInfo DoubleFloatNumberFormat {
            get {
                if (doubleFloatNumberFormat == null) {
                    CultureInfo cultureInfo = new CultureInfo("en-us");
                    NumberFormatInfo formatInfo = cultureInfo.NumberFormat;
                    formatInfo.NumberDecimalSeparator = ",";
                    doubleFloatNumberFormat = formatInfo;
                }
                return doubleFloatNumberFormat;
            }
        }

        public static double ToDouble(this string str) {
            if(string.IsNullOrEmpty(str)) {
                return 0.0;
            }
            double result;
            if (!double.TryParse(str, NumberStyles.Any, DoubleFloatNumberFormat, out result)) {
                result = 0.0;
            }
            return result;
        }

        public static float ToFloat(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return 0.0f;
            }
            float result;
            if (!float.TryParse(str, NumberStyles.Any, DoubleFloatNumberFormat, out result)) {
                result = 0.0f;
            }
            return result;
        }

        public static int ToInt(this string str) {
            if(string.IsNullOrEmpty(str)) {
                return 0;
            }
            int result;
            if(false == int.TryParse(str, out result)) {
                result = 0;
            }
            return result;
        }

        public static string AsString<T>(this T[] array) {
            if(array == null ) {
                return string.Empty;
            }
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach(T element in array) {
                stringBuilder.Append(element.ToString());
                stringBuilder.Append(",");
            }
            if(stringBuilder.Length > 0 ) {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        public static string AsString<T>(this List<T> list)
            => list.ToArray().AsString();

        public static bool IsWhole(this float num)
            => BosUtils.IsWholeNumber(num);

        public static void SetUniformScale(this RectTransform rectTransform, float value ) 
            => rectTransform.localScale = new Vector3(value, value, 1f);

        public static void SetPosition(this RectTransform rectTransform, Vector2 pos)
            => rectTransform.anchoredPosition = pos;

        public static void SetPosition(this RectTransform rectTransform, float x, float y)
            => rectTransform.SetPosition(new Vector2(x, y));
        
        public static void SetColor(this RectTransform rectTransform, Color c)  {
            Graphic g = rectTransform.GetComponent<Graphic>();
            if( g != null ) {
                g.color = c;
            }
        }

        public static void MakeTransparent(this Image image) {
            image.overrideSprite = GameServices.Instance.ResourceService.Sprites.FallbackSprite;
        }

        public static string GetLocalizedString(this string key)
            => GameServices.Instance.ResourceService.Localization.GetString(key);


        public static void SetStringForKey(this Text text, string key) {
            text.text = GameServices.Instance.ResourceService.Localization.GetString(key);
        }

        public static string AsString<K, V>(this Dictionary<K, V> dict, System.Func<K, string> keyToString, System.Func<V, string> valueToString) {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var kvp in dict ) {
                sb.AppendLine($"{keyToString(kvp.Key)} => {valueToString(kvp.Value)}");
            }
            return sb.ToString();
        }

        public static Sprite GetSprite(this SpritePathData data) {
            if(data.IsValid) {
                return GameServices.Instance.ResourceService.GetSprite(data);
            } else {
                return GameServices.Instance.ResourceService.Sprites.FallbackSprite;
            }
        }

        public static Color ChangeAlpha(this Color color, float newAlpha) {
            return new Color(color.r, color.g, color.b, newAlpha);
        }

        public static System.Action<float, GameObject> UpdateZRotation(this RectTransform trs, System.Action action = null) {
            return (v, o) => {
                trs.localRotation = Quaternion.Euler(0, 0, v);
                action?.Invoke();
            };
        }

        public static System.Action<float, float, GameObject> UpdateZRotationTimed(this RectTransform trs)
            => (v, t, o) => trs.localRotation = Quaternion.Euler(0, 0, v);

        public static System.Action<Vector3, GameObject> UpdateScaleFunctor(this RectTransform trs, System.Action action = null)
            => (s, o) => {
                trs.localScale = s;
                action?.Invoke();
            };
        public static System.Action<Vector3, float, GameObject> UpdateScaleTimedFunctor(this RectTransform trs)
            => (s, t, o) => {
                trs.localScale = s;
            };

        public static System.Action<Vector2, GameObject> UpdatePositionFunctor(this RectTransform trs, System.Action action = null)
            => (p, o) => {
                trs.anchoredPosition = p;
                action?.Invoke();
            };

        public static System.Action<Vector2, float, GameObject> UpdatePositionTimedFunctor(this RectTransform trs)
            => (p, t, o) => {
                trs.anchoredPosition = p;
            };

        public static System.Action<Color, GameObject> UpdateColorFunctor(this Graphic trs, System.Action action = null)
            => (c, o) => {
                trs.color = c;
                action?.Invoke();
            };
        public static System.Action<Color, float, GameObject> UpdateColorTimedFunctor(this Graphic graphic, System.Action action = null)
            => (c, t, o) => {
                graphic.color = c;
            };

        public static System.Action<float, GameObject> UpdateAlphaFunctor(this CanvasGroup group, System.Action action = null)
            => (v, o) => {
                group.alpha = v;
                action?.Invoke();
            };

        public static System.Action<float, float, GameObject> UpdateAlphaTimedFunctor(this CanvasGroup group)
            => (v, t, o) => {
                group.alpha = v;
            };

        public static Vector3AnimationData ConstructScaleAnimationData(this RectTransform trs, Vector3 startValue, Vector3 endValue,
            float duration, BosAnimationMode mode, EaseType easeType, System.Action endAction = null ) {
            return new Vector3AnimationData {
                StartValue = startValue,
                EndValue = endValue,
                Duration = duration,
                AnimationMode = mode,
                EaseType = easeType,
                Target = trs.gameObject,
                OnStart = trs.UpdateScaleFunctor(),
                OnUpdate = trs.UpdateScaleTimedFunctor(),
                OnEnd = trs.UpdateScaleFunctor(endAction)
            };
        }

        public static void CopyFrom<K, T>(this Dictionary<K, T> destination, Dictionary<K, T> source ) {
            destination.Clear();
            if(source != null ) {
                foreach(var kvp in source) {
                    destination.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public static List<T> WrapToList<T>(this T obj) {
            return new List<T> { obj };
        }

        public static System.Func<System.ValueTuple> ToFunc(this System.Action action)
            => () => {
                action?.Invoke();
                return BosUtils.Unit;
            };

        public static string GuidSuffix(this string str, int len) {
            return str + Guid.NewGuid().ToString().Replace("-", "").Substring(0, len);
        }

        public static string ToTextMark(this int seconds) {
            if (seconds < 0) {
                seconds = 0;
            }
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.ToTextMark();
        }

        public static string ToTextMark(this TimeSpan ts) {
            double seconds = ts.TotalSeconds;
            if (seconds >= 0.0 && seconds < 60) {
                return "0-60 sec";
            } else if (60 <= seconds && seconds < 5 * 60) {
                return "1-5 min";
            } else if (5 * 60 <= seconds && seconds < 30 * 60) {
                return "5-30 min";
            } else if (30 * 60 <= seconds && seconds < 60 * 60) {
                return "30-60 min";
            } else if (3600 <= seconds && seconds < 7200) {
                return "1-2 h";
            } else if (7200 <= seconds && seconds < 18000) {
                return "2-5 h";
            } else if (18000 <= seconds && seconds < 36000) {
                return "5-10 h";
            } else if (36000 <= seconds && seconds < 57600) {
                return "10-16 h";
            } else if (57600 <= seconds && seconds < 86400) {
                return "16-24 h";
            } else if (86400 <= seconds && seconds < 129600) {
                return "24-36 h";
            } else if (129600 <= seconds && seconds < 172800) {
                return "36-48 h";
            } else if (172800 <= seconds && seconds < 259200) {
                return "48-72 h";
            } else if (259200 <= seconds && seconds < 345600) {
                return "72-96 h";
            } else if (345600 <= seconds && seconds < 518400) {
                return "4-6 d";
            } else if (518400 <= seconds && seconds < 950400) {
                return "7-11 d";
            }
            else {
                return ">1 w";
            }
        }

        public static double OfficialConvertCashValue(this PersonalConvertData convertData, double sourceCash ) {
            return convertData.OfficialConvertPercent * sourceCash;
            
        }

        public static string Format(this string source, params object[] args) {
            return string.Format(source, args);
        }

        public static bool Approximately(this float value, float other ) {
            return Mathf.Approximately(value, other);
        }

        public static bool Approximately(this double value, double other) {
            return MathUtils.Approximately(value, other);
        }
    }

    public static class GeneratorExtensions {

        public static bool IsTeleport(this int generatorId)
            => generatorId == 9;

        public static Option<string> ConstructGeneratorName(this GeneratorLocalData generatorLocalData, int planetId, System.Func<string, string> getString) {

            var nameObject = generatorLocalData.GetName(planetId);
            if(nameObject == null ) {
                UDBG.LogError($"not found name for generator {generatorLocalData.id} and planet {planetId}");
                return F.None;
            } else {
                //UDBG.Log($"generator name founded...");
                return F.Some(getString(nameObject.name));
            }
            /*
            string planetName = getString(planetNameData.name);
            string generatorName = getString(generatorLocalData.name);
            var generatorData = getGenerator(generatorLocalData.id);
            if(generatorData.Type == GeneratorType.Normal ) {
                if(planetNameData.id == 0 ) {
                    return F.Some(generatorName);
                } else {
                    return F.Some(planetName + generatorName);
                }
            } else {
                return F.Some(generatorName);
            }*/

        }
    }

    public static class ResourceExtensions {

        public static T CreateGameObject<T>(this string prefabName, Transform parent) where T : MonoBehaviour {
            var prefab = GameServices.Instance.ResourceService.Prefabs.GetPrefab(prefabName);
            GameObject instance = GameObject.Instantiate(prefab);
            if(parent != null ) {
                instance.GetComponent<Transform>().SetParent(parent, false);
            }
            return instance.GetComponent<T>();
        }
    }

}