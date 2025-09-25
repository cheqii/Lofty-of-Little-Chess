using System;
using _Lofty.Hidden.Interface;
using NUnit.Framework;
using UnityEngine;
using DG.Tweening;

public class Gate : MonoBehaviour, IInteractable
{
    [Header("Gate Settings")]
    [Space]
    [SerializeField] private bool isUnlocked;
    [SerializeField] private Room ownRoom;
    [SerializeField] private Room connectRoom;

    public bool IsUnlocked { get => isUnlocked; set { isUnlocked = value; } }
    public Room OwnRoom { get => ownRoom; set => ownRoom = value; }
    public Room ConnectRoom { get => connectRoom; set => connectRoom = value; }

    [Space]
    public Material lockedGate;
    public Material openedGate;

    [Space]
    [SerializeField] private GameObject popupUI;
    [SerializeField] private bool isActive;

    public GameObject PopupUI { get => popupUI; set => popupUI = value; }
    public bool IsActive { get => isActive; set => isActive = value; }
    public NewPlayer Player { get; set; }

    [Space]
    [Header("Interactable Components")]
    [SerializeField] private Interactable interactable;

    /// <summary>
    /// Setup the gate room with the own room and connect room.
    /// This method will set the interactable object to this gate,
    /// and set the own room and connect room for the gate.
    /// It will also set the isUnlocked property based on the room type.
    /// </summary>
    /// <param name="_ownRoom"></param>
    /// <param name="_connectRoom"></param>
    public void SetupGateRoom(Room _ownRoom, Room _connectRoom)
    {
        interactable.InteractableObject = this;

        OwnRoom = _ownRoom;
        ConnectRoom = _connectRoom;

        if (_ownRoom.RoomData.RoomType is RoomTypes.Combat or RoomTypes.Boss)
        {
            IsUnlocked = false;
        }
        else
        {
            IsUnlocked = true;
        }
    }

    /// <summary>
    /// The order that triggers when the player enters gate to go to the connect room.
    /// </summary>
    private void ConnectRoomTrigger()
    {
        CameraManager.Instance.SetCameraTarget(ConnectRoom.transform);
        GameController.Instance.UpdateRoom(ConnectRoom);

        Player.transform.SetParent(ConnectRoom.transform);

        OwnRoom.gameObject.SetActive(false);
        ConnectRoom.gameObject.SetActive(true);

        Player.gameObject.SetActive(false);
        if (!ConnectRoom.IsVisited) return;
    }

    /// <summary>
    /// Set the position of the player based on the door in the connect room that link to the old room that player came from.
    /// </summary>
    private void TeleportPlayerToGate()
    {
        var _oppositeDoor = ConnectRoom.GateInRoom.Find(gate => gate.ConnectRoom == OwnRoom);

        if (_oppositeDoor == null)
        {
            return;
        }

        var _doorPosition = new Vector3(
            _oppositeDoor.transform.localPosition.x,
            Player.transform.localPosition.y,
            _oppositeDoor.transform.localPosition.z);

        Player.transform.SetLocalPositionAndRotation(new Vector3(
            _oppositeDoor.transform.localPosition.x,
            6f,
            _oppositeDoor.transform.localPosition.z),
            Quaternion.identity);

        Player.transform.DOLocalMoveY(_doorPosition.y, 0.75f).SetEase(Ease.OutElastic);

        Player.gameObject.SetActive(true);
    }

    /// <summary>
    /// Interact method to handle the interaction with the gate in the room that visited.
    /// This method will check if the gate is active and unlocked, then it will set the
    /// camera target to the connect room and update the current room in the game controller.
    /// It will also set the player to the connect room and disable the own room.
    /// It will also set the player position to the opposite door position in the connect room.
    /// If the connect room is not visited, it will not do anything.
    /// </summary>
    public void Interact()
    {
        if (!IsUnlocked) return;

        ConnectRoomTrigger();

        TeleportPlayerToGate();
    }

    public void TriggerEnter(NewPlayer _player)
    {
        if (_player == null) return;

        IsActive = isUnlocked;
        Player = _player;
        PopupUI.SetActive(IsActive);
        // Additional logic for when the player enters the interactable area
    }

    public void TriggerExit()
    {
        IsActive = false;
        Player = null;
        PopupUI.SetActive(IsActive);
    }
}
