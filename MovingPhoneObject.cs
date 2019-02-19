namespace Bos.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MovingPhoneObject : GameBehaviour
    {
        private MovePhoneObjData Data { get; set; }

        private bool IsInitialized { get; set; }

        private RectTransform selfTransform;
        private float currentSpeed;


        public void Setup(MovePhoneObjData data )
        {
            Data = data;
            selfTransform = GetComponent<RectTransform>();
            selfTransform.anchoredPosition = Data.Position;
            selfTransform.localScale = Data.Scale;
            currentSpeed = Data.Speed;

            IsInitialized = true;
        }

        public override void Update()
        {
            base.Update();
            if(!IsInitialized)
            {
                return;
            }

            currentSpeed += Data.Acceleration * Time.deltaTime;
            selfTransform.anchoredPosition += Data.MoveDirection * currentSpeed * Time.deltaTime;


            CheckLimits();
        }

        private void CheckLimits()
        {
            if(Data.MoveDirection.x < 0 )
            {
                if(selfTransform.anchoredPosition.x < -Mathf.Abs(Data.DestroyX))
                {
                    Destroy(gameObject);
                }
            } else
            {
                if(selfTransform.anchoredPosition.x > Mathf.Abs(Data.DestroyX))
                {
                    Destroy(gameObject);
                }
            }
        }
    }


    public class MovePhoneObjData
    {
        public Vector2 Position { get; set; }
        public Vector2 MoveDirection { get; set; }
        public float DestroyX { get; set; }
        public float Speed { get; set; }
        public float Acceleration { get; set; }
        public Vector3 Scale { get; set; }
    }
}