using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class HintPanel : BasePanel
    {
        public TextMeshProUGUI  hintText;
        public Button closeButton;
    
        protected override void AddEventListeners()
        {
            closeButton.onClick.AddListener(()=>
            {
                UIManager.Instance.HidePanel<HintPanel>();
            });
        }

        protected override void RemoveEventListeners()
        {
            closeButton.onClick.RemoveAllListeners();
        }

        protected override void Init()
        {
        
        }

        #region 公开方法

        /// <summary>
        /// 设置提示面板内容
        /// </summary>
        /// <param name="text">提示文本</param>
        /// <param name="isShowCloseButton">是否显示关闭按钮（默认显示）</param>
        public void Set(string text, bool isShowCloseButton = true)
        {
            hintText.text = text;
        
            closeButton.gameObject.SetActive(isShowCloseButton);
        }

        #endregion
    }
}
