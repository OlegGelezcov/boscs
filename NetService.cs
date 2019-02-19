namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Text;
    using System;
    using UnityEngine.Networking;
    using UDebug = UnityEngine.Debug;
    using Newtonsoft.Json.Linq;
    using Bos.Data;
    using System.Linq;
    using Bos.Debug;
    using JWT;
    using Newtonsoft.Json;

    public class NetService : MonoBehaviour, INetService {

        public const string balanceUrl = "http://bos.heatherglade.com/_dev/get_planets_balance";
        public const string balanceShipUrl = "http://bos.heatherglade.com/_dev/get_rocket_balance";
        public const string kBalanceMechanicUrl = "http://bos.heatherglade.com/_dev/get_mech_balance";
        public const string kSecretaryBalanceUrl = "http://bos.heatherglade.com/_dev/get_sec_balance";
        public const string kTransportStrengthUrl = "http://bos.heatherglade.com/_dev/get_transport_sec_coeff";
        public const string kBankUrl = "http://bos.heatherglade.com/_dev/get_bank_balance";
        public const string kPlanetsTransportUrl = "http://bos.heatherglade.com/_dev/get_plantes_transport";

        public const string kPersonalImprovements = "http://bos.heatherglade.com/_dev/get_personal_improvements";
        public const string kManagerImprovements = "http://bos.heatherglade.com/_dev/get_managers_improvements";
        public const string kPromoUrl = "http://bos.heatherglade.com/get_bonus";



        private const string serverLogin = "prcode";
        private const string serverPassword = "CrjhjKtnj";
        private readonly string authKey = "AUTHORIZATION";
        public const string secretKey = "BA9A16CF942BC68EB56BA3C2D28D1";
        public const int kCountPlanetParameters = 8;
        public const int kEarthId = 0;

        private IEnumerator GetPromoBonusImpl(string promoCode, Action<string, int> onSuccess, Action<string> onError ) {
            PromoMessageData data = PromoMessageData.FromPromoCode(promoCode);
            string url = kPromoUrl + "?defs=" + data.ToJsonBase64();
            print("REQUEST URL");
            print(url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            
            request.SetRequestHeader(authKey, AuthHeader);
            print("REQUEST HEADER");
            print($"{authKey}: {request.GetRequestHeader(authKey)}");

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;
            if(operation.webRequest.isHttpError) {
                onError?.Invoke(operation.webRequest.error);
                GameEvents.PromoReceived.OnNext(new GameEvents.PromoCodeInfo {
                    Code = data.bonus_key,
                    Count = 0,
                    IsSuccess = false
                });
                yield break;
            }
            if(operation.webRequest.isNetworkError ) {
                onError?.Invoke(operation.webRequest.error);
                GameEvents.PromoReceived.OnNext(new GameEvents.PromoCodeInfo {
                    Code = data.bonus_key,
                    Count = 0,
                    IsSuccess = false
                });
                yield break;
            }

            string text = request.downloadHandler.text;
            string encodedText = JsonWebToken.Decode(text, secretKey);
            JObject webObj = JObject.Parse(encodedText);
            JToken responseToken = webObj["response"];
            if(responseToken == null ) {
                onError?.Invoke($"key - response not found");
                GameEvents.PromoReceived.OnNext(new GameEvents.PromoCodeInfo {
                    Code = data.bonus_key,
                    Count = 0,
                    IsSuccess = false
                });
                yield break;
            }

            JToken codeToken = responseToken["code"];
            if(codeToken == null ) {
                onError?.Invoke("code key not found");
                GameEvents.PromoReceived.OnNext(new GameEvents.PromoCodeInfo {
                    Code = data.bonus_key,
                    Count = 0,
                    IsSuccess = false
                });
                yield break;
            }

            if(codeToken.Value<string>() != "success") {
                onError?.Invoke($"return code => {codeToken.Value<string>()}");
                GameEvents.PromoReceived.OnNext(new GameEvents.PromoCodeInfo {
                    Code = data.bonus_key,
                    Count = 0,
                    IsSuccess = false
                });
                yield break;
            }

            JToken bonusQuantity = responseToken["data"]["bonus_quantity"];
            int quantity = bonusQuantity.Value<int>();
            onSuccess?.Invoke(data.bonus_key, quantity);
            GameEvents.PromoReceived.OnNext(new GameEvents.PromoCodeInfo {
                Code = data.bonus_key,
                Count = quantity,
                IsSuccess = true
            });
        }


        private IEnumerator GetManagerImprovementsImpl(Action<ManagerImproveData> onSuccess, Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(kManagerImprovements));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JsonWebToken.Decode(result, secretKey);
                        print(json);
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];
                        var efficiencyImrpovements = ParseManagerEfficiencyImprovements(arr);
                        var rollbackImprovements = ParseManagerRollbackImprovements(arr);
                        var megaImprovement = ParseMegaManagerImprovement(arr);
                        ManagerImproveJsonData jsonData = new ManagerImproveJsonData {
                            efficiencyImprovements = efficiencyImrpovements,
                            rollbackImprovements = rollbackImprovements,
                            megaImprovement = megaImprovement,
                        };
                        ManagerImproveData improveData = new ManagerImproveData(jsonData);
                        onSuccess?.Invoke(improveData);

                    } catch(Exception exception ) {
                        UDebug.LogError(exception.Message.Bold());
                        UDebug.LogError(exception.StackTrace.Bold());
                        onError?.Invoke($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                    }
                } else {
                    UDebug.LogError(operation.webRequest.error);
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                UDebug.LogError($"operation {nameof(GetManagerImprovementsImpl)} is not done".Bold());
                onError?.Invoke($"operation {nameof(GetManagerImprovementsImpl)} is not done");
            }
        }

        public static Dictionary<int, ManagerEfficiencyImproveJsonData> ParseManagerEfficiencyImprovements(JToken dataToken ) {
            JToken effToken = dataToken["e"];
            Dictionary<int, ManagerEfficiencyImproveJsonData> result =
                new Dictionary<int, ManagerEfficiencyImproveJsonData>();
            int level = 1;
            foreach(JToken token in effToken ) {
                float efficiencyIncrement = token.Value<float>(0);
                int coinsPrice = token.Value<int>(1);
                result.Add(level, new ManagerEfficiencyImproveJsonData {
                    level = level,
                    efficiencyIncrement = efficiencyIncrement,
                    coinsPrice = coinsPrice
                });
                level++;
            }
            return result;
        }

        public static Dictionary<int, ManagerRollbackImproveJsonData> ParseManagerRollbackImprovements(JToken dataToken ) {
            JToken rollbackToken = dataToken["rb"];
            Dictionary<int, ManagerRollbackImproveJsonData> result =
                new Dictionary<int, ManagerRollbackImproveJsonData>();
            int level = 1;
            foreach(JToken token in rollbackToken) {
                float rollbackIncrement = token.Value<float>(0);
                int coinsPrice = token.Value<int>(1);
                result.Add(level, new ManagerRollbackImproveJsonData {
                    level = level,
                    rollbackIncrement = rollbackIncrement,
                    coinsPrice = coinsPrice
                });
                level++;
            }
            return result;
        }

        public static MegaManagerImproveJsonData ParseMegaManagerImprovement(JToken dataToken) {
            JToken megaToken = dataToken["mega"];
            float efficiencyIncrement = megaToken.Value<float>(0);
            float rollbackIncrement = megaToken.Value<float>(1);
            int coinPrice = megaToken.Value<int>(2);
            return new MegaManagerImproveJsonData {
                efficiencyIncrement = efficiencyIncrement,
                rollbackIncrement = rollbackIncrement,
                coinPrice = coinPrice,
            };
        }

        private IEnumerator GetPersonalImprovementsImpl(
            Action<PersonalImprovementsData> onSuccess,
            Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(kPersonalImprovements));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JsonWebToken.Decode(result, secretKey);
                        print(json);
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];

                        PersonalConvertData personalConvertData = ParsePersonalConvertData(arr["c"]);
                        List<StatusPointData> statusPoints = ParseStatusPoints(arr["s"]);
                        Dictionary<ProductType, List<ProductData>> products = ParseProducts(arr["i"]);
                        onSuccess?.Invoke(new PersonalImprovementsData(personalConvertData, statusPoints));
                        UDebug.Log("success loading personal improvements");

                    } catch(Exception exception) {
                        UDebug.LogError(exception.Message.Bold());
                        UDebug.LogError(exception.StackTrace.Bold());
                        onError?.Invoke($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                    }
                } else {
                    UDebug.LogError(operation.webRequest.error);
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                UDebug.LogError($"operation {nameof(GetPersonalImprovementsImpl)} is not done".Bold());
                onError?.Invoke($"operation {nameof(GetPersonalImprovementsImpl)} is not done");
            }
        }

        public static PersonalConvertData ParsePersonalConvertData(JToken token) {
            if(token == null ) {
                throw new UnityException($"token 'c' in data NULL, no personal convert data passed");
            }
            float officialValue = token["o"]?.Value<float>() ?? 0.0f;
            float unofficialValue = token["uo"]?.Value<float>() ?? 0.0f;
            float loosePercent = token["p"]?.Value<float>() ?? 0.0f;
            return new PersonalConvertData(officialValue, unofficialValue, loosePercent);
        }

        public static List<StatusPointData> ParseStatusPoints(JToken token) {
            if(token == null ) {
                throw new UnityException($"token 's' in data NULL, no status points data passed");
            }

            List<StatusPointData> result = new List<StatusPointData>();
            int level = 1;
            foreach(JToken element in token ) {
                int points = element.Value<int>();
                StatusPointData data = new StatusPointData(level, points, GetPlanetForStatus(level));
                result.Add(data);
                level++;
            }
            return result;
        }

        private static int GetPlanetForStatus(int status)
            => StatusPlanetMap.ContainsKey(status) ? StatusPlanetMap[status] : 0;

        private readonly static Dictionary<int, int> StatusPlanetMap = new Dictionary<int, int> {
            [1] = 0, [2] = 0, [3] = 1, [4] = 1, [5] = 1, [6] = 2, [7] = 2, [8] = 2, [9] = 2,
            [10] = 3, [11] = 3, [12] = 3, [13] = 3, [14] = 3, [15] = 4, [16] = 4, [17] = 4,
            [18] = 4, [19] = 4, [20] = 4, [21] = 5, [22] = 5, [23] = 5, [24] = 5, [25] = 5, [26] = 5, [27] = 5
        };

        public static Dictionary<ProductType, List<ProductData>> ParseProducts(JToken token) {
            
           
            if(token == null ) {
                throw new UnityException($"token 'i' in data NULL, no status products data passed");
            }      
            Dictionary<ProductType, List<ProductData>> allProducts = new Dictionary<ProductType, List<ProductData>>();
            /*
           JToken personalToken = token["p"];
           int id = 0;
           List<ProductData> personalProducts = new List<ProductData>();
           foreach(JToken element in personalToken ) {
               int points = element.Value<int>(0);
               int price = element.Value<int>(1);
               ProductData product = new ProductData(ProductType.Transport, id, price, points);
               personalProducts.Add(product);
               id++;
           }
           allProducts.Add(ProductType.Transport, personalProducts);

           JToken estateToken = token["a"];
           List<ProductData> estateProducts = new List<ProductData>();
           foreach(JToken element in estateToken ) {
               int points = element.Value<int>(0);
               int price = element.Value<int>(1);
               ProductData product = new ProductData(ProductType.RealEstate, id, price, points);
               estateProducts.Add(product);
               id++;
           }
           allProducts.Add(ProductType.RealEstate, estateProducts);

           JToken carToken = token["c"];
           List<ProductData> carProducts = new List<ProductData>();
           foreach(JToken element in carToken) {
               int points = element.Value<int>(0);
               int price = element.Value<int>(1);
               ProductData product = new ProductData(ProductType.Car, id, price, points);
               carProducts.Add(product);
               id++;
           }
           allProducts.Add(ProductType.Car, carProducts);

           JToken aircraftToken = token["f"];
           List<ProductData> aircraftProducts = new List<ProductData>();
           foreach (JToken element in aircraftToken) {
               int points = element.Value<int>(0);
               int price = element.Value<int>(1);
               ProductData product = new ProductData(ProductType.Airplane, id, price, points);
               aircraftProducts.Add(product);
               id++;
           }
           allProducts.Add(ProductType.Airplane, aircraftProducts);

           JToken boatToken = token["y"];
           List<ProductData> boatProducts = new List<ProductData>();
           foreach(JToken element in boatToken) {
               int points = element.Value<int>(0);
               int price = element.Value<int>(1);
               ProductData product = new ProductData(ProductType.Boat, id, price, points);
               boatProducts.Add(product);
               id++;
           }
           allProducts.Add(ProductType.Boat, boatProducts);



           JToken groundToken = token["pl"];
           List<ProductData> groundProducts = new List<ProductData>();
           foreach(JToken element in groundToken) {
               int points = element.Value<int>(0);
               int price = element.Value<int>(1);
               ProductData product = new ProductData(ProductType.Ground, id, price, points);
               groundProducts.Add(product);
               id++;
           }
           allProducts.Add(ProductType.Ground, groundProducts);

           JToken spaceshipToken = token["ss"];
           List<ProductData> spaceShipProducts = new List<ProductData>();
           foreach(JToken element in spaceshipToken) {
               int points = element.Value<int>(0);
               int price = element.Value<int>(1);
               ProductData product = new ProductData(ProductType.SpaceShip, id, price, points);
               spaceShipProducts.Add(product);
               id++;
           }
           allProducts.Add(ProductType.SpaceShip, spaceShipProducts);


*/
            return allProducts;
        }

        private IEnumerator GetPlanetsTransportImpl(Action<List<GeneratorData>> onSuccess, Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(kPlanetsTransportUrl));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JsonWebToken.Decode(result, secretKey);
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];
                        List<GeneratorData> generators = new List<GeneratorData>();
                        int generatorId = 10;
                        foreach(JToken token in arr) {
                            GeneratorJsonData jsonData = new GeneratorJsonData {
                                id = generatorId,
                                baseCost = token.Value<double>(1),
                                incrementFactor = token.Value<double>(2),
                                baseGeneration = token.Value<double>(3),
                                timeToGenerate = token.Value<float>(4),
                                //coinPrice = token.Value<int>(5),
                                enhancePrice = token.Value<int>(6),
                                profitIncrementFactor = token.Value<double>(7)
                            };
                            generators.Add(new GeneratorData(jsonData, GeneratorType.Planet));
                            generatorId++;
                        }
                        onSuccess?.Invoke(generators);
                    } catch (Exception exception ) {
                        UDebug.LogError(exception.Message.Bold());
                        UDebug.LogError(exception.StackTrace.Bold());
                        onError?.Invoke($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                    }
                } else {
                    UDebug.LogError(operation.webRequest.error);
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                UDebug.LogError($"operation {nameof(GetPlanetsTransportImpl)} is not done".Bold());
                onError?.Invoke($"operation {nameof(GetPlanetsTransportImpl)} is not done");
            }
        }

        private IEnumerator GetBankImpl(Action<List<BankLevelData>> onSuccess, Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(kBankUrl));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JsonWebToken.Decode(result, secretKey);
                        //print(json.Colored(ConsoleTextColor.teal).Bold());
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];
                        List<BankLevelData> bankLevels = new List<BankLevelData>();
                        int level = 1;
                        foreach(JToken token in arr) {
                            int price = token.Value<int>(0);
                            float profit = token.Value<float>(1);
                            float interval = token.Value<float>(2);
                            BankLevelData data = new BankLevelData(level, price, profit, interval);
                            bankLevels.Add(data);
                            level++;
                        }
                        onSuccess?.Invoke(bankLevels);
                    } catch(Exception exception ) {
                        UDebug.LogError(exception.Message.Bold());
                        UDebug.LogError(exception.StackTrace.Bold());
                        onError?.Invoke($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                    }
                } else {
                    UDebug.LogError(operation.webRequest.error);
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                UDebug.LogError($"operation {nameof(GetBankImpl)} is not done".Bold());
                onError?.Invoke($"operation {nameof(GetBankImpl)} is not done");
            }
        }

        private IEnumerator GetTransportStrengthImpl(Action<List<UnitStrengthData>> onSuccess, Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(kTransportStrengthUrl));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JsonWebToken.Decode(result, secretKey);
                        print(json.Colored(ConsoleTextColor.teal).Bold());
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];

                        List<UnitStrengthData> list = new List<UnitStrengthData>();
                        int generatorId = 0;
                        foreach(JToken token in arr) {
                            float strength = token.Value<float>();
                            UnitStrengthData data = new UnitStrengthData(generatorId, strength);
                            generatorId++;
                            list.Add(data);
                        }
                        onSuccess?.Invoke(list);

                    } catch (System.Exception exception) {
                        UDebug.LogError(exception.Message.Bold());
                        UDebug.LogError(exception.StackTrace.Bold());
                        onError?.Invoke($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                    }
                } else {
                    UDebug.LogError(operation.webRequest.error);
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                UDebug.LogError($"operation {nameof(GetTransportStrengthImpl)} is not done".Bold());
                onError?.Invoke($"operation {nameof(GetTransportStrengthImpl)} is not done");
            }
        }

        private IEnumerator GetSecretaryBalanceImpl(Action<List<SecretaryData>> onSuccess, Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(kSecretaryBalanceUrl));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JsonWebToken.Decode(result, secretKey);
                        //print(json.Colored(ConsoleTextColor.teal).Bold());
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];

                        int planetId = 0;
                        List<SecretaryData> secretaries = new List<SecretaryData>();
                        foreach (JToken token in arr) {
                            int priceForFirst = token.Value<int>(0);
                            int priceIncreasing = token.Value<int>(1);
                            int reportCountPerSecretary = token.Value<int>(2);
                            float fatigue = token.Value<float>(3);
                            int reportCountProcessedPer10Seconds = token.Value<int>(4);
                            double cashPrice = token.Value<double>(5);
                            secretaries.Add(new SecretaryData(planetId, priceForFirst, priceIncreasing, reportCountPerSecretary, fatigue, reportCountProcessedPer10Seconds, cashPrice));
                            planetId++;
                        }
                        onSuccess?.Invoke(secretaries);

                    } catch(Exception exception) {
                        UDebug.LogError(exception.Message.Bold());
                        UDebug.LogError(exception.StackTrace.Bold());
                        onError?.Invoke($"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
                    }
                } else {
                    UDebug.LogError(operation.webRequest.error);
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                UDebug.LogError($"operation {nameof(GetSecretaryBalanceImpl)} is not done".Bold());
                onError?.Invoke($"operation {nameof(GetSecretaryBalanceImpl)} is not done");
            }
        }

        private IEnumerator GetBalanceMechanicImpl(System.Action<List<MechanicData>> onSuccess, System.Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(kBalanceMechanicUrl));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JWT.JsonWebToken.Decode(result, secretKey);
                        //print(json.Colored(ConsoleTextColor.green));
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];

                        int planetId = 0;
                        List<MechanicData> mechanics = new List<MechanicData>();

                        foreach(JToken token in arr) {
                            int priceForFirstMechanic = token.Value<int>(0);
                            int priceIncreasing = token.Value<int>(1);
                            int unitCountService = token.Value<int>(2);
                            float fatigue = token.Value<float>(3);
                            int restoredPer10Seconds = token.Value<int>(4);
                            double cashPrice = token.Value<double>(5);
                            mechanics.Add(new MechanicData(planetId, priceForFirstMechanic, priceIncreasing,
                                unitCountService, fatigue, restoredPer10Seconds, cashPrice));
                            planetId++;
                        }
                        onSuccess?.Invoke(mechanics);

                    } catch(System.Exception exception) {
                        onError?.Invoke(exception.Message);
                    }
                } else {
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                onError?.Invoke("operation GetBalanceMechanic is not done");
            }
        }

        private IEnumerator GetBalanceShipImpl(System.Action<List<ShipModuleData>> onSuccess, System.Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(balanceShipUrl));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;

            if(operation.isDone) {
                if(false == operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        string json = JWT.JsonWebToken.Decode(result, secretKey);
                        //print(json.Colored(ConsoleTextColor.green));
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];

                        int moduleId = 0;
                        List<ShipModuleData> modules = new List<ShipModuleData>();
                        foreach(JToken token in arr) {
                            int planetLevel = token.Value<int>(0);
                            double companyCash = token.Value<double>(1);
                            double securities = token.Value<double>(2);
                            int coins = token.Value<int>(3);
                            if(companyCash != 0.0 ) {
                                modules.Add(new ShipModuleData(moduleId, planetLevel, Currency.CreateCompanyCash(companyCash)));
                            } else if(securities != 0.0 ) {
                                modules.Add(new ShipModuleData(moduleId, planetLevel, Currency.CreateSecurities(securities)));
                            } else {
                                modules.Add(new ShipModuleData(moduleId, planetLevel, Currency.CreateCoins(coins)));
                            }
                            moduleId++;
                        }
                        onSuccess?.Invoke(modules);

                    } catch(Exception exception ) {
                        onError?.Invoke(exception.Message);
                    }
                } else {
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                onError?.Invoke("operation GetShipBalance not done");
            }
        }

        private IEnumerator GetBalanceImpl(Action<List<PlanetServerData>> onSuccess, Action<string> onError) {
            UnityWebRequest request = UnityWebRequest.Get(FullUrl(balanceUrl));
            request.SetRequestHeader(authKey, AuthHeader);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            yield return operation;
            string json = string.Empty;
            if (operation.isDone ) {
                if (!operation.webRequest.isHttpError) {
                    try {
                        string result = operation.webRequest.downloadHandler.text;
                        json = JWT.JsonWebToken.Decode(result, secretKey);
                        JObject parent = JObject.Parse(json);
                        JToken arr = parent["response"]["data"];

                        int planetId = kEarthId;
                        List<PlanetServerData> planets = new List<PlanetServerData>();
                    
                        foreach (JToken token in arr) {
                            PlanetServerData planet = new PlanetServerData();
                            planet.Id = planetId;
                            planet.ProfitMultiplier = token.Value<double>(0);
                            planet.TimeFactor= token.Value<double>(1);
                            planet.GlobalUpgradeMult = token.Value<double>(2);
                            planet.CompanyCashPrice= token.Value<double>(3);
                            planet.SecuritiesPrice= token.Value<double>(4);
                            planet.OpeningTime = token.Value<double>(5);
                            planet.TransportUnityPriceMult = token.Value<double>(6);
                            planet.GeneratorsMult = token[7].ToObject<List<double>>();
                            planet.ManagersMult =  token[8].ToObject<List<double>>();
                            planet.UpgradeMult =  token[9].ToObject<List<double>>();
                            print($"planet json count => {token.Count()}".Bold().Colored(ConsoleTextColor.green));
                            planets.Add(planet);
                            planetId++;
                        }
                        onSuccess?.Invoke(planets);
                    } catch(Exception exception ) {
                        UDebug.Log(json);
                        onError?.Invoke(exception.Message);
                    }

                } else {
                    onError?.Invoke(operation.webRequest.error);
                }
            } else {
                onError?.Invoke("operation not done");
            }
        }

        public static string AuthHeader {
            get {
                string auth = serverLogin + ":" + serverPassword;
                auth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
                auth = "Basic " + auth;
                return auth;
            }
        }

        public static string FullUrl(string baseUrl)
            => baseUrl + "?defs=" + GetEncodeQuery();



        public static string DeviceId {
            get {
                var deviceId = SystemInfo.deviceUniqueIdentifier;
#if UNITY_IOS
                deviceId =  UnityEngine.iOS.Device.vendorIdentifier;
#endif
                return deviceId;
            }
        }

        public static string DeviceOS {
            get {
                var deviceOS = SystemInfo.operatingSystem;
#if UNITY_EDITOR
                deviceOS = "Editor";
#elif UNITY_ANDROID
                deviceOS = "android";
#elif UNITY_IOS
                deviceOS = "ios";
#endif
                return deviceOS;
            }
        }

        private static string GetEncodeQuery() {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var deviceOS = SystemInfo.operatingSystem;
#if UNITY_ANDROID
            deviceOS = "android";
            deviceId = SystemInfo.deviceUniqueIdentifier;
#elif UNITY_IPHONE
        deviceOS = "ios";
        deviceId =  UnityEngine.iOS.Device.vendorIdentifier; //Device.vendorIdentifier;
#endif

#if UNITY_EDITOR
            deviceId = SystemInfo.deviceUniqueIdentifier;
            deviceOS = "Editor";
#endif
            var query = "{ \"device_id\": " + "\"" + deviceId + "\"" + ", \"device_os\" : " + "\"" + deviceOS + "\"" + "}";
            var bytes = Encoding.ASCII.GetBytes(query);
            var encodedText = Convert.ToBase64String(bytes).Trim('=');
            //Debug.LogError(encodedText);
            return encodedText;
        }

