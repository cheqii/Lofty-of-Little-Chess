using UnityEngine;
using System;

[Serializable]
public class TurnData
{
    public Character unit;
    public float baseSpeed;
    public UnitTurnSlot unitSlot;

    public TurnData(Character _unit, float _turnSpeed, UnitTurnSlot _turnSlot)
    {
        unit = _unit;
        baseSpeed = _turnSpeed;
        unitSlot = _turnSlot;
    }
}
