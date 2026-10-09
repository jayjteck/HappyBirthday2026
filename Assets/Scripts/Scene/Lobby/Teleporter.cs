using EventCenter;
using Network;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;

namespace Scene.Lobby
{
    public class Teleporter : MonoBehaviourPunCallbacks
    {
        #region 私有字段

        private Room _room;
        
        private UnityAction _onCountDownFinish;

        #endregion

        #region Unity生命周期函数

        void Start()
        {
            _room = PhotonNetwork.CurrentRoom;
            
            //只有房主负责切场景（ AutomaticallySyncScene = true 会自动同步给其他客户端）
            _onCountDownFinish = () =>
            {
                if (PhotonNetwork.IsMasterClient)
                    PhotonNetwork.LoadLevel("Chapter01");
            };
            EventCenterMgr.Instance.AddEventListener(E_EventType.CountDownFinish, _onCountDownFinish);
        }

        private void OnDestroy()
        {
            EventCenterMgr.Instance.RemoveEventListener(E_EventType.CountDownFinish, _onCountDownFinish);
        }

        void Update()
        {
            // 不在房间里就不处理
            if (!PhotonNetwork.InRoom || _room == null)
                return;

            // 只有房主负责发起倒计时，其余客户端等待房间属性同步即可
            if (!PhotonNetwork.IsMasterClient)
                return;

            // 人数已满、且当前没有正在进行的倒计时、且游戏还没开始，就由房主发起一次开始倒计时
            if (_room.PlayerCount >= _room.MaxPlayers &&
                !NetworkManager.Instance.IsCountDownRunning &&
                !NetworkManager.Instance.IsGameStarted)
            {
                NetworkManager.Instance.StartCountDown(10);
            }
        }

        #endregion

        #region 事件回调

        /// <summary>
        /// 有人退出房间时触发（所有剩余玩家都会收到）
        /// 若倒计时正在进行、且人数现在不满了，就由房主取消倒计时
        /// </summary>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            // 只有房主负责取消（把结束时间清零，通知所有客户端）
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
                return;

            // 倒计时正在进行、且人数现在不足 → 取消倒计时
            if (NetworkManager.Instance.IsCountDownRunning &&
                PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                NetworkManager.Instance.CancelCountDown();
            }
        }

        #endregion
    }
}
