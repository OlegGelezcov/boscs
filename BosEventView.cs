namespace Bos.UI {
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

    public class BosEventView : TypedView
    {
		public EventView hackEventView;
		public EventView iceEventView;
		public EventView stockMarketEventView;
		public EventView nuclearEventView;
		public EventView huricaneEventView;

        public override ViewType Type => ViewType.BosEventView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

		public override int ViewDepth => 90;

		public override void Setup(ViewData data){
			var eventViewMap = ConstructEventViewMap();
			BosEventViewModel model = data.UserData as BosEventViewModel;
			if(model == null ) {
				throw new ArgumentException($"ViewModel is null {nameof(data.UserData)}");
			}
			ActivateView(eventViewMap, model);
		}

		private void ActivateView(Dictionary<BosEventViewType, EventView> viewMap, BosEventViewModel model) {
			foreach(var kvp in viewMap){
				if(kvp.Key == model.EventType) {
					kvp.Value.Show(model.Model);
				} else
				{
					kvp.Value.gameObject.Deactivate();
				}
			}
		}

		private Dictionary<BosEventViewType, EventView> ConstructEventViewMap()
			=> new Dictionary<BosEventViewType, EventView>{
				[BosEventViewType.Hack] = hackEventView,
				[BosEventViewType.Ice] = iceEventView,
				[BosEventViewType.StockMarket] = stockMarketEventView,
				[BosEventViewType.Nuclear] = nuclearEventView,
				[BosEventViewType.Huricane] = huricaneEventView
			};
    }

	public enum BosEventViewType {
		Hack,
		Ice,
		StockMarket,
		Nuclear,
		Huricane
	}

	public class BosEventViewModel
	{
		public BosEventViewType EventType {get; set;}
		public LocalEvent Model {get; set;}
	}
}
