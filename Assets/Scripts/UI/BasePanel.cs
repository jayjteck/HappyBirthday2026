using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public abstract class BasePanel : MonoBehaviour
    {
        //专门用于控制面板透明度的组件
        private CanvasGroup canvasGroup;
    
        //淡入淡出的速度（改成 protected，方便子面板按需调慢，比如章节标题的慢速淡出）
        protected float alphaSpeed = 10;

        //当前是隐藏还是显示
        public bool isShow = false;

        //当隐藏面板后，要做的事处理
        private UnityAction hideCallBack = null;

        #region Unity生命周期函数

        protected virtual void Awake()
        {
            //获取面板上挂载的组件
            canvasGroup = this.GetComponent<CanvasGroup>();
        
            //如果忘记添加 CanvasGroup组件，就自动添加
            if (canvasGroup == null)
                canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            AddEventListeners();
        }

        protected virtual void Start()
        {
            Init();
        }
    
        protected virtual void Update()
        {
            //淡入（慢慢显示面板）
            if( isShow && canvasGroup.alpha != 1)
            {
                //当处于显示状态时，如果透明度不为 1，就会不停的加到 1，加到 1过后就停止变化
                canvasGroup.alpha += alphaSpeed * Time.deltaTime;
            
                if (canvasGroup.alpha >= 1)
                    canvasGroup.alpha = 1;
            }
            //淡出（慢慢隐藏面板）
            else if( !isShow && canvasGroup.alpha != 0)
            {
                //当处于隐藏状态时，如果透明度不为 0，就会不停的减到 0，减到 0过后就停止变化
                canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            
                if (canvasGroup.alpha <= 0)
                {
                    canvasGroup.alpha = 0;
                
                    //当面板透明度变为0后（淡出完成后）调用事件，执行逻辑
                    hideCallBack?.Invoke();
                }
            }
        }

        private void OnDisable()
        {
            RemoveEventListeners();
        }

        #endregion
    
        /// <summary>
        /// 监听事件的方法，子类所有监听事件都写在其中
        /// </summary>
        protected abstract void AddEventListeners();
    
        /// <summary>
        /// 移除事件监听者的方法
        /// </summary>
        protected abstract void RemoveEventListeners();
    
        /// <summary>
        /// 初始化方法，可以在这里进行面板初始化，以及事件监听
        /// 写成抽象方法，让子类必须去实现
        /// </summary>
        protected abstract void Init();
    
        /// <summary>
        /// 显示自己
        /// </summary>
        public virtual void ShowMe()
        {
            //将透明度设置为 0，以便之后慢慢增加，呈现淡入效果
            canvasGroup.alpha = 0;
        
            isShow = true;
        }

        /// <summary>
        /// 隐藏自己
        /// </summary>
        /// <param name="callBack">外部传进来的函数，当隐藏完面板时要执行的逻辑</param>
        public virtual void HideMe( UnityAction callBack )
        {
            //将透明度设置为 1，以便之后慢慢减少，呈现淡出效果
            canvasGroup.alpha = 1;
        
            isShow = false;

            hideCallBack = callBack;
        }
    }
}
