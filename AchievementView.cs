using Bos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Achievement))]
public class AchievementView : GameBehaviour
{
    private Achievement _achi;

    public GameObject CompletionBorder;
    public Image Icon;
    public Text Name;
    public Text Requirement;
    public Text AchiPoints;

    public Color NameUncompletedColor;
    public Color NameCompletedColor;

    public Color RequirementUncompletedColor;
    public Color RequirementCompletedColor;

    public override  void Start()
    {
        _achi = GetComponent<Achievement>();
        Icon.sprite = _achi.Icon;
        Name.text = _achi.Name;
        AchiPoints.text = _achi.Points.ToString();
    }

    public override void Update() {
        base.Update();

        CompletionBorder.SetActive(_achi.IsCompleted);

        if (_achi.IsCompleted) {
            Name.color = NameCompletedColor;
            AchiPoints.color = NameCompletedColor;
            Requirement.color = RequirementCompletedColor;

            Requirement.text = string.Format("{0}/{0}", _achi.TargetCount);

        } else {
            Name.color = NameUncompletedColor;
            AchiPoints.color = NameUncompletedColor;
            Requirement.color = RequirementUncompletedColor;

            if (Services.TransportService.HasUnits(_achi.TargetGeneratorId)) {
                Requirement.text = string.Format("{0}/{1}", Services.TransportService.GetUnitTotalCount(_achi.TargetGeneratorId), _achi.TargetCount);
            } else {
                Requirement.text = string.Format("{0}/{1}", 0, _achi.TargetCount);
            }
        }
    }

}