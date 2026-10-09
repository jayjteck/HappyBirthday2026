using EventCenter;
using Network;
using Player.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class ChooseCharacterPanel : BasePanel
    {
        #region 公开字段

        public Button characterSelectBtn0;
        public Button characterSelectBtn1;
        public Button characterSelectBtn2;

        #endregion

        #region 私有字段

        private GameObject _characterModel;

        #endregion

        protected override void AddEventListeners()
        {
            characterSelectBtn0.onClick.AddListener(() => SelectCharacter(0));
            characterSelectBtn1.onClick.AddListener(() => SelectCharacter(1));
            characterSelectBtn2.onClick.AddListener(() => SelectCharacter(2));

            EventCenterMgr.Instance.AddEventListener(E_EventType.SetCanvasMode, SetCanvasModeToScreenSpaceCamera);
            
            EventCenterMgr.Instance.AddEventListener(E_EventType.CreateRoomFailed,()=>
            {
                UIManager.Instance.ShowPanel<HintPanel>().Set("创建房间失败", true);
            });
        }

        protected override void RemoveEventListeners()
        {
            characterSelectBtn0.onClick.RemoveAllListeners();
            characterSelectBtn1.onClick.RemoveAllListeners();
            characterSelectBtn2.onClick.RemoveAllListeners();
            
            EventCenterMgr.Instance.RemoveEventListener(E_EventType.SetCanvasMode, SetCanvasModeToScreenSpaceCamera);

            EventCenterMgr.Instance.Clear(E_EventType.CreateRoomFailed);
        }

        protected override void Init()
        {
            _characterModel = Instantiate(Resources.Load<GameObject>("Model/CharacterModels"));
        }

        #region Unity生命周期函数

        private void OnDestroy()
        {
            Destroy(_characterModel);
            _characterModel = null;
        }

        #endregion

        #region 私有方法

        private void SelectCharacter(int index)
        {
            PlayerData.Instance.characterIndex = index;

            // 单人模式：不进房间，直接加载 Chapter01 场景
            if (PlayerData.Instance.isSinglePlayer)
            {
                // SetNamePanel 切到 ScreenSpaceCamera 是为了在选角色面板显示 3D 模型；
                // 进游戏场景前切回 Overlay，否则切场景后相机销毁会导致 UI 不显示
                UIManager.Instance.MainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                UIManager.Instance.HidePanel<ChooseCharacterPanel>();
                SceneManager.LoadScene("Chapter01");
                return;
            }

            NetworkManager.Instance.JoinOrCreateRoom();
        }

        //将 Canvas的模式设置成 Camera模式
        private void SetCanvasModeToScreenSpaceCamera()
        {
            UIManager.Instance.MainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            UIManager.Instance.MainCanvas.worldCamera = Camera.main;
        }

        #endregion
    }
}
