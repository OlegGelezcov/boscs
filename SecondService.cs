using Bos.Ioc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class SecondService : MonoBehaviour, ISecondService {

    private IFirstService firstService;

    [Inject]
    private IFirstService firstServiceInjected;

    [Inject]
    private SomeClass someClass;

    public void Construct(IFirstService fs) {
        firstService = fs;
        print("second service");
    }

    public void Second()
        => print("second");

    public void TestInjectedField() {
        print("test injected field");
        firstServiceInjected.FirstMethod();
        someClass.SomeClassTest();
    }
}

public interface ISecondService {
    void Second();
    void TestInjectedField();
}


public class SomeClass {
    public void SomeClassTest() {
        Debug.Log("hi!");
    }
}*/