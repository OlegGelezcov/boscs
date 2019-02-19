using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    public float Speed = 1;
    public float Amplitude = 2;
    public int Octaves = 4;

    public bool UseX = true;
    public bool UseY = true;
    public bool UseZ = true;

    private Vector3 _destination;
    private Vector3 _vel;
    private int _currentTime = 0;

    void Update()
    {
        if (_currentTime > Octaves)
        {
            _currentTime = 0;
            _destination = GenerateRandomVector(Amplitude);
        }

        var a = Vector3.SmoothDamp(transform.position, _destination, ref _vel, Speed);
        if (UseX && UseY && UseZ)
        {
            transform.position = a;
        }
        else
        {
            Vector3 target = transform.position;

            if (UseX)
                target.x = a.x;

            if (UseY)
                target.y = a.y;

            if (UseZ)
                target.z = a.z;

            transform.position = target;
        }

        _currentTime++;
    }

    private Vector3 GenerateRandomVector(float amp)
    {
        Vector3 result = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            float x = Random.Range(-amp, amp);
            result[i] = x;
        }
        return result;
    }
}
