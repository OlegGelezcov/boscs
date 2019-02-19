namespace Bos {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GameElement  {

        protected IBosServiceCollection Services {
            get {
                return GameServices.Instance;
            }
        }

        protected IGenerationService GenerationSrv
            => Services.GenerationService;
    }

}