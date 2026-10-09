using Photon.Pun;
using Photon.Realtime;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Scene
{
    /// <summary>
    /// 视频触发：挂到场景里，任意一名玩家按触发键，就同步让所有客户端播放同一段视频
    /// 播放中按 Esc 会退出视频（同步给所有人），并且这一帧不会触发 MenuOpener 的菜单
    /// </summary>
    public class VideoPlayTrigger : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        #region 公开字段

        [Header("要播放的视频组件")]
        public VideoPlayer videoPlayer;

        [Header("显示视频画面的 RawImage（播放时显示、结束后隐藏）")]
        public RawImage videoImage;

        [Header("全屏黑底 Image（播放时盖住游戏场景，结束后隐藏）")]
        public Image videoBackground;

        [Header("触发按键")]
        public KeyCode triggerKey = KeyCode.P;

        #endregion

        #region 静态属性

        /// <summary>
        /// 当前是否有视频正在播放。供 MenuOpener 等脚本判断：播放中不响应 Esc 弹菜单。
        /// </summary>
        public static bool IsPlaying { get; private set; }

        #endregion

        #region 私有字段

        //任意客户端 → 其他客户端：开始播放视频（事件码必须在 1~199，200+ 是 Photon 系统保留）
        private const byte PlayVideoEventCode = 196;

        //任意客户端 → 其他客户端：退出视频
        private const byte StopVideoEventCode = 197;
        
        //停止播放视频待处理
        private bool _stopPending;

        #endregion

        #region Unity生命周期函数

        private void Start()
        {
            if (videoImage != null)
                videoImage.gameObject.SetActive(false);

            if (videoBackground != null)
                videoBackground.gameObject.SetActive(false);

            if (videoPlayer != null)
                videoPlayer.loopPointReached += OnVideoFinished;
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
                videoPlayer.loopPointReached -= OnVideoFinished;

            // 场景被销毁（比如切场景）时清掉播放状态，避免残留 true
            IsPlaying = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(triggerKey))
                RequestPlay();

            //播放中按 Esc：只标记“要停止”，IsPlaying 保持 true，让同帧里的 MenuOpener 不弹菜单
            if (IsPlaying && Input.GetKeyDown(KeyCode.Escape))
                _stopPending = true;
        }

        private void LateUpdate()
        {
            if (_stopPending)
            {
                _stopPending = false;
                
                //IsPlaying = false 延后到 LateUpdate中执行
                //保证同帧里 MenuOpener 读到的还是“正在播放”，从而不弹菜单
                RequestStop();
            }
        }

        #endregion

        #region Photon 回调

        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case PlayVideoEventCode:
                    Play();
                    break;

                case StopVideoEventCode:
                    StopVideo();
                    break;
            }
        }

        #endregion

        #region 私有方法

        private void RequestPlay()
        {
            Play();

            if (!PhotonNetwork.InRoom)
                return;

            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(PlayVideoEventCode, null, options, ExitGames.Client.Photon.SendOptions.SendReliable);
        }

        private void RequestStop()
        {
            StopVideo();

            if (!PhotonNetwork.InRoom)
                return;

            var options = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(StopVideoEventCode, null, options, ExitGames.Client.Photon.SendOptions.SendReliable);
        }

        private void Play()
        {
            if (videoPlayer == null)
                return;

            // 视频一播放，立即隐藏顶部的章节标题
            UIManager.Instance.HidePanel<ChapterTitlePanel>(false);

            if (videoImage != null)
                videoImage.gameObject.SetActive(true);

            if (videoBackground != null)
                videoBackground.gameObject.SetActive(true);

            IsPlaying = true;
            videoPlayer.Play();
        }

        /// <summary>
        /// 退出正在播放的视频
        /// </summary>
        private void StopVideo()
        {
            if (videoPlayer != null)
                videoPlayer.Stop();

            HideVideoUI();

            IsPlaying = false;
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            HideVideoUI();

            IsPlaying = false;
        }

        /// <summary>
        /// 隐藏视频画面和黑底
        /// </summary>
        private void HideVideoUI()
        {
            if (videoImage != null)
                videoImage.gameObject.SetActive(false);

            if (videoBackground != null)
                videoBackground.gameObject.SetActive(false);
        }

        #endregion
    }
}
