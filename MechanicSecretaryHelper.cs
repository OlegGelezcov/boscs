namespace Bos {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Common methods for mechanic and secretaries
    /// </summary>
    public static class MechanicSecretaryHelper {

        public static double GetUnitPriceForMechanicSecretaryPrice(GeneratorInfo generator, int unitCount) {
            IBosServiceCollection Services = GameServices.Instance;
            int ownedCount = Services.TransportService.GetUnitTotalCount(generator.GeneratorId) - unitCount;
            if (ownedCount <= 0) { ownedCount = 1; }
            return Services.GenerationService.CalculatePrice(unitCount, ownedCount, generator);
        }
    }

}