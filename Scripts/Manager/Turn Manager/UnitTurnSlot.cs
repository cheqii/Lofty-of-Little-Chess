using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UnitTurnSlot : MonoBehaviour
{
    [SerializeField] private Character unitOwner;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image slotBorder;
    public Character UnitOwner { get => unitOwner; set => unitOwner = value; }
    public Image IconImage { get => iconImage; set => iconImage = value; }

    public void SetUnitSlot(Character _unit, Sprite _sprite)
    {
        UnitOwner = _unit;
        IconImage.sprite = _sprite;

        slotBorder.color = _unit is NewPlayer ? Color.green : Color.red;
    }

    public void ClearUnitSlot()
    {
        // print($"clear unit slot {unitOwner.gameObject.name}");
        // pool object inactive
        Destroy(gameObject);
    }

    public void FocusSlot()
    {
        IconImage.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        slotBorder.color = Color.yellow;
    }

    public void UnFocusSlot()
    {
        IconImage.transform.DOKill();
        IconImage.transform.localScale = Vector3.one;
        slotBorder.color = unitOwner is NewPlayer ? Color.green : Color.red;
    }
}
