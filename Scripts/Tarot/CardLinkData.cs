using System;
using System.Collections.Generic;
using _Lofty.Hidden.Scriptable;
using UnityEngine;

public class CardLinkData
{
    [Tooltip("Class Data Variables")]
    public string ClassName { get; }
    public List<TarotData> CardTypeData { get; }

    public Action<bool> PassiveOneSetter { get; }
    public Func<bool> PassiveOneGetter { get; }
    public Action<bool> PassiveTwoSetter { get; }
    public Func<bool> PassiveTwoGetter { get; }
    public Action<Color> TierOneColor { get; }
    public Action<Color> TierTwoColor { get; }

    [Tooltip("Class UI Variables")]
    public Sprite ClassProfile { get; }
    public Color ClassColor { get; }

    public string CrownAnimationName { get; }

    public CardLinkData(CardLinkDataConfig _config)
    {
        ClassName = _config.ClassName;
        CardTypeData = _config.CardTypeData;

        PassiveOneSetter = _config.PassiveOneSetter;
        PassiveOneGetter = _config.PassiveOneGetter;
        PassiveTwoSetter = _config.PassiveTwoSetter;
        PassiveTwoGetter = _config.PassiveTwoGetter;

        TierOneColor = _config.TierOneColor;
        TierTwoColor = _config.TierTwoColor;

        ClassProfile = _config.ClassProfile;
        ClassColor = _config.ClassColor;

        CrownAnimationName = _config.CrownAnimationName;
    }
}

public class CardLinkDataConfig
{
    [Tooltip("Class Data Variables")]
    public string ClassName;
    public List<TarotData> CardTypeData;

    public Action<bool> PassiveOneSetter;
    public Func<bool> PassiveOneGetter;

    public Action<bool> PassiveTwoSetter;
    public Func<bool> PassiveTwoGetter;

    public Action<Color> TierOneColor;
    public Action<Color> TierTwoColor;

    [Tooltip("Class UI Variables")]
    public Sprite ClassProfile;
    public Color ClassColor;
    public string CrownAnimationName;
}
