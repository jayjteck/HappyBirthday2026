using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// 章节标题面板：顶部显示一章标题，停留几秒后慢慢淡出。
    /// 配合 ChapterTitleManager 使用（由管理器决定何时弹出）。
    /// </summary>
    public class ChapterTitlePanel : BasePanel
    {
        #region 公开字段

        public TextMeshProUGUI titleText;

        [Header("标题停留多久后开始淡出（秒）")]
        public float holdDuration = 3f;

        [Header("淡入淡出速度（数值越小越慢）")]
        public float fadeSpeed = 0.5f;

        #endregion

        protected override void AddEventListeners()
        {
        }

        protected override void RemoveEventListeners()
        {
        }

        protected override void Init()
        {
            // 章节标题用更慢的淡入淡出，呈现“慢慢出现、慢慢消失”的效果
            alphaSpeed = fadeSpeed;
        }

        #region 公开方法

        /// <summary>
        /// 显示章节标题：先淡入，停留 holdDuration 秒后淡出并销毁
        /// </summary>
        /// <param name="title">标题文字，如“第一章 · 初入职场，舞步生涩”</param>
        public void Show(string title)
        {
            titleText.text = title;

            // 淡入（ShowMe 会把 alpha 置 0，再逐帧加到 1）
            ShowMe();

            // 停止可能存在的旧计时协程，再重新计时淡出
            StopAllCoroutines();
            StartCoroutine(HideAfterDelay());
        }

        #endregion

        #region 私有方法

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(holdDuration);

            // 淡出（淡出完成后由 UIManager 销毁面板并从字典移除）
            UIManager.Instance.HidePanel<ChapterTitlePanel>();
        }

        #endregion
    }
}
