using System;
using System.Collections;
using System.Collections.Generic;
using Bos;
using Facebook.Unity;
using UnityEngine;

public class FacebookEventUtils {

	public static void LogApplyGeneratorEvent (string generatorId) {
		Debug.Log("FacebookEventUtils: LogApplyGeneratorEvent " + generatorId);

        if (FB.IsInitialized) {
            var parameters = new Dictionary<string, object>();
            parameters["GeneratorId"] = generatorId;
            parameters["Total Seconds"] = getSpanFromFirstStart().ToTextMark();
            FB.LogAppEvent(
                "ApplyGenerator",
                0,
                parameters
            );
        } else {
            Debug.Log("LogAppGeneratorEvent(): facebook not initialized");
        }
	}
	
	public static void LogSellBusinessToInvestorsEvent (int sellNumber) {
		Debug.Log("FacebookEventUtils: LogSellBusinessToInvestorsEvent " + sellNumber);
		var parameters = new Dictionary<string, object>();
		parameters["SellNumber"] = sellNumber;
		parameters["Total Seconds"] = getSpanFromFirstStart().ToTextMark();
		FB.LogAppEvent(
			"SellBusinessToInvestors",
			0,
			parameters
		);
	}
	
	public static void LogGameCompleteEvent () {
		Debug.Log("FacebookEventUtils: LogGameCompleteEvent");
		var parameters = new Dictionary<string, object>();
		parameters["Total Seconds"] = getSpanFromFirstStart().ToTextMark();
		FB.LogAppEvent(
			"GameComplete",
			0,
			parameters
		);
	}
	
	public static  void LogFirstHireManagerEvent (int managerId) {
		Debug.Log("FacebookEventUtils: LogFirstHireManagerEvent " + managerId);
		var parameters = new Dictionary<string, object>();
		parameters["ManagerId"] = managerId;
		parameters["Total Seconds"] = getSpanFromFirstStart().ToTextMark();
		FB.LogAppEvent(
			"FirstHireManager",
			0,
			parameters
		);
	}
	
	public static void LogFirstKickBackManagerEvent (int managerId) {
		Debug.Log("FacebookEventUtils: LogFirstKickBackManagerEvent " + managerId);
		var parameters = new Dictionary<string, object>();
		parameters["ManagerId"] = managerId;
		parameters["Total Seconds"] = getSpanFromFirstStart().ToTextMark();
		FB.LogAppEvent(
			"FirstKickBackManager",
			0,
			parameters
		);
	}	
		
	public static void LogADEvent (string place) {
		//Debug.Log("FacebookEventUtils: LogADEvent " + place);
		var parameters = new Dictionary<string, object>();
		parameters["Place"] = place;
		parameters["Total Seconds"] = getSpanFromFirstStart().ToTextMark();
		FB.LogAppEvent(
			"Advertising_ " + place,
			1,
			parameters
		);
	}
	
	public static void LogCoinSpendEvent (string id, int cost, int coin) {
		Debug.Log("FacebookEventUtils: LogADEvent " + id);
		var parameters = new Dictionary<string, object>();
		parameters["Product"] = id;
		parameters["Cost"] = cost;
		parameters["Left"] = coin;
		parameters["Total Seconds"] = getSpanFromFirstStart().ToTextMark();
		FB.LogAppEvent(
			"Coin",
			1,
			parameters
		);

        //Facebook.Unity.AppEventName.UnlockedAchievement
	}

	private static int getSpanFromFirstStart()
	{
		return (int)(DateTime.Now - LocalData.FirstOpenDate).TotalSeconds;
	}
}
