namespace Bos.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MovingPhoneObjectSystem : GameBehaviour
    {
        public GameObject[] meteorPrefabs;
        public GameObject[] asteroidPrefabs;
        public MovePhoneData meteorData;
        public MovePhoneData asteroidsData;
        public float createPeriod;

        private float createTimer = 0f;
        private bool IsInitialized { get; set; }

        public void Setup()
        {
            IsInitialized = true;
        }


        public override void Update()
        {
            if(!IsInitialized )
            {
                return;
            }

            base.Update();
            createTimer += Time.deltaTime;
            if(createTimer >= createPeriod )
            {
                createTimer -= createPeriod;
                CreateObject(SelectObject());
            }
        }

        private void CreateObject(MovePhoneData data )
        {
            Vector2 position = new Vector2(data.startX.RangeValue(), data.startY.RangeValue());
            Vector2 moveDirection = data.moveDirection.normalized;
            float destroyX = data.destroyX;
            float speed = data.speedRange.RangeValue();
            float acceleration = data.accelerationRange.RangeValue();

            float scaleRatio = (speed - data.speedRange.minValue) / ( data.speedRange.maxValue - data.speedRange.minValue);
            float scaleValue = Mathf.Lerp(1.2f, 0.8f, scaleRatio);


            MovePhoneObjData createData = new MovePhoneObjData
            {
                Acceleration = acceleration,
                DestroyX = destroyX,
                Position = position,
                MoveDirection = moveDirection,
                Speed = speed,
                Scale = new Vector3(scaleValue, scaleValue, 1)
            };

            GameObject prefab = SelectPrefab(data);
            GameObject instance = Instantiate<GameObject>(prefab, new Vector3(40000, 40000), Quaternion.identity);
            RectTransform instanceTransform = instance.GetComponent<RectTransform>();
            instanceTransform.SetParent(transform, false);
            instance.GetComponent<MovingPhoneObject>().Setup(createData);
        }

        private GameObject SelectPrefab(MovePhoneData data )
        {
            switch(data.objectType)
            {
                case PhoneObjectType.Asteroid:
                    return asteroidPrefabs[UnityEngine.Random.Range(0, asteroidPrefabs.Length)];
                case PhoneObjectType.Meteor:
                    return meteorPrefabs[UnityEngine.Random.Range(0, meteorPrefabs.Length)];
                default:
                    throw new UnityException($"invalid object data");
            }
        }

        private MovePhoneData SelectObject()
        {
            int totalFrequence = meteorData.frequency + asteroidsData.frequency;
            float meteorProb = (float)meteorData.frequency / totalFrequence;
            if (UnityEngine.Random.value <= meteorProb)
            {
                return meteorData;
            }
            else
            {
                return asteroidsData;
            }
        }

    }


    public enum PhoneObjectType {
        Meteor,
        Asteroid
    }

    [Serializable]
    public class MovePhoneData
    {
        public PhoneObjectType objectType;
        public FloatRange startX;
        public FloatRange startY;
        public FloatRange speedRange;
        public FloatRange accelerationRange;
        public Vector2 moveDirection;
        public float destroyX;
        public int frequency;     
    }

    [Serializable]
    public class FloatRange
    {
        public float minValue;
        public float maxValue;
    }


    public static class MovingPhoneObjectSystemExtensions
    {
        public static float RangeValue(this FloatRange range)
            => UnityEngine.Random.Range(range.minValue, range.maxValue);
    }
}