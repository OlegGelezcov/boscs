using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstService : MonoBehaviour, IFirstService {

    public void Construct() {
        print("first service construct");
    }

    public void FirstMethod()
        => print("first method");
}

public interface IFirstService {
    void FirstMethod();
}