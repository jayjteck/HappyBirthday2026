using Network;
using UnityEngine.UI;

namespace UI
{
    public class MenuPanel : BasePanel
    {
        #region 公开字段

        public Button exitButton;
        public Button closeButton;

        #endregion
    
        protected override void AddEventListeners()
        {
            exitButton.onClick.AddListener(() =>
            {
                NetworkManager.Instance.LeaveRoom();
            });
        
            closeButton.onClick.AddListener(() =>
            {
                UIManager.Instance.HidePanel<MenuPanel>();
            });
        }

        protected override void RemoveEventListeners()
        {
            exitButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();
        }

        protected override void Init()
        {
        }
    }
}
