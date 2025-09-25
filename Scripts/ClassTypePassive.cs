using System;
using _Lofty.Hidden.Helpers;

[Serializable]
public class ClassTypePassive
{
    public ClassType ClassType { get; }

    public Func<bool> PassiveOneGetter { get; }
    public Func<bool> PassiveTwoGetter { get; }

    public ClassTypePassive(ClassTypePassiveConfig _config)
    {
        ClassType = _config.ClassType;
        PassiveOneGetter = _config.PassiveOneGetter;
        PassiveTwoGetter = _config.PassiveTwoGetter;
    }
}

public class ClassTypePassiveConfig
{
    public ClassType ClassType;

    public Func<bool> PassiveOneGetter;
    public Func<bool> PassiveTwoGetter;
}
