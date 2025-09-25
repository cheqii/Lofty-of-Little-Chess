using UnityEngine;

public enum AttackPattern
{
    MeleeAttack,
    RangedAttack,
}

[CreateAssetMenu(fileName = "AttackData", menuName = "Scriptable Objects/AttackData")]
public class AttackData : ScriptableObject
{
    public AttackPattern AttackPattern;
    public GameObject PatternPrefab;
}
