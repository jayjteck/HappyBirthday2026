using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// 章节概述面板：切换章节前弹出，显示一章的概述图 + 情节文字。
    /// 由 ChapterExitPoint 触发显示；切到下一章后由 ChapterTitleManager 负责清除。
    /// </summary>
    public class ChapterOverviewPanel : BasePanel
    {
        #region 公开字段

        public Image overviewImage;
        public TextMeshProUGUI overviewText;

        #endregion

        protected override void AddEventListeners()
        {
        }

        protected override void RemoveEventListeners()
        {
        }

        protected override void Init()
        {
        }

        #region 公开方法

        /// <summary>
        /// 设置概述图与情节文字并淡入显示
        /// </summary>
        public void Show(Sprite img, string text)
        {
            overviewImage.sprite = img;
            overviewText.text = text;

            ShowMe();
        }

        #endregion
    }
}
