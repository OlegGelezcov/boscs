namespace Bos.Services {
    using System.Collections.Generic;

    public class PromoServiceSave  {

        public List<string> codes;

        public void Validate() {
            if(codes == null ) {
                codes = new List<string>();
            }
        }
    }
}
