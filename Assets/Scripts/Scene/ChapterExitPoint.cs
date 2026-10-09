using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using Player.Data;
using UI;
using UnityEngine;

namespace Scene
{
    /// <summary>
    /// 章节出口：放在每章场景末尾
    /// 任意一名玩家进入触发区并按空格，就向所有客户端弹出本章概述图 + 情节文字
    /// 停留 overviewDuration 秒后，由房主切换场景到下一章（AutomaticallySyncScene 会把全员一起带过去）
    /// </summary>
    public class ChapterExitPoint : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        #region 公开字段

        [Header("下一章场景名")]
        public string nextSceneName = "Chapter02";

        [Header("弹出展示的概述图（拖入 Sprite）")]
        public Sprite overviewImage;

        [Header("弹出展示的情节文字")]
        [TextArea(3, 8)]
        public string overviewText;

        [Header("展示多久后切场景（秒）")]
        public float overviewDuration = 5f;
        
        public AudioSource backgroundMusic;

        #endregion

        #region 私有字段

        // 本地玩家是否在触发区内
        private bool _localPlayerInside;

        // 是否已经启动了切换流程，防止连按空格重复触发
        private bool _isLoading;

        // 非房主 → 房主：请求开始“弹概述 + 切场景”流程
        private const byte RequestTransitionEventCode = 198;

        // 房主 → 其他客户端：现在弹出概述面板
        private const byte ShowOverviewEventCode = 199;

        #endregion

        #region Unity生命周期函数

        private void Update()
        {
            if (!_localPlayerInside || _isLoading)
                return;

            if (!Input.GetKeyDown(KeyCode.Space))
                return;

            RequestTransition();
        }

        #endregion

        #region 触发器

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 只关心本地玩家自己的角色（IsMine），别人的角色不算
            if (IsLocalPlayer(other))
                _localPlayerInside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsLocalPlayer(other))
                _localPlayerInside = false;
        }

        #endregion

        #region Photon 回调

        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case RequestTransitionEventCode:
                    // 房主收到“请求”后启动流程
                    if (PhotonNetwork.IsMasterClient)
                        StartTransition();
                    break;

                case ShowOverviewEventCode:
                    // 非房主客户端收到后弹出概述面板（房主自己会本地调用 ShowOverview）
                    ShowOverview();
                    break;
            }
        }

        #endregion

        #region 私有方法

        // 判断进入触发区的角色是不是本地玩家（单人模式只有一个角色，直接视为本地玩家）
        private bool IsLocalPlayer(Collider2D other)
        {
            if (PlayerData.Instance.isSinglePlayer)
                return true;

            var view = other.GetComponentInParent<PhotonView>();
            return view != null && view.IsMine;
        }

        /// <summary>
        /// 本地玩家按空格后调用：决定是本地直接开始，还是发请求给房主
        /// </summary>
        private void RequestTransition()
        {
            if (_isLoading)
                return;

            _isLoading = true;

            // 离线（没进房间）时本地直接开始，方便编辑器里单独测试
            if (!PhotonNetwork.InRoom)
            {
                ShowOverview();
                StartCoroutine(LoadNextSceneAfterDelay());
                return;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                StartTransition();
            }
            else
            {
                // 非房主：发事件请房主来启动流程
                var options = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                PhotonNetwork.RaiseEvent(RequestTransitionEventCode, nextSceneName, options, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
        }

        /// <summary>
        /// 房主启动流程：先弹概述面板给所有人，再计时切场景
        /// </summary>
        private void StartTransition()
        {
            // 房主自己先弹出（RaiseEvent 不会回传给发送者）
            ShowOverview();

            // 通知其他客户端也弹出
            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(ShowOverviewEventCode, null, options, ExitGames.Client.Photon.SendOptions.SendReliable);

            // 计时 overviewDuration 秒后切场景
            StartCoroutine(LoadNextSceneAfterDelay());
        }

        /// <summary>
        /// 弹出概述面板（概述图 + 情节文字）
        /// </summary>
        private void ShowOverview()
        {
            // 出现概述图时，关闭本场景的背景音乐
            if (backgroundMusic != null)
                backgroundMusic.Stop();
            
            UIManager.Instance.ShowPanel<ChapterOverviewPanel>().Show(overviewImage, overviewText);
        }
        
        /// <summary>
        /// 等 overviewDuration 秒后切换场景
        /// </summary>
        private IEnumerator LoadNextSceneAfterDelay()
        {
            yield return new WaitForSeconds(overviewDuration);
            
            if (PhotonNetwork.InRoom)
                PhotonNetwork.LoadLevel(nextSceneName);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }

        #endregion
    }
}
