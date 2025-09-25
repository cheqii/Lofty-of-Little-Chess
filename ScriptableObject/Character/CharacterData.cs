using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/Character", order = 1)]
public class CharacterData : ScriptableObject
{
    [Header("Data")]
    public string Name;

    [ShowAssetPreview(128, 128)]
    public Sprite Sprite;

    [Header("Pattern")]
    public PatternType MovePattern;
    public AttackPattern AttackPattern;

    [Header("Stats")]
    public int MaxHealth;
    [Space]
    public int MaxActionPoint;
    [Space(10)]
    public int Damage;
    [Space]
    public float MoveSpeed;
    [Range(0,100)]
    public float TurnSpeed;
}
