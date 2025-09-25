using System;

namespace _Lofty.Hidden
{
    public enum DebuffType
    {
        Empty,
        Bleed,
        Burn,
        Stun,
        Provoke,
        Hypnosis,
        FireErupt,
    }

    public enum BuffType
    {
        Empty,
        Evade,
        CounterAttack,
        Resist,
        FullBelly,
        Awakening,
        Reflect,
    }


    [Serializable]
    public class BuffInfo
    {
        public int CurseIndex;
        public DebuffType DebuffType;
        public int CurseTurn;
        public CurseUI UI;
        public bool Activated;

        public BuffInfo(DebuffType _debuffType, int _curseTurn, CurseUI _ui)
        {
            DebuffType = _debuffType;
            CurseTurn = _curseTurn;
            UI = _ui;
        }
    }
}
