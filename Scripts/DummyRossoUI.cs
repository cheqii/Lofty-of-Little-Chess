using System;
using TMPro;
using UnityEngine;

public class DummyRossoUI : Singleton<DummyRossoUI>
{
    public TextMeshProUGUI MaxActionPointText;
    public TextMeshProUGUI CurrentActionPointText;

    public void SetupActionPointUI(int maxActionPoints)
    {
        MaxActionPointText.text = $"{maxActionPoints}";
        CurrentActionPointText.text = $"{maxActionPoints}";
    }

    public void UpdateCurrentActionPoint(int currentActionPoints)
    {
        CurrentActionPointText.text = $"{currentActionPoints}";
    }
}
