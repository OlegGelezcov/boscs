namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Bos;
    using Bos.Debug;

    public class CanvasOptimizer : GameBehaviour {

        private readonly Dictionary<string, ViewCachedData> optimizedViews 
            = new Dictionary<string, ViewCachedData>();

        public void Push(Transform targetTransform) {
            if(!Contains(targetTransform)) {
                if (targetTransform.parent != transform) {
                    ViewCachedData viewCachedData = new ViewCachedData(targetTransform);
                    optimizedViews.Add(viewCachedData.Key, viewCachedData);
                    viewCachedData.AddToOptimizer(transform);
                    Services.GetService<IConsoleService>().AddOnScreenText("generators hided");
                }
            }
        }

        public void Pop(string key) {
            if(Contains(key)) {
                ViewCachedData viewCachedData = optimizedViews[key];
                optimizedViews.Remove(key);
                viewCachedData.RemoveFromOptimizer(transform);
                Services.GetService<IConsoleService>().AddOnScreenText("generators showed");
            }
        }

        public void Pop(Transform transform)
            => Pop(transform.name);


        public bool Contains(Transform transform)
            => Contains(transform.name);

        public bool Contains(string key)
            => optimizedViews.ContainsKey(key);
    }

    public class ViewCachedData {
        public string Key { get; }
        public Transform TargetTransform { get;  }
        public Transform SourceParentTransform { get;  }

        public ViewCachedData(Transform targetTransform)
            : this(targetTransform.name, targetTransform, (RectTransform)targetTransform.parent) { }

        public ViewCachedData( string key, Transform targetTransform, Transform sourceParentTransform) {
            this.Key = key;
            this.TargetTransform = targetTransform;
            this.SourceParentTransform = sourceParentTransform;
        }

        public void AddToOptimizer(Transform optimizerTransform ) {
            TargetTransform?.SetParent(optimizerTransform, true);
        }

        public void RemoveFromOptimizer(Transform optimizerTransform ) {
            if(SourceParentTransform != null && TargetTransform != null ) {
                TargetTransform.SetParent(SourceParentTransform, true);
            }
        }
    }
}