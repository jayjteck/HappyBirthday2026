using System.Collections.Generic;
using Singleton;
using UnityEngine;

namespace UI
{
    public class UIManager : SingletonBase<UIManager>
    {
        #region 私有字段

        //该字典用于存储显示着的面板，每个显示的面板都会存入这个字典
        //隐藏面板时，直接获取字典中的对应面板，进行隐藏
        /// <summary>
        /// key：面板名字
        /// value：面板
        /// </summary>
        private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

        //得到场景中的 Canvas对象，用于设置为面板的父对象
        private Transform canvasTrans;

        #endregion
    
        //私有构造函数
        //单例模式会在最开始调用一次构造函数
        private UIManager()
        {
            //实例化 Canvas预设体
            GameObject canvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        
            //得到该 Canvas对象
            canvasTrans = canvas.transform;
        
            //通过过场景不移除该对象，保证这个游戏过程中，只有一个 canvas对象
            GameObject.DontDestroyOnLoad(canvas);
        }

        #region 公共属性

        public Canvas MainCanvas => canvasTrans.gameObject.GetComponent<Canvas>();

        #endregion
    
        #region 公共方法

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <typeparam name="T">面板类名</typeparam>
        /// <returns>返回要显示的面板脚本</returns>
        public T ShowPanel<T>() where T:BasePanel
        {
            //规定面板名字与面板类名一致
            string panelName = typeof(T).Name;

            //判断字典中是否有这个面板，有的话就直接返回要显示的面板
            if (panelDic.ContainsKey(panelName))
                return panelDic[panelName] as T;

            //如果字典中没有要显示的面板，就根据面板名字动态实例化预设体
            GameObject panelObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/" + panelName));
        
            //把这个对象放到场景中的 Canvas下面
            panelObj.transform.SetParent(canvasTrans, false);

            //得到预设体上的面板脚本，用于返回出去
            T panel = panelObj.GetComponent<T>();
        
            //把这个面板脚本存储到字典中，方便之后的获取和隐藏
            panelDic.Add(panelName, panel);
        
            //显示面板
            panel.ShowMe();

            return panel;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        /// <typeparam name="T">面板类名</typeparam>
        /// <param name="isFade">是否淡出完毕过后才删除面板，默认是 ture</param>
        public void HidePanel<T>(bool isFade = true) where T:BasePanel
        {
            //规定面板名字与面板类名一致
            string panelName = typeof(T).Name;
        
            //判断字典中有没有该面板
            //即：要隐藏的面板是否显示了
            if (panelDic.ContainsKey(panelName)) 
            {
                //是否淡出完毕过后才删除面板
                if (isFade) 
                {
                    //让面板淡出完毕过后再删除它 
                    panelDic[panelName].HideMe(() =>
                    {
                        //删除对象
                        GameObject.Destroy(panelDic[panelName].gameObject);
                    
                        //删除字典里面存储的面板脚本
                        panelDic.Remove(panelName);
                    });
                }
                //直接删除面板对象
                else
                {
                    //删除对象
                    GameObject.Destroy(panelDic[panelName].gameObject);
                
                    //删除字典里面存储的面板脚本
                    panelDic.Remove(panelName);
                }
            }
        }

        /// <summary>
        /// 得到面板
        /// </summary>
        /// <typeparam name="T">面板类名</typeparam>
        /// <returns>返回要的到的面板脚本</returns>
        public T GetPanel<T>() where T:BasePanel
        {
            //规定面板名字与面板类名一致
            string panelName = typeof(T).Name;
        
            //如果字典中有该面板，就返回它
            if (panelDic.ContainsKey(panelName))
                return panelDic[panelName] as T;
        
            //如果字典中没有该面板，就返回 null
            return null;
        }

        #endregion
    }
}
