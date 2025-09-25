using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Lofty.Hidden.Manager
{
    public class ActionKeyManager : Singleton<ActionKeyManager>
    {
        private ActionKey actionKey;
        public ActionKey ActionKey { get => actionKey; set => actionKey = value; }

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
            actionKey.CameraAction.ChangeCameraView.performed +=
                _ctx => CameraManager.Instance.ChangeCameraView();

            // actionKey.InteractAction.Interact.performed += 
        }
    }
}
