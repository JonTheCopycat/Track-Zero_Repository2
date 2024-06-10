using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarBehaviour;
using Replays;

public class ReplayCarControl : CarControl
{
    Replay currentReplay;

    public override void CustomStart()
    {
        carGetter.providedCar = currentReplay.GetCar();
    }
    

    
}
