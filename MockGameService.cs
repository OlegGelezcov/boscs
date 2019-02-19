using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
//using System.Threading.Tasks;
using UnityEngine;
using UnityHTTP;

public class MockGameService : IGameService
{

    private DateTime serverDateTime;
    public DateTime GetServerDateTime()
    {
       return DateTime.Now;
    }

    private IEnumerator CheckOnline()
    {
        var www  = new WWW("");
        yield return www;

        if (www.error == null)
        {
            yield break;
        }

        var time = www.text;
        var format = "yyyy-MM-dd-HH-mm-ss";
        serverDateTime = DateTime.ParseExact(time, format, CultureInfo.InvariantCulture);
    }
}
