using System;
using UnityEngine.UI;

namespace Bos.UI {
    public class FirstInvestorView : TypedView {

        public Text descriptionText;


        public override ViewType Type => ViewType.FirstInvestorView;

        public override CanvasType CanvasType => CanvasType.UI;

        public override bool IsModal => true;

        public override int ViewDepth => 50;

        public override void Setup(ViewData data) {
            base.Setup(data);

            double securitiesCount = Services.InvestorService.GetSecuritiesCountFromInvestors();
            double div1000 = Math.Floor(securitiesCount / 1000);
            if(div1000 == 0 ) { div1000 = 1; }
            int roundedCount = (int)(div1000 * 1000);
            descriptionText.text = LocalizationObj.GetString("lbl_first_investor_desc").Format(roundedCount);
        }
    }

}