#region INetService
        public void Setup(object data = null) {
        }

        public void UpdateResume(bool pause)
            => UDebug.Log($"{nameof(NetService)}.{nameof(UpdateResume)}() => {pause}");

        public void GetBalance(Action<List<PlanetServerData>> onSuccess, Action<string> onError) 
            => StartCoroutine(GetBalanceImpl(onSuccess, onError));

        public void GetBalanceShip(System.Action<List<ShipModuleData>> onSuccess, System.Action<string> onError) 
            => StartCoroutine(GetBalanceShipImpl(onSuccess, onError));

        public void GetBalanceMechanic(Action<List<MechanicData>> onSuccess, Action<string> onError) 
            => StartCoroutine(GetBalanceMechanicImpl(onSuccess, onError));

        public void GetSecretaryBalance(Action<List<SecretaryData>> onSuccess, Action<string> onError)
            => StartCoroutine(GetSecretaryBalanceImpl(onSuccess, onError));

        public void GetTransportStrength(Action<List<UnitStrengthData>> onSuccess, Action<string> onError)
            => StartCoroutine(GetTransportStrengthImpl(onSuccess, onError));

        public void GetBank(Action<List<BankLevelData>> onSuccess, Action<string> onError)
            => StartCoroutine(GetBankImpl(onSuccess, onError));

        public void GetPlanetsTransport(Action<List<GeneratorData>> onSuccess, Action<string> onError)
            => StartCoroutine(GetPlanetsTransportImpl(onSuccess, onError));

        public void GetPersonalImprovements(Action<PersonalImprovementsData> onSuccess, Action<string> onError)
            => StartCoroutine(GetPersonalImprovementsImpl(onSuccess, onError));

        public void GetManagerImprovements(Action<ManagerImproveData> onSuccess, Action<string> onError)
            => StartCoroutine(GetManagerImprovementsImpl(onSuccess, onError));

        public void GetPromoBonus(string promoCode, Action<string, int> success, Action<string> onError)
            => StartCoroutine(GetPromoBonusImpl(promoCode, success, onError));
