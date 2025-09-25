using System;
using _Lofty.Hidden.Scriptable;

public class TarotAction
{
    public TarotData TarotData { get; set; }
    public Action<bool> TarotCheck { get; }
    public Action ActionFunction { get; private set; }

    // private readonly Lazy<Tarot> tarot = new Lazy<Tarot>();

    public TarotAction(Action<bool> tarotCheck, Action _actionFunction)
    {
        TarotCheck = tarotCheck;
        ActionFunction = _actionFunction;
    }

    public TarotAction(TarotData _tarotData, Action<bool> _tarotCheck, Action _actionFunction)
    {
        TarotData = _tarotData;
        TarotCheck = _tarotCheck;
        ActionFunction = _actionFunction;
    }

    public void AddAction(Action _action)
    {
        ActionFunction = () =>
        {
            _action?.Invoke();
        };
    }
}
