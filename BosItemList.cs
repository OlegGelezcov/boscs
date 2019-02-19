namespace Bos.UI {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class BosItemList<T, U>  where T : class where U : MonoBehaviour, IListItemView<T> {

        private readonly List<ListItem<T, U>> views = new List<ListItem<T, U>>();

        private GameObject viewPrefab;
        private Transform layout;
        private System.Func<T, T, bool> isEqualData = null;
        private System.Action<T, U> initFunc = null;
        private System.Func<T, T, int> compareFunc = null;
        private readonly List<ListItemChange<T>> changes = new List<ListItemChange<T>>();
        private ICoroutineContext context;

        public bool IsLoaded { get; private set; }

        public bool IsCorrectScale { get; set; } = false;
        public Vector3 CorrectedScaleValue { get; set; } = Vector3.one;

        public int Count
            => views.Count;

        public List<T> DataList
            => views.Select(v => v.Data).ToList();

        public void Setup(GameObject prefab, Transform layout,
            System.Action<T, U> initFunc = null,
            System.Func<T, T, bool> isEqualData = null,
            System.Func<T, T, int> compareFunc = null,
            ICoroutineContext context = null) {
            this.viewPrefab = prefab;
            this.layout = layout;
            this.initFunc = initFunc;
            this.isEqualData = isEqualData;
            this.compareFunc = compareFunc;
            this.context = context;
        }

        public bool Remove(U view) {
            var result = views.FirstOrDefault(v => isEqualData(v.Data, view.Data));
            if(result != null ) {
                views.Remove(result);
                if(result.View && result.View.gameObject) {
                    GameObject.Destroy(result.View.gameObject);
                    return true;
                }
            }
            return false;
        }


        public void Clear() {
            foreach (var view in views) {
                if (view.View && view.View.gameObject) {
                    GameObject.Destroy(view.View.gameObject);
                }
            }
            views.Clear();
        }

        public void Fill(List<T> dataList, float delayBetweenItems = 0.0f)
        {
            IsLoaded = false;
            if (dataList != null) {
                Clear();
                if (delayBetweenItems == 0.0f || context == null) {
                    foreach (T data in dataList) {
                        AddItem(data);
                        IsLoaded = true;
                    }
                } else {
                    context?.RunCoroutine(FillDelayedImpl(dataList, delayBetweenItems));
                }
            }
        }

        private IEnumerator FillDelayedImpl(List<T> dataList, float delay) {
            foreach(T data in dataList ) {
                AddItem(data);
                yield return new WaitForSeconds(delay);
            }
            IsLoaded = true;
        }

        

        

        public U FindView(T data) {
            foreach (var li in views) {
                if (isEqualData(data, li.Data)) {
                    return li.View;
                }
            }
            return default(U);
        }

        private bool ContainsData(List<T> targetList, T data) {
            foreach (var item in targetList) {
                if (isEqualData(item, data)) {
                    return true;
                }
            }
            return false;
        }




        public void UpdateViews(List<T> dataList) {
            changes.Clear();

            foreach (T data in dataList) {
                var view = FindView(data);
                if (view && view.gameObject) {
                    changes.Add(new ListItemChange<T> {
                        Action = ListAction.Update,
                        Data = data
                    });
                } else {
                    changes.Add(new ListItemChange<T> {
                        Action = ListAction.Add,
                        Data = data
                    });
                }
            }

            foreach (var li in views) {
                if (!ContainsData(dataList, li.Data)) {
                    changes.Add(new ListItemChange<T> {
                        Action = ListAction.Delete,
                        Data = li.Data
                    });
                }
            }

            HandleChanges(changes);
            if (changes.Count > 0 && compareFunc != null) {
                Order();
            }
        }

        private void AddItem(T data) {
            if (viewPrefab) {
                GameObject inst = GameObject.Instantiate<GameObject>(viewPrefab);
                inst.transform.SetParent(layout, false);
                if (IsCorrectScale) {
                    inst.transform.localScale = CorrectedScaleValue;
                }
                initFunc?.Invoke(data, inst.GetComponent<U>());
                views.Add(new ListItem<T, U> {
                    Data = data,
                    View = inst.GetComponent<U>()
                });
            }
        }

        private void UpdateItem(T data) {
            for (int i = 0; i < views.Count; i++) {
                if (isEqualData(views[i].Data, data)) {
                    views[i].Data = data;
                    initFunc?.Invoke(data, views[i].View);
                }
            }
        }

        private void DeleteItem(T data) {
            ListItem<T, U> targetItem = null;
            for (int i = 0; i < views.Count; i++) {
                if (isEqualData(data, views[i].Data)) {
                    targetItem = views[i];
                    break;
                }
            }
            if (targetItem != null) {
                if (targetItem.View && targetItem.View.gameObject) {
                    GameObject.Destroy(targetItem.View.gameObject);
                    views.Remove(targetItem);
                }
            }
        }

        private void HandleChanges(List<ListItemChange<T>> changes) {
            foreach (var change in changes) {
                switch (change.Action) {
                    case ListAction.Add: {
                            AddItem(change.Data);
                        }
                        break;
                    case ListAction.Update: {
                            UpdateItem(change.Data);
                        }
                        break;
                    case ListAction.Delete: {
                            DeleteItem(change.Data);
                        }
                        break;
                }
            }
        }

        private int Comparision(ListItem<T, U> item1, ListItem<T, U> item2) {
            if (item1.Data != null && item2.Data != null) {
                if (compareFunc != null) {
                    return compareFunc(item1.Data, item2.Data);
                }
            }
            return 0;
        }

        private void Order() {
            views.Sort(Comparision);
            for (int i = 0; i < views.Count; i++) {
                views[i].View.transform.SetSiblingIndex(i);
            }
        }
    }

    public interface IListItemView<T> where T : class {
        T Data { get; }
    }

    public class ListItem<T, U> where U : MonoBehaviour {
        public T Data { get; set; }
        public U View { get; set; }
    }

    public class ListItemChange<T> {
        public ListAction Action { get; set; }
        public T Data { get; set; }
    }

    public enum ListAction {
        Update,
        Add,
        Delete
    }

}