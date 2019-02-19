namespace Bos.Services {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IPromoService : IGameService {

        //bool IsValid(string code);
        void RequestPromo(string code);
        bool IsAllowPromo();
    }

}