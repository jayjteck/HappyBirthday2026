using Photon.Pun;
using Player.Data;
using UnityEngine;

namespace Player
{
    public class CameraFollow : MonoBehaviour
    {
        #region 公开字段
    
        public Vector2 offset;

        #endregion

        #region 私有字段

        private Camera _mainCamera;
        private Transform _player;

        #endregion

        #region Unity生命周期函数

        private void Start()
        {
            _mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        }
    
        void Update()
        {
            // 还没找到自己的角色时，每帧重试查找（角色是加入房间后才生成的）
            if (_player == null)
            {
                _player = FindLocalPlayer();
                return;
            }
        
            //摄像机跟随
            _mainCamera.transform.position = _player.position + (Vector3)offset + Vector3.back * 10;
        }

        #endregion
    
        #region 私有方法

        //找到本地玩家（IsMine）的角色
        private Transform FindLocalPlayer()
        {
            //单人模式：场景里只有一个角色，直接找 PlayerController
            if (PlayerData.Instance.isSinglePlayer)
            {
                PlayerController controller = FindObjectOfType<PlayerController>();
                
                return controller != null ? controller.transform : null;
            }

            foreach (PhotonView view in FindObjectsOfType<PhotonView>())
            {
                if (view.IsMine)
                    return view.transform;
            }
            
            return null;
        }

        #endregion
    
    }
}
