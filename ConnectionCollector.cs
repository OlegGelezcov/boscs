using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConnectionCollector : List<IDisposable>, IDisposable
{
	public IDisposable add { set { Add(value); } }
   

	public void DisconnectAll()
	{
		foreach(var c in this)
		{
			c.Dispose();
		}

		Clear();
	}

	public void RemoveAndDispose(IDisposable disp)
	{
		var removed = Remove(disp);
		if (!removed)
		{
			Debug.Log("that was not in collection");
		}
		disp.Dispose();
	}
	public void Dispose() { DisconnectAll(); }
	public void Collect(IDisposable connection)
	{
		add = connection;
	}
}
