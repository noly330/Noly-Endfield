using System.Collections;
using System.Collections.Generic;
using Endfield.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Endfield.Module.UI
{
    public class TopToolBarView : BaseView
    {
        private Button _bagBtn, _activityBtn, _missionBtn, 
        _teamBtn, _shopBtn, _operatorBtn, _gachaBtn;
        protected override UniTask OnInit(Transform root)
        {
            base.OnInit(root);

            _bagBtn = GetComponent<Button>("Main/Buttoms/OpenBag");
            _activityBtn = GetComponent<Button>("Main/Buttoms/OpenActivity");
            _missionBtn = GetComponent<Button>("Main/Buttoms/OpenMission");
            _teamBtn = GetComponent<Button>("Main/Buttoms/OpenTeam");
            _shopBtn = GetComponent<Button>("Main/Buttoms/OpenShop");
            _operatorBtn = GetComponent<Button>("Main/Buttoms/OpenOperator");
            _gachaBtn = GetComponent<Button>("Main/Buttoms/OpenGacha");

            _operatorBtn.onClick.AddListener(OpenOperatorDisplay);
            if (PlayerInputSystem.Instance != null)
                PlayerInputSystem.Instance.OpenOperatorDisplay.performed += OnOpenShortcut;
            return UniTask.CompletedTask;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (PlayerInputSystem.Instance != null)
                PlayerInputSystem.Instance.OpenOperatorDisplay.performed -= OnOpenShortcut;
        }

        /// <summary>打开干员展示界面：按钮与 C 键共用入口。</summary>
        private void OpenOperatorDisplay()
            => UIManager.Instance.OpenView(UIRegister.OperatorDisplayView, layer: UILayer.Top).Forget();
        private void OnOpenShortcut(InputAction.CallbackContext _) => OpenOperatorDisplay();
    }
}