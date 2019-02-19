

namespace Bos.UI {
    using Bos.Debug;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    
    public abstract class BaseView : GameBehaviour {

        protected ViewData ViewData { get; private set; }

        public virtual void Setup(ViewData data) {
            ViewData = data;
        }

        public virtual void OnViewRemove() { }
        
        public virtual int ViewDepth { get; } = 0;
        private int OverrideViewDepth { get; set; } = int.MinValue;
        
        public void SetViewDepth(int value) {
            OverrideViewDepth = value;
        }

        public int SortDepth
            => (OverrideViewDepth == int.MinValue) ? ViewDepth : OverrideViewDepth;
        
        public abstract CanvasType CanvasType { get; }

        public abstract bool IsModal { get; }

        public virtual void AnimIn() {
            Animator animator = GetComponent<Animator>();
            if(animator != null ) {
                //Debug.Log("show trigger setted".Colored(ConsoleTextColor.white));
                animator.SetTrigger("show");
            }
            //GetComponent<Animator>()?.SetTrigger("show");
        }

        public virtual void AnimOut() {
            Animator animator = GetComponent<Animator>();
            if (animator != null) {
                animator.SetTrigger("hide");
                Debug.Log("hide trigger setted".Colored(ConsoleTextColor.white));
            }
            //GetComponent<Animator>()?.SetTrigger("hide");
        }
    }

    public abstract class TypedView : BaseView {

        public abstract ViewType Type { get; }

        public override void OnViewRemove() {
            base.OnViewRemove();

            if(ViewData != null ) {
                if(ViewData.ParentView != ViewType.None && ViewData.ParentViewData != null ) {
                    Services.ViewService.Show(ViewData.ParentView, ViewData.ParentViewData);
                }
            }
        }
    }

    public abstract class TypedViewWithCloseButton : TypedView
    {
        public Button closeButton;
    }

    public abstract class NamedView : BaseView {
        public abstract string Name { get; }
    }
}