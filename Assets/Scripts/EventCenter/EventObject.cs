using UnityEngine.Events;

namespace EventCenter
{
    /// <summary>
    /// 无参数事件对象
    /// </summary>
    public class EventObject : EventObjectBase
    {
        //被这个类包裹的委托变量，所有监听者都是监听这个事件（委托）
        public UnityAction actions;
    
        //通过构造函数，为该事件添加监听者
        public EventObject(UnityAction action)
        {
            actions += action;
        }
    }

    /// <summary>
    /// 有参数事件对象
    /// </summary>
    /// <typeparam name="T">要传的参数类型</typeparam>
    public class EventObject<T> : EventObjectBase
    {
        //被这个类包裹的委托变量，所有监听者都是监听这个事件（委托）
        public UnityAction<T> actions;

        //通过构造函数，为该事件添加监听者
        public EventObject(UnityAction<T> action)
        {
            actions += action;
        }
    }
}