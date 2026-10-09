using Network;
using Photon.Pun;
using Player.Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        #region 公开字段

        public float speed = 10;
        public float jumpForce = 8;
        
        public Transform groundCheck;// 脚底检测点（角色脚下挂一个空物体子节点）
        public LayerMask groundLayer;// 地面所在 Layer，只把这层算作"可以落地"

        #endregion
        
        #region 私有字段

        private Animator _animator;
        private PhotonView _photonView;
        private Transform _modelTransform;
        private Rigidbody2D  _rigidbody2D;

        private bool _isGameScene = false;
        private bool _isEnd = false;

        // 当前是否朝右（默认朝右），朝向改的是子物体 Skeletal 的 localScale
        // Photon 的内置同步组件不碰子物体缩放，所以要用 RPC 手动同步给别的客户端
        private bool _facingRight = true;
        
        private const float GroundCheckRadius = 0.5f;//接地检测半径

        /// <summary>
        /// 是否是本地角色
        /// 单人模式下没有 PhotonView 所有权，视为“就是自己的角色”
        /// </summary>
        private bool IsLocalPlayer => PlayerData.Instance.isSinglePlayer || (_photonView != null && _photonView.IsMine);

        #endregion

        #region Unity生命周期函数

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _photonView = GetComponent<PhotonView>();
            _modelTransform = transform.Find("Skeletal");
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        // 名字不能写在 Awake里，因为 PhotonNetwork.Instantiate会先 Instantiate（此时 Awake 已执行完、 PhotonView.Owner还没赋值），再设置 ViewID / Owner
        // 所以放到 Start 里读取才正确
        private void Start()
        {
            SetName();
            
            //根据所在场景决定控制模式
            _isGameScene = SceneManager.GetActiveScene().name.Contains("Chapter");
            
            //判断是否是结束场景
            _isEnd = SceneManager.GetActiveScene().name.Contains("End");
            
            _rigidbody2D.gravityScale = _isGameScene ? 1 : 0;
        }

        private void Update()
        {
            //跳跃按键要在 Update 里检测，放 FixedUpdate 里容易漏掉
            if (_isGameScene) 
            {
                Jump();
            }
            
            if (_isGameScene || _isEnd) 
            {
                OpenBackpack();
            }
                
        }

        private void FixedUpdate()
        {
            if (!_isGameScene)
            {
                MoveInLobby();
            }
            else
            {
                MoveInGaming();
            }
            
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 大厅中是 wasd控制上下左右移动的方式
        /// </summary>
        private void MoveInLobby()
        {
            //只控制自己的角色
            //别人的角色位置/动画由 PhotonTransformView / PhotonAnimatorView 同步
            if (!IsLocalPlayer)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            //合并两个轴得到移动向量
            Vector3 moveVelocity = new Vector3(horizontal, vertical, 0);

            //斜向时归一化，避免斜向移动比直线更快
            if (moveVelocity.magnitude > 1f)
            {
                moveVelocity.Normalize();
            }

            bool isMoving = horizontal != 0f || vertical != 0f;
            _animator.SetBool("isRun", isMoving);

            //仅在水平方向有输入时翻转角色朝向，方向固定赋符号，避免每帧取反导致闪烁
            UpdateFacing(horizontal);

            transform.position += moveVelocity * (speed * Time.deltaTime);
        }

        /// <summary>
        /// 游戏中只有左右控制移动，w是跳跃键
        /// </summary>
        private void MoveInGaming()
        {
            if (!IsLocalPlayer)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            
            //只改水平速度，保留 rb 现有的竖直速度（重力和跳跃都交给物理处理）
            _rigidbody2D.velocity = new Vector2(horizontal * speed, _rigidbody2D.velocity.y);
            
            _animator.SetBool("isRun", horizontal != 0f);

            // 仅在水平方向有输入时翻转角色朝向，方向固定赋符号，避免每帧取反导致闪烁
            UpdateFacing(horizontal);
        }

        /// <summary>
        /// 根据水平输入决定朝向：有输入时按方向翻转，无输入时保持原朝向。
        /// 朝向真正变化时才翻转并 RPC 同步一次，不用每帧都发。
        /// </summary>
        private void UpdateFacing(float horizontal)
        {
            bool facingRight = _facingRight;
            
            if (horizontal < 0) 
                facingRight = false;
            else if (horizontal > 0) 
                facingRight = true;

            if (facingRight == _facingRight)
                return;

            SetFacing(facingRight);

            // 只把变化同步给其他客户端（单人模式没有 PhotonView，跳过）
            if (_photonView != null && !PlayerData.Instance.isSinglePlayer)
            {
                _photonView.RPC(nameof(RPC_SetFacing), RpcTarget.Others, facingRight);
            }
        }

        /// <summary>
        /// 翻转模型朝向，朝向改的是子物体 Skeletal 的 localScale，
        /// PhotonTransformView / PhotonAnimatorView 都不会同步子物体缩放，所以必须手动同步给别的客户端
        /// </summary>
        private void SetFacing(bool facingRight)
        {
            _facingRight = facingRight;
            float x = Mathf.Abs(_modelTransform.localScale.x) * (facingRight ? 1f : -1f);
            _modelTransform.localScale = new Vector3(x, _modelTransform.localScale.y, _modelTransform.localScale.z);
        }

        [PunRPC]
        private void RPC_SetFacing(bool facingRight)
        {
            SetFacing(facingRight);
        }

        private void Jump()
        {
            // 只控制自己的角色
            if (!IsLocalPlayer)
                return;

            bool grounded = IsGrounded();

            // isJump 表示"当前是否在空中"：空中 = true，落地 = false，动画跟着状态走，不会时有时无
            _animator.SetBool("isJump", !grounded);

            if (grounded && Input.GetKeyDown(KeyCode.W))
            {
                _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, jumpForce);
            }
        }
        
        private bool IsGrounded()
        {
            // 没有配置检测点就默认视为接地，避免漏配导致永远跳不起来
            if (groundCheck is null)
                return true;

            return Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer) is not null;
        }
        
        /// <summary>
        /// 每个客户端都读取该角色“拥有者”的名字并显示到头顶。
        /// 名字通过玩家自定义属性 "PlayerName" 同步（在 SetNamePanel 里写入），
        /// 因此本地玩家和远端玩家都能正确显示。
        /// </summary>
        private void SetName()
        {
            // 单人模式：没有 Photon 玩家属性，直接读 PlayerData 里设置的名字
            if (PlayerData.Instance.isSinglePlayer)
            {
                var singleNameText = GetComponentInChildren<TextMeshPro>();
                if (singleNameText != null)
                    singleNameText.text = PlayerData.Instance.playerName;
                return;
            }

            if (_photonView == null || _photonView.Owner == null)
                return;

            var nameText = GetComponentInChildren<TextMeshPro>();
            if (nameText == null)
                return;

            // 优先读玩家自定义属性里的名字
            if (_photonView.Owner.CustomProperties.ContainsKey(PlayerPropertyKey.PlayerName))
            {
                string name = _photonView.Owner.CustomProperties[PlayerPropertyKey.PlayerName] as string;
                if (!string.IsNullOrEmpty(name))
                {
                    nameText.text = name;
                    return;
                }
            }

            // 兜底：用 Photon 内置的 NickName
            nameText.text = _photonView.Owner.NickName;
        }

        //打开背包
        private void OpenBackpack()
        {
            // 只有自己按 B 才开自己的背包
            if (!IsLocalPlayer)
                return;
            
            if (Input.GetKeyDown(KeyCode.B))
                UIManager.Instance.ShowPanel<BackpackPanel>();
        }
        
        #endregion

        #region 公开方法

        /// <summary>
        /// 传送：把本角色瞬移到目标位置。
        /// 由 TeleportPoint 通过 RPC 调用，所有客户端都会执行，保证传送在别人屏幕上也瞬间同步。
        /// </summary>
        [PunRPC]
        public void Teleport(Vector3 targetPos)
        {
            if (_rigidbody2D != null)
            {
                _rigidbody2D.position = targetPos;
                _rigidbody2D.velocity = Vector2.zero;
            }
            else
            {
                transform.position = targetPos;
            }
        }

        #endregion
    }
}
