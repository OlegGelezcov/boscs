using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlatformUtils {

	
	private static Dictionary<string, int> _ios = new Dictionary<string, int>
	{
		{"iPhone10,3", 98}, // iPhone X (Verizon/Sprint/China/A1865)
		{"iPhone10,6", 98}, // iPhone X (AT&T/T-Mobile/Global/A1901)
		{"iPhone11,8", 98}, // iPhone Xs Max
		{"iPhone11,2", 98}, // iPhone Xs
		{"iPhone11,6", 98}, // iPhone XR
	};
	private static Dictionary<string, int> _devicesWithDrop = new Dictionary<string, int> // a drop
	{
		{"HMA-L29", 50}, //HUAWEI Mate
		{"ONEPLUS A6013", 50}, //OnePlus6T
	};
	private static Dictionary<string, int> _android = new Dictionary<string, int>
	{
		{"Pixel 3", 100},
		{"Pixel 3 XL", 100},
		
		{"ONEPLUS A6003", 100}, // OnePlus 6
		
		{"LM-V405", 100}, // LG V40
		{"LM-V409N", 100}, // LG V40
		{"LM-Q910", 100}, // LG G7 One
		{"LG-G710", 100}, // LG G7 ThinQ 
		{"LM-G710", 100}, // LG G7 ThinQ 
		{"LM-G710N", 100}, // LG G7 ThinQ 
		{"LM-G710VM", 100}, // LG G7 ThinQ 
		{"LM-Q850", 100}, // LG G7 fit 
		
		{"ASUS_T00F", 100}, // ZenFone 5 
		{"ASUS_T00J", 100}, // ZenFone 5 
		{"ASUS_T00K", 100}, // ZenFone 5 
		{"ASUS_X00QD", 100}, // ZenFone 5 
		{"ASUS_X00QSA", 100}, // ZenFone 5 
		{"ZE620KL", 100}, // ZenFone 5 
		{"ZS620KL", 100}, // ZenFone 5Z 
		{"ASUS_Z01RD", 100}, // ZenFone 5Z 
		

		{"LYA-L0C", 100}, // HUAWEI Mate 20 Pro
		{"LYA-L29", 100}, // HUAWEI Mate 20 Pro
		{"SNE-LX1", 100}, // HUAWEI Mate 20 lite
		{"INE-AL00", 100}, // HUAWEI nova 3i
		{"INE-LX1", 100}, // HUAWEI nova 3i
		{"INE-TL00", 100}, // HUAWEI nova 3i
		
		{"motorola one", 100}, // motorola one 
		{"moto x4", 100}, // motorola one power 
		{"motorola one power", 100}, // motorola one power 
		
		{"PH-1", 100}, // The Essential Phone
		
		{"Redmi Note 6 Pro", 100}, // Redmi Note 6 Pro
		{"POCOPHONE F1", 100}, // POCO F1 
		{"MI 8 Explorer Edition", 100}, // MI 8 Explorer Edition 
		{"MI 8", 100}, // MI 8
		{"MI 8 Lite", 100}, // MI 8 Lite
		{"Platina", 100}, // MI 8 Lite
		{"MI 8 SE", 100}, // MI 8 SE
		{"sirius", 100}, // MI 8 SE
		{"MI 8 Pro", 100}, // MI 8 UD 
		{"MI 8 UD", 100}, // MI 8 UD 
		{"equuleus", 100}, // MI 8 UD 

		
		{"vivo 1727", 100}, // V9 
		{"vivo 1723", 100}, // V9 6GB 
		{"vivo 1851", 100}, // V9 Pro
		
		{"LLD-AL20", 100}, // Honor 9N
		
		{"Nokia X5", 100}, // Nokia X5
		{"Nokia X6", 100}, // Nokia X6
		{"Nokia 8.1", 1000}, // Nokia 8.1
		{"Nokia 7.1", 100}, // Nokia 7.1

	};

	public static bool IsPhoneXResolution()
	{
		return _ios.ContainsKey(SystemInfo.deviceModel);
	}

	public static int GetNotchOffset()
	{
		var devcieId = SystemInfo.deviceModel;
		
#if UNITY_IOS
		if (_ios.ContainsKey(devcieId))
			return _ios[devcieId];
#endif
		return 0;
	}
}
