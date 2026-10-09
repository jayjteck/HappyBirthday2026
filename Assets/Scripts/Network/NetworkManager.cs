using System;
using EventCenter;
using Photon.Pun;
using Photon.Realtime;
using Player.Data;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        #region 单例模式

        private static NetworkManager _instance;

        public static NetworkManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    //在场景上创建空物体
                    GameObject obj = new GameObject
                    {
                        //得到脚本的类名，为对象改名，这样在编辑器中可以明确的看到该单例模式脚本对象依附的 GameObject
                        name = typeof(NetworkManager).ToString()
                    };

                    //动态挂载对应的单例模式脚本
                    _instance = obj.AddComponent<NetworkManager>();

                    //过场景时不移除对象，保证它在整个游戏生命周期中都存在
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            //监听倒计时结束事件：数到 0 后由房主清空房间属性，方便以后重新倒数
            EventCenterMgr.Instance.AddEventListener(E_EventType.CountDownFinish, OnCountDownFinish);
        }

        private void OnDestroy()
        {
            EventCenterMgr.Instance.RemoveEventListener(E_EventType.CountDownFinish, OnCountDownFinish);
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 连接 Photon服务器
        /// </summary>
        public void ConnectToServer()
        {
            PhotonNetwork.GameVersion = "1.0";
            PhotonNetwork.AutomaticallySyncScene = true;//开启场景同步
            PhotonNetwork.ConnectUsingSettings();

            UIManager.Instance.ShowPanel<HintPanel>().Set("连接中...", false);
        }

        /// <summary>
        /// 断开服务器连接
        /// </summary>
        public void DisconnectFromServer()
        {
            PhotonNetwork.Disconnect();
        }

        /// <summary>
        /// 加入或创建房间
        /// </summary>
        /// <param name="roomName">房间名</param>
        public void JoinOrCreateRoom()
        {
            PhotonNetwork.JoinRandomRoom();

            UIManager.Instance.ShowPanel<HintPanel>().Set("连接中...", false);
            UIManager.Instance.HidePanel<ChooseCharacterPanel>();
        }

        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="roomName">房间名</param>
        public void LeaveRoom()
        {
            UIManager.Instance.HidePanel<MenuPanel>();

            // 单人模式：没连服务器，直接回主菜单
            if (PlayerData.Instance.isSinglePlayer)
            {
                SceneManager.LoadScene("MainMenu");
                return;
            }

            // 回主菜单前关掉场景同步，避免 Photon 再尝试把主菜单场景同步进房间属性
            PhotonNetwork.AutomaticallySyncScene = false;

            // 断开连接（会自动离开房间）
            DisconnectFromServer();

            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 倒计时结束时间的房间属性 key（所有客户端共享同一个结束时刻）
        /// </summary>
        public const string CountDownEndKey = "CountDownEnd";

        /// <summary>
        /// 游戏是否已开始的房间属性 key（倒计时结束后由房主置为 true，防止再次倒数）
        /// </summary>
        public const string GameStartedKey = "GameStarted";

        /// <summary>
        /// 当前是否有正在进行的倒计时（结束时间已设置且大于 0）
        /// 结束时间 = 0 表示“无倒计时/已取消”
        /// </summary>
        public bool IsCountDownRunning
        {
            get
            {
                if (PhotonNetwork.CurrentRoom == null)
                    return false;

                var props = PhotonNetwork.CurrentRoom.CustomProperties;
                if (!props.ContainsKey(CountDownEndKey))
                    return false;

                return Convert.ToInt64(props[CountDownEndKey]) > 0;
            }
        }

        /// <summary>
        /// 游戏是否已开始（倒计时结束后为 true，此时不会再发起倒计时）
        /// </summary>
        public bool IsGameStarted
        {
            get
            {
                if (PhotonNetwork.CurrentRoom == null)
                    return false;

                var props = PhotonNetwork.CurrentRoom.CustomProperties;
                return props.ContainsKey(GameStartedKey) && Convert.ToBoolean(props[GameStartedKey]);
            }
        }

        /// <summary>
        /// 由房主（MasterClient）发起倒计时：把“服务器结束时间”写入房间属性，
        /// Photon 会自动把它同步给房间内所有客户端。
        /// </summary>
        /// <param name="seconds">倒计时秒数</param>
        public void StartCountDown(int seconds)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            // 结束时间用“真实世界 Unix 时间”（毫秒），所有客户端任何时候都能读到相同的值。
            // 不能用 PhotonNetwork.ServerTimestamp：它是服务器 uptime，重连/新连的客户端读到的值不可靠。
            long endUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + seconds * 1000L;

            var props = new ExitGames.Client.Photon.Hashtable
            {
                { CountDownEndKey, endUnixMs }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        /// <summary>
        /// 由房主取消倒计时：把结束时间清零，通知所有客户端停止并隐藏倒计时面板
        /// </summary>
        public void CancelCountDown()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            // 0 表示“无倒计时/已取消”（结束时间一定是大于 0 的毫秒数，不会和 0 冲突）
            var props = new ExitGames.Client.Photon.Hashtable
            {
                { CountDownEndKey, 0L }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        #endregion

        #region 事件回调

        public override void OnConnectedToMaster()
        {
            print("连接服务器成功");

            UIManager.Instance.HidePanel<HintPanel>();

            UIManager.Instance.ShowPanel<SetNamePanel>();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            //断开连接后才切场景
            SceneManager.LoadScene("MainMenu");
        }

        public override void OnJoinedRoom()
        {
            print("加入房间成功");

            UIManager.Instance.HidePanel<HintPanel>();

            PhotonNetwork.LoadLevel("Lobby");
        }

        /// <summary>
        /// 房间属性变化回调：房主设置/取消倒计时后，把它同步给所有客户端
        /// </summary>
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            if (!propertiesThatChanged.ContainsKey(CountDownEndKey))
                return;

            long endUnixMs = Convert.ToInt64(propertiesThatChanged[CountDownEndKey]);

            if (endUnixMs > 0)
            {
                // 开始倒计时
                UIManager.Instance.ShowPanel<CountDownPanel>().StartCountDown(endUnixMs);
                print("显示倒计时面板");
            }
            else
            {
                // 取消倒计时（有人中途退出）：停止并隐藏面板
                var panel = UIManager.Instance.GetPanel<CountDownPanel>();
                if (panel != null)
                    panel.StopCountDown();
                UIManager.Instance.HidePanel<CountDownPanel>();
                print("取消倒计时，隐藏倒计时面板");
            }
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            print("无空闲房间，自动创建新房间");

            CreatRoom();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            EventCenterMgr.Instance.EventTrigger(E_EventType.CreateRoomFailed);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 倒计时数到 0 时触发（所有客户端都会触发）。
        /// 由房主清空倒计时属性、并把“游戏开始”标记置为 true，防止再次倒数。
        /// </summary>
        private void OnCountDownFinish()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            var props = new ExitGames.Client.Photon.Hashtable
            {
                { CountDownEndKey, 0L },   // 清掉倒计时
                { GameStartedKey, true }    // 标记游戏已开始，防止再次倒数
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        private void CreatRoom()
        {
            RoomOptions roomOptions = new RoomOptions()
            {
                MaxPlayers = 2,
                IsVisible = true,
            };

            PhotonNetwork.CreateRoom(null, roomOptions);
        }

        #endregion
    }
}
