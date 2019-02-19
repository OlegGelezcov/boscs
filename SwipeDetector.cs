using System;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    private Vector2 fingerDownPosition;
    private Vector2 fingerUpPosition;

    [SerializeField]
    private bool detectSwipeOnlyAfterRelease = false;

    [SerializeField]
    private float minDistanceForSwipe = 20f;

    public static event Action<SwipeData> OnSwipe = delegate { };

    private void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                fingerUpPosition = touch.position;
                fingerDownPosition = touch.position;
            }

            if (!detectSwipeOnlyAfterRelease && touch.phase == TouchPhase.Moved)
            {
                fingerDownPosition = touch.position;
                DetectSwipe();
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                fingerDownPosition = touch.position;
                DetectSwipe(true);
            }
        }

#if UNITY_EDITOR
        if(Input.GetMouseButtonDown(0)) {
            fingerUpPosition = Input.mousePosition;
            fingerDownPosition = Input.mousePosition;
        }
        if(!detectSwipeOnlyAfterRelease && Input.GetMouseButton(0)) {
            fingerDownPosition = Input.mousePosition;
            DetectSwipe();
        }
        if(Input.GetMouseButtonUp(0)) {
            fingerDownPosition = Input.mousePosition;
            DetectSwipe(true);
        }
#endif
    }

    private void DetectSwipe(bool isEnd = false)
    {
        if (!isEnd) {
            if (SwipeDistanceCheckMet()) {
                if (IsVerticalSwipe()) {
                    var direction = fingerDownPosition.y - fingerUpPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                    SendSwipe(direction, isEnd);
                } else {
                    var direction = fingerDownPosition.x - fingerUpPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                    SendSwipe(direction, isEnd);
                }
                fingerUpPosition = fingerDownPosition;
            }
        } else {
            bool isDistCheck = SwipeDistanceCheckMet();

            if (IsVerticalSwipe()) {
                var direction = fingerDownPosition.y - fingerUpPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                if(!isDistCheck) {
                    SendSwipe(direction, false);
                }
                SendSwipe(direction, isEnd);
            } else {
                var direction = fingerDownPosition.x - fingerUpPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                if (!isDistCheck) {
                    SendSwipe(direction, false);
                }
                SendSwipe(direction, isEnd);
            }
            fingerUpPosition = fingerDownPosition;
        }
    }

    private bool IsVerticalSwipe()
    {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }

    private bool SwipeDistanceCheckMet()
    {
        return /*VerticalMovementDistance() > minDistanceForSwipe ||*/ HorizontalMovementDistance() > minDistanceForSwipe;
    }

    private float VerticalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.y - fingerUpPosition.y);
    }

    private float HorizontalMovementDistance()
    {
        return Mathf.Abs(fingerDownPosition.x - fingerUpPosition.x);
    }

    private void SendSwipe(SwipeDirection direction, bool isEnd)
    {
        SwipeData swipeData = new SwipeData()
        {
            Direction = direction,
            StartPosition = fingerDownPosition,
            EndPosition = fingerUpPosition,
            IsEnd = isEnd
        };
        OnSwipe(swipeData);
    }
}

public struct SwipeData
{
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public SwipeDirection Direction;
    public bool IsEnd;

    public float HorizontalLength
        => Mathf.Abs(EndPosition.x - StartPosition.x);

    public float VerticalLength
        => Mathf.Abs(EndPosition.y - StartPosition.y);

}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right
}