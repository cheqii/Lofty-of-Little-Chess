using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Lofty.Hidden.Scriptable;
using _Lofty.Hidden.Helpers;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

public class CardInventory : MonoBehaviour
{
    #region -Declared Variables-

    [Tab("Normal")]
    // [SerializeField] private PlayerTarot tarotHandle;

    // public PlayerTarot TarotHandle { get => tarotHandle; set => tarotHandle = value; }

    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private bool isActive;

    [Header("Card Inventory")]
    [SerializeField] private int maxTarot;
    // [SerializeField] private List<Tarot> allTarotHaves;
    [SerializeField] private List<ArtifactUI> allTarotSlots;

    [Space]
    [SerializeField] private List<TarotData> normalType;

    [Tab("Sword Knight")]
    [SerializeField] private List<TarotData> swordKnightType;
    [SerializeField] private List<ArtifactUI> swordKnightSlots;

    [Tab("Blade Master")]
    [SerializeField] private List<TarotData> bladeMasterType;
    [SerializeField] private List<ArtifactUI> bladeMasterSlots;

    [Tab("Shooting Caster")]
    [SerializeField] private List<TarotData> shootCasterType;
    [SerializeField] private List<ArtifactUI> shootCasterSlots;

    #region -Public get / set variables-

    public int MaxTarot { get => maxTarot; set => maxTarot = value; }
    // public List<Tarot> AllTarotHaves { get => allTarotHaves; set => allTarotHaves = value; }
    public List<ArtifactUI> AllTarotSlots { get => allTarotSlots; set => allTarotSlots = value; }
    public List<TarotData> NormalType { get => normalType; set => normalType = value; }
    public List<TarotData> SwordKnightType { get => swordKnightType; set => swordKnightType = value; }
    public List<TarotData> BladeMasterType { get => bladeMasterType; set => bladeMasterType = value; }
    public List<TarotData> ShootCasterType { get => shootCasterType; set => shootCasterType = value; }
    public List<ArtifactUI> SwordKnightSlots { get => swordKnightSlots; set => swordKnightSlots = value; }
    public List<ArtifactUI> BladeMasterSlots { get => bladeMasterSlots; set => bladeMasterSlots = value; }
    public List<ArtifactUI> ShootCasterSlots { get => shootCasterSlots; set => shootCasterSlots = value; }

    #endregion

    private List<List<ArtifactUI>> allTarotUIs;

    #endregion

    private void Start()
    {
        MaxTarot = AllTarotSlots.Count;
    }

    public void InventoryAppear()
    {
        // var _playerState = tarotHandle.Player.PlayerMovementGrid;

        // if (_playerState.currentState == MovementState.Moving)
        //     return;

        // if (isActive)
        // {
        //     isActive = false;
        //     inventoryUI.SetActive(isActive);
        //     _playerState.currentState = MovementState.Idle;
        // }
        // else
        // {
        //     isActive = true;
        //     inventoryUI.SetActive(isActive);
        //     _playerState.currentState = MovementState.Freeze;
        // }
    }

    private void InitArtifactUI()
    {
        if (allTarotUIs != null) return;
        allTarotUIs = new List<List<ArtifactUI>>
        {
            AllTarotSlots,
            SwordKnightSlots,
            BladeMasterSlots,
            ShootCasterSlots
        };
    }

    private void ClearAllArtifactSlot()
    {
        InitArtifactUI();
        foreach (var _slot in allTarotUIs.SelectMany(_artifacts => _artifacts))
        {
            _slot.ClearArtifactSlot();
        }
    }

    public void SortingTarotType(TarotData _tarotData)
    {
        switch (_tarotData.Class)
        {
            case ClassType.Normal:
                NormalType.Add(_tarotData);
                break;
            case ClassType.SwordKnight:
                SwordKnightSlots[SwordKnightType.Count].SetArtifactUI(_tarotData.TarotName, _tarotData.TarotSprite, _tarotData.Description);
                SwordKnightType.Add(_tarotData);
                break;
            case ClassType.BladeMaster:
                BladeMasterSlots[BladeMasterType.Count].SetArtifactUI(_tarotData.TarotName, _tarotData.TarotSprite, _tarotData.Description);
                BladeMasterType.Add(_tarotData);
                break;
            case ClassType.ShootingCaster:
                ShootCasterSlots[ShootCasterType.Count].SetArtifactUI(_tarotData.TarotName, _tarotData.TarotSprite, _tarotData.Description);
                ShootCasterType.Add(_tarotData);
                break;
        }
    }

    public void SortingSlot()
    {
        ClearAllArtifactSlot();

        // foreach (var artifact in AllTarotHaves)
        // {
        //     AllTarotSlots[AllTarotHaves.IndexOf(artifact)]
        //         .SetArtifactUI(artifact.tarotData.artifactName, artifact.tarotData.artifactImage,artifact.tarotData.artifactDetail);
        // }

        foreach (var _tarot in SwordKnightType)
        {
            SwordKnightSlots[SwordKnightType.IndexOf(_tarot)]
                .SetArtifactUI(_tarot.TarotName, _tarot.TarotSprite, _tarot.Description);
        }
        foreach (var artifact in BladeMasterType)
        {
            BladeMasterSlots[BladeMasterType.IndexOf(artifact)]
                .SetArtifactUI(artifact.TarotName, artifact.TarotSprite, artifact.Description);
        }
        foreach (var artifact in ShootCasterType)
        {
            ShootCasterSlots[ShootCasterType.IndexOf(artifact)]
                .SetArtifactUI(artifact.TarotName, artifact.TarotSprite, artifact.Description);
        }
    }

    public void AddNewTarot(TarotData newTarot)
    {
        // var newCard = new Tarot(newTarot);
        // if (AllTarotHaves.Count == maxTarot)
        //     return;

        // if (AllTarotHaves.Contains(newCard))
        //     return;

        // AllTarotSlots[AllTarotHaves.Count].SetArtifactUI(newTarot.artifactName, newTarot.artifactImage,newTarot.artifactDetail);
        // AllTarotHaves.Add(newCard);
        // SortingTarotType(newTarot);
        // TarotHandle.TarotResult();
    }

    private void RemoveByType(TarotData _removeTarot)
    {
        switch (_removeTarot.Class)
        {
            case ClassType.Normal:
                NormalType.Remove(_removeTarot);
                break;
            case ClassType.SwordKnight:
                SwordKnightType.Remove(_removeTarot);
                break;
            case ClassType.BladeMaster:
                BladeMasterType.Remove(_removeTarot);
                break;
            case ClassType.ShootingCaster:
                ShootCasterType.Remove(_removeTarot);
                break;
        }
    }

    public void RemoveTarot(TarotData _removeTarot)
    {
        // var removeCard = allTarotHaves.Find(x=>x.tarotData == _removeTarot);
        // if (!allTarotHaves.Contains(removeCard))
        //     return;

        // allTarotSlots[allTarotHaves.IndexOf(removeCard)].ClearArtifactSlot();
        // allTarotHaves.Remove(removeCard);
        // RemoveByType(_removeTarot);
        // SortingSlot();
        // TarotHandle.TarotResult();
    }
}
