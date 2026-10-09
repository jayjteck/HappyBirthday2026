using Network;
using Player.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuPanel : BasePanel
    {
        public Button singleplayerStartButton;
        public Button multiplayerStartButton;
        public Button exitButton;
    
        protected override void AddEventListeners()
        {
            singleplayerStartButton.onClick.AddListener(() =>
            {
                // 单人模式：不连服务器，直接走 设置名字 → 选角色 → 进入章节 流程
                PlayerData.Instance.isSinglePlayer = true;
                UIManager.Instance.ShowPanel<SetNamePanel>();
                UIManager.Instance.HidePanel<MainMenuPanel>();
            });
            
            multiplayerStartButton.onClick.AddListener(() =>
            {
                // 切回多人模式：清掉单人标记，走正常的 Photon 流程
                PlayerData.Instance.isSinglePlayer = false;
                NetworkManager.Instance.ConnectToServer();
            });
        
            exitButton.onClick.AddListener(Application.Quit);
        }

        protected override void RemoveEventListeners()
        {
            singleplayerStartButton.onClick.RemoveAllListeners();
            multiplayerStartButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();
        }

        protected override void Init()
        {
        
        }
    }
}
