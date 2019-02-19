using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorsScreen : MonoBehaviour
{
	public static GeneratorsScreen Instance;
	public void Awake()
	{
		Instance = this;
	}

	//public SimpleGeneratorView[] generatorvievs;
}
