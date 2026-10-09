using Network;
using Photon.Pun;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene
{
    /// <summary>
    /// 章节标题管理器：挂在每个 Chapter 场景里。
    /// 等房间内所有玩家都加载完本场景后，才在顶部弹出章节标题（停留几秒后淡出）。
    /// </summary>
    public class ChapterTitleManager : MonoBehaviourPunCallbacks
    {
        #region 公开字段

        [Header("本场景标题（每个章节场景在 Inspector 里填各自的标题）")]
        public string chapterTitle = "第一章 · 初入职场，舞步生涩";

        #endregion

        #region 私有字段

        // 是否已经弹出过标题，防止重复弹
        private bool _shown;

        #endregion

        #region Unity生命周期函数

        private void Start()
        {
            // 清除上一章切换时残留的概述面板（概述面板挂在 DontDestroyOnLoad 的 Canvas 下，不会随场景销毁）
            UIManager.Instance.HidePanel<ChapterOverviewPanel>(false);

            // 没进房间时（比如编辑器里单独运行测试）也直接弹，方便调试
            if (!PhotonNetwork.InRoom)
            {
                ShowTitle();
                return;
            }

            string currentScene = SceneManager.GetActiveScene().name;

            // 标记本客户端“已加载到当前章节场景”。用场景名当值，
            // 切到下一章时旧值自然不等于新场景名，无需手动重置。
            var props = new ExitGames.Client.Photon.Hashtable
            {
                { PlayerPropertyKey.LoadedChapter, currentScene }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            // 自己可能是最后一个加载完的，先检查一次
            CheckAllLoadedAndShow();
        }

        #endregion

        #region Photon 回调

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            // 有人更新了“已加载章节”属性，就重新判断是否全员到齐
            if (changedProps.ContainsKey(PlayerPropertyKey.LoadedChapter))
                CheckAllLoadedAndShow();
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            // 有人中途退出时也重新判断一次，避免一直等一个已离线的玩家
            CheckAllLoadedAndShow();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 房间内所有玩家都加载到当前场景后，弹出章节标题
        /// </summary>
        private void CheckAllLoadedAndShow()
        {
            if (_shown || PhotonNetwork.CurrentRoom == null)
                return;

            string currentScene = SceneManager.GetActiveScene().name;

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                var props = player.CustomProperties;
                bool loadedCurrent = props.ContainsKey(PlayerPropertyKey.LoadedChapter) 
                                     && (props[PlayerPropertyKey.LoadedChapter] as string) == currentScene;

                // 还有任意一个玩家没加载到当前场景，就再等等
                if (!loadedCurrent)
                    return;
            }

            ShowTitle();
        }

        private void ShowTitle()
        {
            if (_shown)
                return;

            _shown = true;

            UIManager.Instance.ShowPanel<ChapterTitlePanel>().Show(chapterTitle);
        }

        #endregion
    }
}
