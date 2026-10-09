using Photon.Pun;
using Player.Data;
using UnityEngine;

namespace Scene
{
    public class PlayerSpawner : MonoBehaviour
    {
        #region Unity生命周期函数

        void Start()
        {
            // 单人模式：不依赖 Photon，直接用普通 Instantiate 生成角色
            if (PlayerData.Instance.isSinglePlayer)
            {
                GameObject prefab = Resources.Load<GameObject>(PlayerData.Instance.GetCharacterPath());
                Instantiate(prefab, transform.position, Quaternion.identity);
                return;
            }

            // 不在房间内就不生成（比如误启动/离线时）
            if (!PhotonNetwork.InRoom)
                return;

            // 场景里已经存在自己的角色（IsMine）就跳过，防止重复生成
            if (LocalPlayerAlreadySpawned())
                return;

            // 生成自己的角色。
            // 头顶的名字不再在这里手动设置，而是由 PlayerController 读取“拥有者”的
            // 自定义属性 "PlayerName" 统一显示，这样本地/远端玩家都能看到正确的名字。
            PhotonNetwork.Instantiate(
                PlayerData.Instance.GetCharacterPath(),
                transform.position,
                Quaternion.identity
            );
        }

        #endregion

        #region 私有方法

        //判断本地角色是否已经生成过
        private bool LocalPlayerAlreadySpawned()
        {
            foreach (var view in FindObjectsOfType<PhotonView>())
            {
                if (view.IsMine)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
