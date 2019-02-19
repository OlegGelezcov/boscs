namespace Bos.Data {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    /// <summary>
    /// Implemented by differences in version data classes
    /// </summary>
    public interface IDataDifference {

        /// <summary>
        /// Is no difference in data objects
        /// </summary>
        bool IsSame { get; }

        /// <summary>
        /// Collection of changed properties and changes, key - changed property, value - difference in property
        /// </summary>
        Dictionary<string, object> Difference { get; }
    }

}