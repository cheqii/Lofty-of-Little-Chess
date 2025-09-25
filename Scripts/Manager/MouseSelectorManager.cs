using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VInspector;

public class MouseSelectorManager : Singleton<MouseSelectorManager>, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Tab("Grid")]
    [SerializeField] private LayerMask layerToHit;

    [Header("Mouse Hover Variables")]
    [SerializeField] private GridMover selectedGrid;
    [SerializeField] private Character focusTarget;
    [SerializeField] private bool isInspecting;

    private Character tempFocusTarget;

    private GameController gameController => GameController.Instance;

    private ActionKey actionKey;


    // [Tab("Cursor Setup")]
    // public Texture2D defaultCursor;
    // public Texturn2D

    public override void Awake()
    {
        base.Awake();
        actionKey = new ActionKey();
    }

    private void OnEnable()
    {
        actionKey.Enable();
    }

    private void OnDisable()
    {
        actionKey.Disable();
    }

    private void Start()
    {
        actionKey.MouseAction.OnHover.performed += _ctx =>
        {
            // if (!GridManager.Instance.SuccessGenerate) return;
            OnMouseHover(_ctx);
        };
        // actionKey.MouseAction.OnClick.performed += ctx =>
        // {
        //     var _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;

        //     if (!Physics.Raycast(_ray, out hit, Mathf.Infinity, layerToHit)) return;
        //     if (hit.collider == null) return;
        //     var _gridMover = hit.collider.GetComponent<GridMover>();

        //     // if (_gridMover.CurrentState == GridState.OnEnemy && !_gridMover.TargetOnGrid.IsDead)
        //     // {
        //     //     // mean player is attacking the enemy then should stop the hover coroutine and count it again
        //     //     // HoverCoroutineHandle();
        //     // }

        //     // if (_gridMover.CurrentState is GridState.OnEnemy or GridState.OnObstacle) return;
        //     // var spawnTrans = new GameObject().transform;
        //     // spawnTrans.position = new Vector3(hit.transform.position.x, 0.25f, hit.transform.position.z);
        //     // VisualEffectManager.Instance.CallEffect(EffectName.Pop, spawnTrans,1.5f);
        //     // Destroy(spawnTrans.gameObject,0.5f);
        // };

        // actionKey.MouseAction.OnClick.performed += _ctx =>
        // {
        //     var _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;

        //     if (!Physics.Raycast(_ray, out hit, Mathf.Infinity, layerToHit)) return;
        //     if (hit.collider.gameObject.CompareTag("Draggable"))
        //     {
        //         print("Click on Draggable Object");
        //         return;
        //     }
        // };

        actionKey.MouseAction.OnInspector.performed += _ctx =>
        {
            if (selectedGrid == null || isInspecting || selectedGrid.CurrentState != GridState.OnEnemy) return;
            InspectorTarget();
        };
    }

    private void InspectorTarget()
    {
        isInspecting = true;

        if (selectedGrid.TargetOnGrid is Enemy && selectedGrid.TargetOnGrid == focusTarget)
        {
            tempFocusTarget = focusTarget;

            gameController.ClearEnemyGridInRoom();
            gameController.EnemyPatternVisible(focusTarget, selectedGrid);
        }
    }

    private void InspectorTargetCheck()
    {
        var _isSwapToAnotherTarget = focusTarget != null
        && selectedGrid.TargetOnGrid != tempFocusTarget
        && selectedGrid.TargetOnGrid != null;

        var _isUnInspect = focusTarget != null
        && selectedGrid.TargetOnGrid == null
        || isInspecting;

        // print($"turn data queue count {TurnBasedManager.Instance.TurnDataQueue.Count}");

        // only hovering the grid that have target on grid but not inspecting
        if (selectedGrid.TargetOnGrid != null && focusTarget != null && !isInspecting)
        {
            // tempFocusTarget = focusTarget;
            if (!focusTarget.IsDead && TurnBasedManager.Instance.TurnDataQueue.Count > 0 && GameController.Instance.CurrentRoom.RoomData.RoomType != RoomTypes.Standby)
            {
                TurnBasedManager.Instance.GetFocusUnitSlot(focusTarget).FocusSlot();
            }

            focusTarget.SelectableObject.SetSelected(true);
        }

        if (_isSwapToAnotherTarget)
        {
            // set the old focus target to default material
            tempFocusTarget.SelectableObject.SetSelected(false);

            TurnBasedManager.Instance.GetFocusUnitSlot(tempFocusTarget).UnFocusSlot();
            // set the focus target to new target on grid
            focusTarget = selectedGrid.TargetOnGrid;

            // set the new focus target to highlight material
            focusTarget.SelectableObject.SetSelected(true);

            if (!focusTarget.IsDead && TurnBasedManager.Instance.TurnDataQueue.Count > 0 && GameController.Instance.CurrentRoom.RoomData.RoomType != RoomTypes.Standby)
            {
                TurnBasedManager.Instance.GetFocusUnitSlot(focusTarget).FocusSlot();
            }

            // clear the old enemy grid pattern and show the new enemy grid pattern

            if (!isInspecting) return;
            gameController.ClearEnemyGridInRoom();
            gameController.EnemyPatternVisible(focusTarget, selectedGrid);

            // if mouse is swap target to the player while inspecting then clear the enemy grid pattern and show the ally pattern
            // and set denied the inspecting state

            if (selectedGrid.CurrentState == GridState.OnPlayer)
            {
                gameController.ClearEnemyGridInRoom();
                gameController.AllyPatternVisibilityCheck();
                isInspecting = false;
            }
        }
        else if (_isUnInspect)
        {
            // if change the selected grid that not have target on grid or change to another target on grid then return the old target material to default
            if (!focusTarget.IsDead && TurnBasedManager.Instance.TurnDataQueue.Count > 0 && GameController.Instance.CurrentRoom.RoomData.RoomType != RoomTypes.Standby)
            {
                TurnBasedManager.Instance.GetFocusUnitSlot(focusTarget).UnFocusSlot();
            }

            if (!focusTarget.OnTurn || focusTarget is Enemy && !focusTarget.OnTurn)
                focusTarget.SelectableObject.SetSelected(false);

            if (selectedGrid.TargetOnGrid == null && isInspecting || selectedGrid.CurrentState == GridState.OnPlayer)
            {
                gameController.ClearEnemyGridInRoom();
                gameController.AllyPatternVisibilityCheck();
            }

            focusTarget = null;
            tempFocusTarget = null;
            isInspecting = false;
        }
    }

    private void MouseHoverHandle(GridMover _targetHover)
    {
        // if the selected grid is not null and the target hover is not the same as the selected grid then set the selected grid on hover to false
        // if the selected grid is not the same as the target hover then the selected grid is the old target and set it to not on hover
        if (selectedGrid != null && _targetHover != selectedGrid)
        {
            // set the temp focus target to the selected grid target on grid if the selected grid target on grid is not null
            if (selectedGrid.TargetOnGrid != null && tempFocusTarget != null)
            {
                tempFocusTarget = selectedGrid.TargetOnGrid;
            }
            else if (tempFocusTarget == null && _targetHover.TargetOnGrid != null)
            {
                tempFocusTarget = _targetHover.TargetOnGrid;
            }

            selectedGrid!.OnHover = false;
        }

        if (selectedGrid == _targetHover) return;

        // set the new selected grid to the target hover and set on hover to true
        selectedGrid = _targetHover;
        selectedGrid!.OnHover = true;

        // set the focus target to the target on grid if the target on grid is not null
        if (selectedGrid.TargetOnGrid != null)
        {
            focusTarget = selectedGrid.TargetOnGrid;
        }

        // check if the mouse is hover the target and is inspecting or not
        InspectorTargetCheck();
    }

    private void OnMouseHover(InputAction.CallbackContext _ctx)
    {
        var _pos = _ctx.ReadValue<Vector2>();
        var _ray = Camera.main.ScreenPointToRay(_pos);
        RaycastHit hit;

        if (Physics.Raycast(_ray, out hit, Mathf.Infinity, layerToHit))
        {
            if (hit.collider != null)
            {
                var _gridMover = hit.collider.GetComponent<GridMover>();

                MouseHoverHandle(_gridMover);
            }
        }
        else
        {
            if (selectedGrid == null) return;

            if (focusTarget != null)
            {
                focusTarget?.SelectableObject.SetSelected(false);
                TurnBasedManager.Instance.GetFocusUnitSlot(focusTarget)?.UnFocusSlot();
            }

            selectedGrid!.OnHover = false;

            selectedGrid = null;
            focusTarget = null;

            isInspecting = false;

            gameController.AllyPatternVisibilityCheck();
        }

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
        // var _draggableObject = eventData.pointerClick.gameObject;
        // _draggableObject.transform.SetParent(_draggableObject.transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // throw new System.NotImplementedException();
    }
}
