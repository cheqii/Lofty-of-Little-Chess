using UnityEngine;

namespace _Lofty.Hidden.Helpers
{
    public enum AttackType
    {
        NormalAttack, // default attack type
        FightingStyleAttack // player change attack type based on tarot card class collecting
    }

    public enum BehaviorState
    {
        Idle,
        Combat,
        OnAttack,
        Moving,
        Freeze
    }

    public enum PlayerMoveMode
    {
        FreeMove, // player can move freely without any restriction
        GridMove // player can only move in the grid base movement
    }
    public class PlayerHelpers : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
