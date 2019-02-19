using Bos.Ioc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerTest : MonoBehaviour {

    /*
    IocContainer container;

    private void Awake() {
        container = new IocContainer();
        container.RegisterInstance<IFirstService, FirstService>(FindObjectOfType<FirstService>(), LifeCycle.MonoBehaviour);
        container.RegisterInstance<ISecondService, SecondService>(FindObjectOfType<SecondService>(), LifeCycle.MonoBehaviour);
        container.Register<SomeClass, SomeClass>(LifeCycle.Singleton);

        container.Build();
    }

    private void OnGUI() {
        GUILayout.BeginVertical();
        if(GUILayout.Button("resolve")) {
            container.Resolve<ISecondService>().TestInjectedField();
        }
        GUILayout.EndVertical();
    }*/
}
