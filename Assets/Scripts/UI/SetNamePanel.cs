using TMPro;
using UnityEngine.UI;
using EventCenter;
using Network;
using Photon.Pun;
using Player.Data;
using WebSocketSharp;

namespace UI
{
    public class SetNamePanel : BasePanel
    {
        #region 公开字段

        public TMP_InputField  nameInputField;
        public Button confirmButton;

        #endregion

        protected override void AddEventListeners()
        {
            nameInputField.onValueChanged.AddListener((strValue) =>
            {
                PlayerData.Instance.playerName = strValue;
            });

            confirmButton.onClick.AddListener(() =>
            {
                if (nameInputField.text.IsNullOrEmpty() || nameInputField.text.Length > 10)
                {
                    UIManager.Instance.ShowPanel<HintPanel>().Set("用户名不合法");
                    return;
                }
                
                // 多人模式才同步到 Photon 玩家自定义属性，别的玩家就能读到；单人模式跳过
                if (!PlayerData.Instance.isSinglePlayer)
                {
                    var props = new ExitGames.Client.Photon.Hashtable
                    {
                        { PlayerPropertyKey.PlayerName, nameInputField.text }
                    };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                }
                
                UIManager.Instance.ShowPanel<ChooseCharacterPanel>();
                UIManager.Instance.HidePanel<SetNamePanel>();
                UIManager.Instance.HidePanel<MainMenuPanel>();
            
                EventCenterMgr.Instance.EventTrigger(E_EventType.SetCanvasMode);
            });
        }

        protected override void RemoveEventListeners()
        {
            nameInputField.onValueChanged.RemoveAllListeners();
            confirmButton.onClick.RemoveAllListeners();
        }

        protected override void Init()
        {
        
        }
    }
}
