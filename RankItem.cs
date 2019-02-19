using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankItem : MonoBehaviour
{
    public Text RankLabel;
    public Text NameLabel;
    public Text ValueLabel;

    public void Set(int rank, string name, object value)
    {
        RankLabel.text = rank.ToString();
        NameLabel.text = name;

        if (value is string)
            ValueLabel.text = value as string;
        else
            ValueLabel.text = value.ToString();
    }
   
}
