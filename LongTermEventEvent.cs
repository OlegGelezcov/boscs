using UnityEngine;
using System.Collections;
using Bos;
using Bos.UI;

public class LongTermEventEvent : LocalEvent
{
    public float ProfitReduction = .3f;
    public float SpeedReduction = .3f;

    private GeneratorInfoCollection Generators
        => Services.GenerationService.Generators;
    
    public override void EventEnter()
    {

        foreach (var x in Services.GenerationService.Generators.Generators.Values) {
            //Services.GenerationService.Generators.SetEfficiencyMult(x.GeneratorId, 1.0 - ProfitReduction);
            //Services.GenerationService.Generators.SetSpeedMult(x.GeneratorId, 1.0 + SpeedReduction);
            Generators.AddProfitBoost(x.GeneratorId, BoostInfo.CreateTemp(GetType().FullName, 1.0 - ProfitReduction));
            Generators.AddTimeBoost(x.GeneratorId,
                BoostInfo.CreateTemp(GetType().FullName, 1.0f - Mathf.Clamp01(SpeedReduction)));
        }

        StartCoroutine(ShowImpl());
    }

    private System.Collections.IEnumerator ShowImpl() {
        yield return new WaitUntil(() => GameObject.Find("AutoCloseBg") == null );
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => Services.ViewService.ModalCount == 0);
        //EventView.Show(this);
        ViewService.Show(ViewType.BosEventView, new ViewData {
            UserData = new BosEventViewModel {
                EventType = this.eventType,
                Model = this
            }
        });
    }

    public override void EventExit()
    {

        foreach (var x in Services.GenerationService.Generators.Generators.Values)
        {
            //Services.GenerationService.Generators.SetEfficiencyMult(x.GeneratorId, 1.0 );
            Generators.RemoveProfitBoost(x.GeneratorId, GetType().FullName);
            Generators.RemoveTimeBoost(x.GeneratorId, GetType().FullName);
            //Services.GenerationService.Generators.SetSpeedMult(x.GeneratorId, 1.0 );
        }
    }

    public override void EventStay()
    {

    }
}
