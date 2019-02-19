using Bos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : GameBehaviour
{
    public string Name;
    public string Description;
    public int GeneratorIdToManage;
    public double Cost
        => Services.ManagerService.GetManager(GeneratorIdToManage).Cost; //GameData.instance.managers[GeneratorIdToManage].BaseCost;
}
