using Photon.Pun;
using Player;
using Player.Data;
using UnityEngine;

namespace Scene
{
    /// <summary>
    /// 传送点：放在 Chapter08 场景里，任意玩家走进触发区，就把该玩家（仅这一个）传送到目标位置
    /// 通过 RPC 传送，保证所有客户端都把该玩家瞬移到目标点
    /// </summary>
    public class TeleportPoint : MonoBehaviour
    {
        #region 公开字段

        [Header("传送目标点（拖入一个空物体，放在目的地）")]
        public Transform targetPoint;

        #endregion

        #region Unity生命周期函数

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 单人模式：没有网络，直接调用角色身上的 Teleport 方法本地传送
            if (PlayerData.Instance.isSinglePlayer)
            {
                var controller = other.GetComponentInParent<PlayerController>();
                if (controller != null)
                    controller.Teleport(targetPoint.position);
                return;
            }

            var view = other.GetComponentInParent<PhotonView>();
            if (view == null || !view.IsMine)
                return;

            //只传送“自己”这个角色，RpcTarget.All 让所有客户端都把该角色瞬移到目标点
            view.RPC("Teleport", RpcTarget.All, targetPoint.position);
        }

        #endregion
    }
}
