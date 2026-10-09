using System;
using EventCenter;
using TMPro;
using UnityEngine;

namespace UI
{
    public class CountDownPanel : BasePanel
    {
        #region 公开字段

        public TextMeshProUGUI countDownText;

        #endregion

        #region 私有字段

        // 统一约定的“结束时间点”（真实世界 Unix 时间，毫秒）。所有客户端读到的是同一个值。
        private long _endUnixMs;

        // 本地锚点：记录收到结束时间的那一刻，之后完全交给本地真实时钟推进，保证与现实世界同速。
        private float  _anchorLocalTime;
        private double _anchorRemainingSec;

        private bool _isCounting;

        #endregion

        protected override void AddEventListeners()
        {
        }

        protected override void RemoveEventListeners()
        {
        }

        protected override void Init()
        {
        }

        protected override void Update()
        {
            // 先执行基类的淡入淡出逻辑，否则面板会一直停在 alpha=0（透明不可见）
            base.Update();

            if (!_isCounting)
                return;

            //用本地高精度时钟推进，流速与现实世界一致，且不受 Time.timeScale 影响
            //现在还剩余的时间 = 收到倒计时结束时间那一刻的总剩余时间 -（这一刻的本地时间 - 收到倒计时结束时间那一刻的本地时间）
            double remaining = _anchorRemainingSec - (Time.unscaledTime - _anchorLocalTime);

            if (remaining > 0)
            {
                //向上取整
                countDownText.text = $"离游戏开始还有:{Mathf.CeilToInt((float)remaining).ToString()}秒";
            }
            else
            {
                _isCounting = false;
                countDownText.text = "离游戏开始还有:0秒";

                // 倒计时结束，通知游戏其它模块（例如正式开始游戏）
                EventCenterMgr.Instance.EventTrigger(E_EventType.CountDownFinish);

                UIManager.Instance.HidePanel<CountDownPanel>();
            }
        }

        #region 私有方法

        /// <summary>
        /// 用统一约定的结束时间（真实世界 Unix 毫秒）启动倒计时
        /// （由 NetworkManager 收到房间属性后调用）
        /// </summary>
        /// <param name="endUnixMs">Unix 时间戳（毫秒），倒计时结束的时刻</param>
        public void StartCountDown(long endUnixMs)
        {
            //得到房主设置的倒计时结束时间
            _endUnixMs = endUnixMs;

            // 关键：用“结束时间 - 当前真实世界时间”算出初始剩余秒数，之后完全交给本地真实时钟推进。
            // 这样所有客户端都指向同一个结束时刻，自然就同步了。
            long nowUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _anchorRemainingSec = (_endUnixMs - nowUnixMs) / 1000.0;

            //记录这一刻的本地时间
            _anchorLocalTime = Time.unscaledTime;

            _isCounting = true;
        }

        /// <summary>
        /// 停止倒计时（比如有人中途退出、倒计时被取消时调用）
        /// </summary>
        public void StopCountDown()
        {
            _isCounting = false;
        }

        #endregion
    }
}
