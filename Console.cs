namespace Bos.Debug {
    using Bos.Data;
    using Bos.UI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UniRx;
    using UnityEngine;

    public class Console : GameBehaviour, IConsoleService {

        public const int kDrawLog = 0;
        public const int kDrawPlayerStatus = 1;
        public const int kDrawBankService = 2;
        public const int kDrawUnits = 3;
        public const int kDrawInvestors = 4;
        public const int kDrawSpecialOffer = 6;

        private readonly float lineHeight = 45;
        private readonly float scrollViewWidth = 1000;
        public bool isOnScreenTextEnabled = false;
        public bool isConsoleEnabled = true;

        public GUISkin skin;
        public bool isAlwayVisible = false;

        private string command = string.Empty;
        private readonly List<ConsoleString> outputList = new List<ConsoleString>();
        private Vector2 scrollViewPosition = Vector2.zero;
        private GUIStyle textFieldStyle;
        private GUIStyle labelStyle;
        private readonly List<string> onScreenTexts = new List<string>();
        private float onScreenTextTimer = 0f;
        private string currentOnScreenText = string.Empty;
        private bool isWasOpened = false;
        private RectTransform screenLockTransform = null;
        private bool isScrollVisible = true;
        private int currentViewIndex = -1;
        private bool isInitialized = false;
     

        private Rect ScrollViewRect {
            get {
                float height = 0f;
                foreach (var text in outputList) {
                    height += text.MeasureHeight(labelStyle, scrollViewWidth);
                }
                return new Rect(0, 0, scrollViewWidth, height);
            }
        }

        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command> {
            ["echo"] = new EchoCommand(),
            ["add"] = new AddCommand(),
            ["reset"] = new ResetCommand(),
            ["spawn"] = new SpawnCommand(),
            ["time"] = new TimeCommand(),
            ["net"] = new NetCommand(),
            ["view"] = new ViewCommand(),
            ["print"] = new PrintCommand(),
            ["unit"] = new UnitCommand(),
            ["test"] = new TestCommand(),
            ["transform"] = new TransformCommand(),
            ["set"] = new SetCommand(),
            ["get"] = new GetCommand(),
            ["write"] = new WriteCommand(),
            ["sub"] = new SubCommand(),
            ["skip"] = new SkipCommand(),
            ["tut"] = new TutCommand()
        };

        private float CalculateLineHeight(string line, float width, GUIStyle style) {
            return style.CalcHeight(new GUIContent(line), width);
        }

        private readonly Dictionary<int, Action> drawViews = new Dictionary<int, Action>();


        public override void Awake() {
            base.Awake();
            drawViews.Clear();
            drawViews.Add(kDrawLog, DrawLog);
            drawViews.Add(kDrawPlayerStatus, DrawStatus);
            drawViews.Add(kDrawBankService, DrawBank);
            drawViews.Add(kDrawUnits, DrawUnits);
            drawViews.Add(kDrawInvestors, DrawInvestors);
            drawViews.Add(kDrawSpecialOffer, DrawSpecialOffer);
        }

        public void Setup(object data = null) {
            if (!isInitialized) {
                isInitialized = true;
            }
        }

        public void UpdateResume(bool pause)
            => Debug.Log($"{nameof(Console)}.{nameof(UpdateResume)}() => {pause}");

        public override void OnEnable() {
            base.OnEnable();
            GameEvents.GameModeChanged += OnGameModeChanged;
            GameEvents.InvestorTriesChanged += OnInvestorTriesChanged;
        }

        public override void OnDisable() {
            GameEvents.GameModeChanged -= OnGameModeChanged;
            GameEvents.InvestorTriesChanged -= OnInvestorTriesChanged;
            base.OnDisable();
        }

        private void OnInvestorTriesChanged(object service, InvestorTriesChangedEventArgs args ) {
            AddOutput($"investor tries changed from {args.OldTries} to {args.NewTries}", ConsoleTextColor.yellow, true);
        }

        public override void Start() {
            base.Start();
            textFieldStyle = new GUIStyle(skin.GetStyle("textfield"));
            labelStyle = new GUIStyle(skin.GetStyle("console_label"));
        }

        public override void Update() {
            base.Update();
#if UNITY_EDITOR 
            if (Input.GetKeyUp(KeyCode.BackQuote) || Input.GetKeyUp(KeyCode.Escape)) {
                IsVisible = !IsVisible;
                if (IsVisible) {
                    isWasOpened = true;
                }
            }
#endif
            if (onScreenTextTimer > 0) {
                onScreenTextTimer -= Time.deltaTime;
            } else {
                if (onScreenTexts.Count > 0) {
                    currentOnScreenText = onScreenTexts[0];
                    onScreenTexts.RemoveAt(0);
                    onScreenTextTimer = 2;
                } else {
                    currentOnScreenText = string.Empty;
                }
            }

            if (Input.GetKeyUp(KeyCode.Alpha0)) {
                currentViewIndex = 0;
            } else if (Input.GetKeyUp(KeyCode.Alpha1)) {
                currentViewIndex = 1;
            } else if (Input.GetKeyUp(KeyCode.Alpha2)) {
                currentViewIndex = 2;
            } else if (Input.GetKeyUp(KeyCode.Alpha3)) {
                currentViewIndex = 3;
            } else if (Input.GetKeyUp(KeyCode.Alpha4)) {
                currentViewIndex = 4;
            } else if (Input.GetKeyUp(KeyCode.Alpha5)) {
                currentViewIndex = 5;
            } else if(Input.GetKeyUp(KeyCode.Alpha6)) {
                currentViewIndex = 6;
            }
        }


        private Matrix4x4 SetupDrawMatrix() {
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = ConsoleUtils.ConsoleMatrix;
            return oldMatrix;
        }

        private void RestoreDrawMatrix(Matrix4x4 matr) {
            GUI.matrix = matr;
        }

        private void DrawOnScreenTextIfNeeded() {
            if (isOnScreenTextEnabled) {
                if (!string.IsNullOrEmpty(currentOnScreenText)) {
                    GUI.Label(new Rect(0, 0, ConsoleUtils.ReferenceWidth, 300), currentOnScreenText, labelStyle);
                }
            }
        }

        private void ForceVisibilityIfNeeded() {
            if (isAlwayVisible) {
                IsVisible = true;
            }
        }

        private float TextFieldHeight
            => lineHeight + 10;

        private float TextFieldWidth
            => ConsoleUtils.ReferenceWidth - 20 - RunButtonWidth;

        private float RunButtonWidth
            => 100;


        private void CreateScreenLock() {
            if (screenLockTransform == null) {
                GameObject instance = GameObject.Instantiate(Services.ResourceService.Prefabs.GetPrefab("lastsibling"), Services.ViewService.GetCanvasTransform(CanvasType.UI), false);
                screenLockTransform = instance.GetComponent<RectTransform>();
            }
        }

        private void DestroyScreenLock() {
            if (screenLockTransform != null && screenLockTransform.gameObject) {
                Destroy(screenLockTransform.gameObject);
                screenLockTransform = null;
            }
        }

        private void StartGroup() {
            GUI.Box(new Rect(0, 0, ConsoleUtils.ReferenceWidth, ConsoleUtils.ReferenceHeight - TextFieldHeight), string.Empty);
            GUI.BeginGroup(new Rect(0, 0, ConsoleUtils.ReferenceWidth, ConsoleUtils.ReferenceHeight - TextFieldHeight));
        }

        private void EndGroup() {
            GUI.EndGroup();
        }

        private Rect StartRect()
            => new Rect(0, 0, ConsoleUtils.ReferenceWidth, 45);

        private void DrawView(Action drawAction) {
            StartGroup();
            drawAction?.Invoke();
            EndGroup();
        }



        private void DrawLog() {
            if (isScrollVisible) {
                Rect viewRect = ScrollViewRect;
                Rect scrollRect = new Rect(10, 10, scrollViewWidth + 40, ConsoleUtils.ReferenceHeight - TextFieldHeight - 15);
                scrollViewPosition = GUI.BeginScrollView(scrollRect, scrollViewPosition, viewRect);
                float y = 0f;
                for (int i = 0; i < outputList.Count; i++) {
                    float height = outputList[i].MeasureHeight(labelStyle, scrollViewWidth);
                    GUI.Label(new Rect(0, y, scrollViewWidth, height), outputList[i].DecoratedString, labelStyle);
                    y += height;
                }
                GUI.EndScrollView();
            }

            isScrollVisible = GUI.Toggle(new Rect(10, ConsoleUtils.ReferenceHeight - TextFieldHeight - 60, 60, 60), isScrollVisible, "TOGGLE SCROLL");
        }

        private void DrawStatus() {
            var impr = Services.ResourceService.PersonalImprovements;
            var player = Services.PlayerService;
            float height = 45;

            StartGroup();
            Rect rect = new Rect(0, 0, ConsoleUtils.ReferenceWidth, 45);
            GUI.Label(rect, $"Min status => {impr.MinStatusLevel}");
            rect = rect.Down(height);
            GUI.Label(rect, $"Max status => {impr.MaxStatusLevel}");
            rect = rect.Down(height);
            GUI.Label(rect, $"Current status => {player.StatusLevel}, current points => {player.StatusPoints}");
            rect = rect.Down(height);
            GUI.Label(rect, $"Prev level => {player.StatusLevel - 1}, prev level points => {impr.GetPointsForStatusLevel(player.StatusLevel - 1)}");
            rect = rect.Down(height);
            GUI.Label(rect, $"Next level => {player.StatusLevel + 1}, next level points => {impr.GetPointsForStatusLevel(player.StatusLevel + 1)}");
            rect = rect.Down(height);
            GUI.Label(rect, $"progress level => {impr.GetStatusLevelProgress(player.StatusPoints)}");
            GUI.EndGroup();
        }


        private void DrawBank() {
            DrawView(() => {
                var bankService = Services.BankService;
                StartRect()
                .Label($"Current Bank Level => {bankService.CurrentBankLevel}")
                .Down()
                .Label($"Profit timer => {bankService.ProfitTimer}")
                .Down()
                .Label($"Time from last collect => {bankService.TimerFromLastCollect}")
                .Down()
                .Label($"Accumulated coins count => {bankService.CoinsAccumulatedCount}");
            });
        }

        private void DrawUnits() {
            DrawView(() => {
                var uService = Services.TransportService;
                var currentRect = StartRect();
                foreach(var generator in Services.GenerationService.GeneratorEnumarable ) {
                    string generatorName = ViewService.Utils.ApplyGeneratorName(null, generator).Match(() => string.Empty, str => str);
                    currentRect = currentRect.Label($"NAME => {generatorName.Colored(ConsoleTextColor.green)}({generator.GeneratorId}), live count => {uService.GetUnitLiveCount(generator.GeneratorId)}, broken count => {uService.GetUnitBrokenedCount(generator.GeneratorId).ToString().Colored(ConsoleTextColor.red)}, total count => {uService.GetUnitTotalCount(generator.GeneratorId).ToString().Colored(ConsoleTextColor.yellow)}").Down();
                }
            });
        }

        private void DrawInvestors() {
            DrawView(() => {
                var investorService = Services.InvestorService;
                var rect = StartRect().Label("Investor service=>")
                .Down().Label($"Tries count: {investorService.TriesCount}")
                .Down().Label($"Allow Sell Time: {investorService.AllowSellTime}")
                .Down().Label($"Effectiveness: {investorService.Effectiveness}")
                .Down().Label($"Securities profit mult: {investorService.SecuritiesProfitMult}")
                .Down();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("State => ");
                foreach(var state in investorService.SellState ) {
                    sb.AppendLine($"\t\t:{state}");
                }
                rect.Label(sb.ToString());
            });
        }

        private void DrawSpecialOffer() {
            DrawView(() => {
                var specialOfferService = Services.GetService<ISpecialOfferService>();
                float width = StartRect().width;
                GUIStyle labelStyle = skin.GetStyle("label");
                float height = labelStyle.CalcHeight(new GUIContent(specialOfferService.ToString()), width);
                StartRect().WithHeight(height).Label(specialOfferService.ToString()).Down();
            });
        }

        private void DrawCommandInput() {
            GUI.SetNextControlName("command");
            command = GUI.TextField(new Rect(10, ConsoleUtils.ReferenceHeight - TextFieldHeight, TextFieldWidth, TextFieldHeight), command, textFieldStyle);

            if (GUI.Button(new Rect(10 + TextFieldWidth, ConsoleUtils.ReferenceHeight - TextFieldHeight, RunButtonWidth, TextFieldHeight), "RUN")) {
                RunCommand(command);
            }

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return) {
                RunCommand(command);
            } else if (Event.current.isKey && Event.current.keyCode == KeyCode.BackQuote) {
                if (GUI.GetNameOfFocusedControl() == "command") {
                    IsVisible = !IsVisible;
                }
            }

            if (isWasOpened) {
                isWasOpened = false;
                GUI.FocusControl("command");
            }
        }


        void OnGUI() {
            if (isConsoleEnabled) {
                GUI.skin = skin;
                Matrix4x4 oldMatrix = SetupDrawMatrix();
                DrawOnScreenTextIfNeeded();
                ForceVisibilityIfNeeded();

                
                if (IsVisible) {
                    CreateScreenLock();
                } else {
                    DestroyScreenLock();
                }


#if UNITY_EDITOR || UNITY_ANDROID
                if (IsVisible) {
                    DrawCommandInput();
                    if(drawViews.ContainsKey(currentViewIndex)) {
                        drawViews[currentViewIndex].Invoke();
                    }
                }
#endif

                RestoreDrawMatrix(oldMatrix);
            }
        }

        public void ToggleConsole() =>
            IsVisible = !IsVisible;

        public void SetDebugView(int index)
            => currentViewIndex = index;

        public void AddOnScreenText(string txt) {
            onScreenTexts.Add(txt);
            if (onScreenTexts.Count > 100) {
                onScreenTexts.RemoveAt(0);
            }
        }

        public void RunCommand(string command) {
            string commandName = GetCommandName(command);
            if (commands.ContainsKey(commandName)) {
                commands[commandName].Run(command, this);
            } else {
                AddOutput($"invalid command => {command}", ConsoleTextColor.red);
            }
        }

        private string GetCommandName(string command) {
            string[] tokens = command.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length > 0) {
                return tokens[0].ToLower();
            }
            return string.Empty;
        }

        private void OnGameModeChanged(GameModeName oldGameMode, GameModeName newGameMode) {
            AddOnScreenText($"new game mode => {newGameMode.ToString()}");
        }

        #region IConsoleService


        public void AddOutput(string text, ConsoleTextColor color = ConsoleTextColor.white, bool isDuplicateAtUnityLog = false) {
#if UNITY_EDITOR || UNITY_ANDROID
            ConsoleString consoleString = new ConsoleString(text, color);
            outputList.Add(consoleString);

            while (outputList.Count > 200) {
                outputList.RemoveAt(0);
            }

            if (isDuplicateAtUnityLog) {
                Debug.Log(consoleString.DecoratedString);
            }

            scrollViewPosition.y = float.MaxValue * .5f;
#endif

        }

        public void SetVisible(bool isVisible) {
            this.IsVisible = isVisible;
        }

        public bool IsVisible { get; private set; } = false;

        #endregion
    }


    public interface IConsoleService : IGameService {
        void AddOutput(string text, ConsoleTextColor color = ConsoleTextColor.white, bool isDuplicateAtUnityLog = false);
        void SetVisible(bool isVisible);
        bool IsVisible { get; }
        void AddOnScreenText(string txt);
        void ToggleConsole();
        void SetDebugView(int index);
        void RunCommand(string command);
    }

    public static class ConsoleUtils {
        public static float ReferenceWidth => 1080;
        public static float ReferenceHeight => 1920;
        public static Matrix4x4 ConsoleMatrix =>
            Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight, 1));

        public static Rect Down(this Rect rect) {
            return new Rect(rect.x, rect.y + rect.height, rect.width, rect.height);
        }

        public static Rect Down(this Rect rect, float offset)
            => new Rect(rect.x, rect.y + offset, rect.width, rect.height);

        public static Rect Up(this Rect rect, float offset)
            => new Rect(rect.x, rect.y - offset, rect.width, rect.height);

        public static Rect Right(this Rect rect, float offset)
            => new Rect(rect.x + offset, rect.y, rect.width, rect.height);

        public static Rect Left(this Rect rect, float offset)
            => new Rect(rect.x - offset, rect.y, rect.width, rect.height);

        public static Rect Label(this Rect rect, string content) {
            GUI.Label(rect, content);
            return rect;
        }

        public static Rect WithHeight(this Rect rect, float newHeight) {
            return new Rect(rect.position, new Vector2(rect.width, newHeight));
        }


    }

    public class ConsoleString {
        public string Text { get; private set; } = string.Empty;
        public ConsoleTextColor Color { get; private set; } = ConsoleTextColor.white;

        public ConsoleString(string text, ConsoleTextColor color = ConsoleTextColor.white) {
            Text = text;
            Color = color;
        }

        public string DecoratedString => Text.Colored(Color);


        public float MeasureHeight(GUIStyle style, float width) {
            return style.CalcHeight(new GUIContent(Text), width);
        }
    }

    public enum ConsoleTextColor {
        aqua,
        black,
        blue,
        brown,
        cyan,
        darkblue,
        fuchsia,
        green,
        grey,
        lightblue,
        lime,
        magenta,
        maroon,
        navy,
        olive,
        orange,
        purple,
        red,
        silver,
        teal,
        white,
        yellow
    }


    #region Console Commands

    public class TutCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string whatToDo = GetParam<string>(1);
            if(whatToDo.ToLower() == "start") {
                int state = GetParam<int>(2);

                /*
                var player = Services.PlayerService;
                var product = player.ProductNotifier.NotPurchasedMinPriceProduct;

                if(product == null ) {
                    Debug.Log("no non purchased product...");
                    return;
                }

                while(player.ProductNotifier.TargetCash < product.price ) {
                    player.AddCompanyCash(product.price * 2);
                }*/
                
                var result = Services.TutorialService.ForceStart(state);
                Debug.Log($"started => {result.Name}".Attrib(bold: true, italic: true, color: "green", size: 20));
            } else if(whatToDo.ToLower() == "dump")
            {
                Debug.Log(Services.TutorialService.GetValidationDump());
            }
        }
    }
    
    public class WriteCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);
        }
    }

    public class GetCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string whatGet = GetParam<string>(1);
            if (whatGet == "profitstats") {
                int generatorId = GetParam<int>(2);
                var generator = Services.GenerationService.GetGetenerator(generatorId);
                string resultStr = generator.ProfitBoosts.ToString(); //generator.ProfitMultStats.AsString(key => key, value => value.ToString("F2"));
                Debug.Log(resultStr.Colored(ConsoleTextColor.fuchsia));
            }
        }
    }

    public class SetCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string what = GetParam<string>(1).ToLower();
            if (what == "planetprofitmult") {
                var planetData = Services.ResourceService.Planets.GetPlanet(Services.PlanetService.CurrentPlanet.Id);
                planetData.SetProfitMultiplier(GetParam<double>(2));
                Debug.Log($"SetCommand.Run(), set planetprofitmult => {planetData.ProfitMultiplier}");
            } else if (what == "coins") {
                Services.PlayerService.SetCoins(GetParam<int>(2));
            } else if (what == "end") {
                Services.GameModeService.StartWinGame();
            } else if (what.ToLower() == "debug") {
                console.SetVisible(GetParam<int>(2) != 0 ? true : false);
            } else if (what.ToLower() == "offer") {
                Services.GetService<ISpecialOfferService>().ForceStart();
            } else if (what.ToLower() == "viewindex") {
                int index = GetParam<int>(2);
                console.SetDebugView(index);
            } else if (what.ToLower() == "nextplanet") {
                if (Services.PlanetService.HasNextPlanet) {
                    Services.PlanetService.ForceSetOpened(Services.PlanetService.NextPlanetId);
                }
            } else if(what.ToLower() == "module") {
                int moduleId = GetParam<int>(2);
                Services.Modules.ForceOpenModule(moduleId);
            }
        }
    }

    public class TransformCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string what = GetParam<string>(1);
            switch (what) {
                case "cash": {

                    }
                    break;
                case "investor": {

                    }
                    break;
                case "coin": {

                    }
                    break;
            }
        }
    }

    public class TestCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string whatTest = GetParam<string>(1);
            switch (whatTest) {
                case "sound": {
                        Services.RunCoroutine(TestSoundImpl(console));
                    }
                    break;
                case "typedboost": {
                        Services.RunCoroutine(TestTypedBoosts(console));
                    }
                    break;
            }

            
        }

        private IEnumerator TestTypedBoosts(IConsoleService console) {
            console.AddOutput("adding typed boosts", ConsoleTextColor.yellow, true);
            int count = AddTestTypedBoosts();
            console.AddOutput($"boosts added: {count}", ConsoleTextColor.yellow, true);
            console.AddOutput("wait 3 sec", ConsoleTextColor.yellow, true);
            yield return new WaitForSeconds(3);
            console.AddOutput("try remove boosts", ConsoleTextColor.yellow, true);
            count = GenerationSrv.RemoveBoosts(BoostSourceType.CoinUpgrade);
            console.AddOutput($"boosts removed: {count}", ConsoleTextColor.yellow, true);
        }

        private int AddTestTypedBoosts() {
            int boostCount = 0;

            for(int i = 0; i < 3; i++ ) {
                BoostInfo globalProfitBoost = BoostInfo.CreatePersist(Guid.NewGuid().ToString(), 2, (int)BoostSourceType.CoinUpgrade);
                GenerationSrv.Generators.AddProfitBoost(globalProfitBoost);
                boostCount++;
            }

            for(int i = 0; i < 3; i++ ) {
                BoostInfo globalTimeBoost = BoostInfo.CreateTemp(Guid.NewGuid().ToString(), 2, (int)BoostSourceType.CoinUpgrade);
                GenerationSrv.Generators.AddTimeBoost(globalTimeBoost);
                boostCount++;
            }

            for(int i = 0; i < 3; i++ ) {
                BoostInfo localProfitBoost = BoostInfo.CreatePersist(Guid.NewGuid().ToString(), 2, (int)BoostSourceType.CoinUpgrade);
                BoostInfo localTimeBoost = BoostInfo.CreatePersist(Guid.NewGuid().ToString(), 2, (int)BoostSourceType.CoinUpgrade);
                foreach(GeneratorInfo generator in GenerationSrv.Generators.ActiveGenerators ) {
                    generator.AddProfitBoost(localProfitBoost);
                    generator.AddTimeBoost(localTimeBoost);
                    boostCount += 2;
                }
            }
            return boostCount;
        }


        private IEnumerator TestSoundImpl(IConsoleService console) {
            foreach (SoundName soundName in System.Enum.GetValues(typeof(SoundName))) {
                Services.GetService<ISoundService>().PlayOneShot(soundName);
                console.AddOutput(text: $"playing sound => {soundName.ToString()}",
                    color: ConsoleTextColor.fuchsia, isDuplicateAtUnityLog: true);
                yield return new WaitForSeconds(2);
            }
        }
    }

    public class PrintCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string whatPrint = GetParam<string>(1);
            if (whatPrint == "gens") {
                console.AddOutput(Services.GenerationService.Generators.ToString(), ConsoleTextColor.yellow, true);
            } else if (whatPrint == "indices") {
                PlanetHeaderLineView headerLineView = GameObject.FindObjectOfType<PlanetHeaderLineView>();
                console.AddOutput(headerLineView.GetInfoString(), ConsoleTextColor.lightblue, true);
            } else if (whatPrint == "gendiff") {
                var resData = Services.ResourceService.Generators.GetGeneratorData(0);
                var infoData = Services.GenerationService.Generators.GetGeneratorInfo(0).Data;

                Debug.Log($"from resources => {resData.IncrementFactor}, from info => {infoData.IncrementFactor}, ref equals => {object.ReferenceEquals(resData, infoData)}");
            }

            switch (whatPrint.ToUpper().Trim()) {
                case "UPGRADES": {
                        foreach (var upgradeData in Services.ResourceService.CashUpgrades.UpgradeList) {
                            console.AddOutput(upgradeData.ToString());
                        }
                        foreach (var upgradeData in Services.ResourceService.SecuritiesUpgrades.UpgradeList) {
                            console.AddOutput(upgradeData.ToString());
                        }
                        foreach (var upgradeData in Services.ResourceService.CoinUpgrades.UpgradeList) {
                            console.AddOutput(upgradeData.ToString());
                        }
                    }
                    break;
                case "TUTORIAL": {
                        var tutorialService = Services.TutorialService;
                        foreach (var state in tutorialService.States) {
                            console.AddOutput($"{state.GetType().Name}, is active: {state.IsActive}, is completed: {state.IsCompleted}", ConsoleTextColor.lightblue, true);
                        }
                        tutorialService.PrintTutorialStates();
                    }
                    break;
                case "STATUS": {
                        // StringBuilder stringBuilder = new StringBuilder();
                        // var impr = Services.ResourceService.PersonalImprovements;
                        // stringBuilder.AppendLine($"Min Status => {impr.MinStatusLevel}");
                        // stringBuilder.AppendLine($"Max Status => ")
                    }
                    break;
                case "PERSONALPRODUCTS": {
                        var productTypes = (ProductType[])System.Enum.GetValues(typeof(ProductType));
                        System.Text.StringBuilder sb = new StringBuilder();
                        productTypes.ToList().ForEach(pt => sb.AppendLine($"type => {pt}, count => {Services.ResourceService.PersonalProducts.GetProducts(pt).Count}"));
                        console.AddOutput(sb.ToString(), ConsoleTextColor.white, true);
                    }
                    break;
                case "BALANCE": {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"COMPANY => {Services.PlayerService.CompanyCash.Value}");
                        sb.AppendLine($"PERSONAL => {Services.PlayerService.PlayerCash.Value}");
                        sb.AppendLine($"SECURITIES => {Services.PlayerService.Securities.Value}");
                        sb.AppendLine($"COINS => {Services.PlayerService.Coins}");
                        console.AddOutput(sb.ToString(), ConsoleTextColor.green, true);
                    }
                    break;
                case "GENPLANETMULT": {
                        var generators = Services.GenerationService.Generators;
                        string result = $"PROFIT: {generators.PlanetProfitBoost}, TIME: {generators.PlanetTimeBoost}";
                        console.AddOutput(result, ConsoleTextColor.fuchsia, true);
                    }
                    break;
                case "PROFITPERROUND": {
                        int generatorId = GetParam<int>(2);
                        var generator = Services.GenerationService.GetGetenerator(generatorId);
  
                        generator.ProfitPerRound(Services.GenerationService.Generators, true);
                    }
                    break;
                case "PRFINCRFACT": {
                        StringBuilder sb = new StringBuilder();
                        foreach(var genData in Services.ResourceService.Generators.GeneratorCollection ) {
                            sb.AppendLine($"Profit increment factor, gen id = {genData.Id}, incr factor = {genData.ProfitIncrementFactor}");
                        }
                        console.AddOutput(sb.ToString(), ConsoleTextColor.aqua, true);
                    }
                    break;
            }
        }


    }

    public class ViewCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string view = GetParam<string>(1);
            if (view == "shipend") {
                //Services.ViewService.Show(UI.ViewType.SpaceShipEndView);
            } else if (view == "bank") {
                Services.ViewService.Show(UI.ViewType.BankView);
            } else if (view == "investor") {
                Services.ViewService.ShowDelayed(UI.ViewType.InvestorsView, BosUISettings.Instance.ViewShowDelay);
            } else if (view == "confirm") {
                Services.ViewService.Show(ViewType.InvestorConfirmMessageBox, new ViewData {
                    UserData = 1e6
                });

            } else if (view == "history") {
                List<HistoryEntry> entries = new List<HistoryEntry>();
                for (int i = 0; i < 12; i++) {
                    HistoryEntry entry = new HistoryEntry(i % 6, UnityEngine.Random.Range(0, 1e10f), UnityEngine.Random.Range(0, 1000000));
                    entries.Add(entry);
                }
                Services.ViewService.ShowDelayed(ViewType.HistoryView, BosUISettings.Instance.ViewShowDelay, new ViewData() { UserData = entries });
            } else if (view == "main") {
                Services.ViewService.Show(ViewType.MainView);
            } else if (view == "finger") {
                Services.ViewService.Utils.CreateTutorialFinger(new TutorialFingerData() {
                    Id = System.Guid.NewGuid().ToString(),
                    Position = Vector2.zero
                });
            } else if (view == "dialog") {
                Services.ViewService.Show(ViewType.TutorialDialogView, new ViewData() {
                    UserData = new TutorialDialogData() {
                        Texts = "Hello there, start new tutor!".WrapToList()
                    }
                });
            } else if(view == "firstinvestor") {
                /*
                Services.ViewService.Show(ViewType.FirstInvestorView);*/
                console.AddOutput("command already not supported(view removed)");
            } else if(view == "rateapp") {
                Services.ViewService.ShowDelayed(ViewType.RateAppView, BosUISettings.Instance.ViewShowDelay, new ViewData { ViewDepth = Services.ViewService.NextViewDepth });
            } else if(view == "waitad") {
                Services.ViewService.Show(ViewType.WaitAdView, new ViewData {
                    UserData = new WaitAdData { ContentType = "test", Action = () => { } },
                    ViewDepth = Services.ViewService.NextViewDepth
                });
            } else if(view == "offerbutton") {
                //Services.ViewService.Show(ViewType.SpecialOfferButton);
                OfferButtonContainer container = GameObject.FindObjectOfType<OfferButtonContainer>();
                container?.specialOfferButtonObject?.Activate();
            } else if(view == "event" ) {
                LocalEventManager mgr = GameObject.FindObjectOfType<LocalEventManager>();
                mgr?.Events[UnityEngine.Random.Range(0, mgr.Events.Length)].EventEnter();
            } else if(view == "moduleflight")
            {
                Services.ViewService.Show(ViewType.BuyModuleView, new ViewData
                {
                    ViewDepth = Services.ViewService.NextViewDepth,
                    UserData = new ModuleViewModel
                    {
                        ScreenType = ModuleScreenType.Flight
                    }
                });
            }
        }
    }

    public class NetCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            INetService netService = Services.GetService<INetService>();

            string what = GetParam<string>(1);
            if (what == "ships") {
                netService.GetBalanceShip((modules) => {
                    foreach (var module in modules) {
                        console.AddOutput(module.ToString(), ConsoleTextColor.fuchsia, true);
                    }
                }, (error) => {
                    console.AddOutput(error, ConsoleTextColor.red, true);
                });
            } else if (what == "mechanic") {
                netService.GetBalanceMechanic((mechanics) => {
                    foreach (var mechanic in mechanics) {
                        console.AddOutput(mechanic.ToString(), ConsoleTextColor.green, true);
                    }
                }, (error) => {
                    console.AddOutput(error, ConsoleTextColor.red, true);
                });
            } else if (what == "secretary") {
                netService.GetSecretaryBalance((secretaries) => {
                    console.AddOutput($"Secretaries loaded=>", ConsoleTextColor.green, true);
                    foreach (SecretaryData secretaryData in secretaries) {
                        console.AddOutput($"{secretaryData.ToString()}".Bold(), ConsoleTextColor.green, true);
                    }
                }, (error) => console.AddOutput(error, ConsoleTextColor.red, true));
            } else if (what == "strength") {
                netService.GetTransportStrength(items => {
                    foreach (var item in items) {
                        console.AddOutput(item.ToString(), ConsoleTextColor.green, true);
                    }
                }, error => {
                    console.AddOutput(error, ConsoleTextColor.red, true);
                });
            } else if (what == "bank") {
                netService.GetBank(levels => {
                    foreach (var bankLevel in levels) {
                        console.AddOutput(text: bankLevel.ToString(), color: ConsoleTextColor.yellow, isDuplicateAtUnityLog: true);
                    }
                }, error => {
                    console.AddOutput(text: error, color: ConsoleTextColor.red, isDuplicateAtUnityLog: true);
                });
            } else if (what == "planettrans") {
                netService.GetPlanetsTransport((generators) => {
                    foreach (var data in generators) {
                        console.AddOutput(text: data.ToString(), color: ConsoleTextColor.aqua, isDuplicateAtUnityLog: true);
                    }
                }, (error) => {
                    console.AddOutput(text: error, color: ConsoleTextColor.red, isDuplicateAtUnityLog: true);
                });
            } else if (what == "impr") {
                netService.GetPersonalImprovements((improvements) => {
                    console.AddOutput(improvements.ConvertData.ToString(), ConsoleTextColor.orange, true);
                    //foreach (var kvp in improvements.Products) {
                    //    console.AddOutput($"{kvp.Key} => {string.Join("\n", kvp.Value.Select(p => p.ToString()).ToArray())}", ConsoleTextColor.orange, true);
                    //}
                    console.AddOutput(string.Join("\n", improvements.StatusPoints.Select(sp => sp.ToString()).ToArray()), ConsoleTextColor.orange, true);
                }, error => {
                    console.AddOutput(text: error, color: ConsoleTextColor.red, isDuplicateAtUnityLog: true);
                });
            } else if (what == "manimpr") {
                netService.GetManagerImprovements((improvements) => {
                    console.AddOutput(text: improvements.ToString(), color: ConsoleTextColor.yellow, isDuplicateAtUnityLog: true);
                },
                (error) => {
                    console.AddOutput(text: error, color: ConsoleTextColor.red, isDuplicateAtUnityLog: true);
                });
            } else if(what == "promo") {
                netService.GetPromoBonus("YCXdqFTw", (code, val) => {
                    Debug.Log($"Received quantity: {val}".Attrib(bold: true, italic: true, color: "y", size: 30));
                }, err => {
                    Debug.LogError(err);
                });
            }
        }
    }

    public class EchoCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            console.AddOutput(commandText, ConsoleTextColor.yellow);
        }
    }

    public class UnitCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);
            string whatToDo = GetParam<string>(1);

            if (whatToDo == "live") {
                int generatorId = GetParam<int>(2);
                int count = GetParam<int>(3);
                Services.TransportService.AddLiveUnits(generatorId, count);
                console.AddOutput($"added {count} live units to generator => {generatorId}", ConsoleTextColor.yellow, true);
            } else if (whatToDo == "broke") {
                int generatorId = GetParam<int>(2);
                int count = GetParam<int>(3);
                Services.TransportService.ForceBroke(generatorId, count);
                console.AddOutput($"added {count} brokened units to generator => {generatorId}", ConsoleTextColor.yellow, true);
            }
        }
    }

    public class SubCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);
            string what = GetParam<string>(1);
            switch (what.Trim().ToUpper()) {
                case "X20TIMER": {
                        var x20Service = Services.GetService<IX20BoostService>();
                        if (x20Service.State != BoostState.Active) {
                            console.AddOutput("megaboost not active", ConsoleTextColor.orange, true);
                        } else {
                            x20Service.RemoveFromActiveTimer(GetParam<int>(2));
                        }
                    }
                    break;
            }
        }
    }

    public class AddCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string whatToAdd = GetParam<string>(1);
            if (whatToAdd == "coins") {
                int count = GetParam<int>(2);
                //var balanceManager = GameObject.FindObjectOfType<BalanceManager>();
                //balanceManager.IAPManager.Coins.Value += count;
                Services.PlayerService.AddCoins(count);
                console.AddOutput($"added {count} coins", ConsoleTextColor.green);
            } else if (whatToAdd == "balance") {
                int count = GetParam<int>(2);
                double value = count * Services.PlayerService.MaxCompanyCash;
                Services.PlayerService.AddCompanyCash(value);
                console.AddOutput($"added {value} to balance", ConsoleTextColor.green);
            } else if (whatToAdd == "nextplanet") {
                IPlanetService planetService = Services.GetService<IPlanetService>();
                if (planetService.HasNextPlanet) {
                    PlanetInfo nextPlanet = planetService.GetPlanet(planetService.NextPlanetId);
                    Services.PlayerService.AddCurrencies(nextPlanet.Data.Prices);
                    console.AddOutput($"Price for planet {nextPlanet.Id} added", ConsoleTextColor.white, true);
                } else {
                    console.AddOutput("Don't exists next planet", ConsoleTextColor.red, true);
                }
            } else if (whatToAdd == "all") {
                Services.PlayerService.AddCompanyCash((1e40));
                Services.PlayerService.AddPlayerCash((1e40).ToCurrencyNumber());
                Services.PlayerService.AddSecurities((1e40).ToCurrencyNumber());
                Services.PlayerService.AddCoins(1000000);
            } else if (whatToAdd == "module") {
                var modules = Services.Modules.Modules;
                for (int i = 0; i < modules.Count; i++) {
                    if (modules[i].State != ShipModuleState.Opened) {
                        Services.PlayerService.AddCurrency(modules[i].Data.Currency);
                        break;
                    }
                }
            } else if (whatToAdd == "reports") {
                var secretaryService = Services.SecretaryService;
                int reportsCount = GetParam<int>(2);
                foreach (ManagerInfo manager in Services.ManagerService.HiredManagers) {
                    secretaryService.ForceAddReports(managerId: manager.Id, reportsCount: reportsCount);
                }
            } else if (whatToAdd == "rewards") {
                int count = GetParam<int>(2);
                Services.RewardsService.AddAvailableRewards(count);
            } else if (whatToAdd == "points") {
                int count = GetParam<int>(2);
                Services.PlayerService.AddStatusPoints(count);
            } else if (whatToAdd == "playercash") {
                float count = GetParam<float>(2);
                Services.PlayerService.AddPlayerCash(new CurrencyNumber(count));
            } else if (whatToAdd == "forproduct") {
                var product = Services.ResourceService.PersonalProducts.ProductCollection
                    .Where(p => !Services.PlayerService.IsProductPurchased(p.id))
                    .OrderBy(p => p.price)
                    .FirstOrDefault();
                if(product != null ) {
                    Services.PlayerService.AddCompanyCash(product.price);
                }
            } else {
                console.AddOutput($"invalid name {whatToAdd}", ConsoleTextColor.orange);
            }

        }
    }

    public class TimeCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);
            /* 
            int generatorId = GetParam<int>();
            var generator = GameObject.FindObjectsOfType<Generator>().Where(gen => gen.GeneratorId == generatorId).FirstOrDefault();

            if (generator != null) {
                console.AddOutput($"time for generator is => {Services.GenerationService.Generators.GetCurrentValueForTime(generator.GeneratorId)}",
                    ConsoleTextColor.yellow, true);
            } else {
                console.AddOutput($"Generator => {generatorId} not founded", ConsoleTextColor.yellow, true);
            }*/
        }
    }

    public class ResetCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string what = GetParam<string>(1).ToLower();
            switch(what) {
                case "planets": {
                        (Services.PlanetService as SaveableGameBehaviour).LoadDefaults();
                        (Services.Modules as SaveableGameBehaviour).LoadDefaults();
                        Services.SaveService.SaveAll();
                        console.AddOutput("planets and modules cleared", ConsoleTextColor.yellow, true);
                    }
                    break;
            }
        }
    }

    public class SpawnCommand : Command {
        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);

            string whatSpawn = GetParam<string>(1);
            if (whatSpawn == "achievment") {
                int achievmentId = GetParam<int>(2);
                string achievmentName = GetParam<string>(3);
                GameEvents.OnAchievmentCompleted(new AchievmentInfo(achievmentId, achievmentName));
            }
        }
    }

    public class SkipCommand : Command {

        public override void Run(string commandText, IConsoleService console) {
            base.Run(commandText, console);
            string whatSkip = GetParam<string>(1);
            switch (whatSkip.Trim().ToUpper()) {
                case "TUTORIAL": SkipTutorial(console); break;
            }
        }

        private void SkipTutorial(IConsoleService console) {
            var tutorialService = Services.TutorialService;
            tutorialService.SkipTutorial();
            console.AddOnScreenText("tutorial skipped...");
        }
    }

    public abstract class Command : GameElement {

        private string rawText;


        private string[] Tokens => rawText.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

        public string Name {
            get {
                var tokens = Tokens;
                return tokens.Length > 0 ? tokens[0].ToLower() : string.Empty;
            }
        }

        public T GetParam<T>(int paramterIndex = 1) {
            var tokens = Tokens;
            try {
                if (tokens.Length > paramterIndex) {
                    if (typeof(T) == typeof(int)) {
                        return (T)(object)tokens[paramterIndex].AsInt();
                    } else if (typeof(T) == typeof(float)) {
                        return (T)(object)tokens[paramterIndex].AsFloat();
                    } else if (typeof(T) == typeof(bool)) {
                        return (T)(object)tokens[paramterIndex].AsBool();
                    } else if (typeof(T) == typeof(string)) {
                        return (T)(object)tokens[paramterIndex];
                    } else if(typeof(T) == typeof(double)) {
                        return (T)(object)tokens[paramterIndex].AsDouble();
                    } else {
                        return default(T);
                    }
                }
            } catch(System.Exception exception ) {
                Debug.LogException(exception);
            }
            return default(T);
        } 

        public virtual void Run(string commandText, IConsoleService console) {
            rawText = commandText;
        }
    }

    #endregion
}