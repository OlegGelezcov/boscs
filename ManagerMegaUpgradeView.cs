using System.Collections;
using System.Collections.Generic;
using Bos;
using UnityEngine;
using UnityEngine.UI;

public class ManagerMegaUpgradeView : GameBehaviour {

	public Text MegaTextCost;	
	public Text MegaRollbackText;
	public Text MegaEfficiencyText;
	
	public Text MegaRollbackIncrementText;
	public Text MegaEfficiencyIncrementText;


	public void Fill(ManagerInfo manager)
	{
		var megaData = Services.ResourceService.ManagerImprovements.MegaImprovement;
		MegaRollbackIncrementText.text = $"+{megaData.RollbackIncrement * 100}%";
		MegaEfficiencyIncrementText.text = $"+{megaData.EfficiencyIncrement * 100}%";
		MegaTextCost.text = $"{megaData.CoinPrice}";
		
		MegaRollbackText.text = $"{(int)(manager.MaxEfficiency * 100)}%";
		MegaEfficiencyText.text = $"{(int)(manager.MaxEfficiency * 100)}%";
	}
}