#endregion

    }

    public class PromoMessageData {
        public string device_id;
        public string device_os;
        public string bonus_key;

        public string ToJsonBase64() {
            string json = JsonConvert.SerializeObject(this);
            UDebug.Log(json);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            return Convert.ToBase64String(bytes).Trim('=');
        }

        public static PromoMessageData FromPromoCode(string promoCode) {
            return new PromoMessageData {
                device_id = NetService.DeviceId,
                device_os = NetService.DeviceOS,
                bonus_key = promoCode
            };
        }
    }



    public interface INetService : IGameService {
        void GetBalance(Action<List<PlanetServerData>> onSuccess, Action<string> onError);
        void GetBalanceShip(Action<List<ShipModuleData>> onSuccess, Action<string> onError);
        void GetBalanceMechanic(Action<List<MechanicData>> onSuccess, Action<string> onError);
        void GetSecretaryBalance(Action<List<SecretaryData>> onSuccess, Action<string> onError);
        void GetTransportStrength(Action<List<UnitStrengthData>> onSuccess, Action<string> onError);
        void GetBank(Action<List<BankLevelData>> onSuccess, Action<string> onError);
        void GetPlanetsTransport(Action<List<GeneratorData>> onSuccess, Action<string> onError);

        void GetPersonalImprovements(Action<PersonalImprovementsData> onSuccess, Action<string> onError);
        void GetManagerImprovements(Action<ManagerImproveData> onSuccess, Action<string> onError);
        void GetPromoBonus(string promoCode, Action<string, int> success, Action<string> onError);

    }

    
}