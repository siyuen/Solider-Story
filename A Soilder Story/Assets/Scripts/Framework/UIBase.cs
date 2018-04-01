using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    public class UIBase : MonoBehaviour
    {
        /*字段*/
        private UIType _CurrentUIType = new UIType();

        /* 属性*/
        //当前UI窗体类型
        public UIType CurrentUIType
        {
            get { return _CurrentUIType; }
            set { _CurrentUIType = value; }
        }

        /// <summary>
        /// 注册点击按钮事件
        /// </summary>
        protected void RegisterButtonObjectEvent(string buttonName, EventTriggerListener.VoidDelegate delHandle)
        {
            GameObject goButton = UnityHelper.FindTheChildNode(this.gameObject, buttonName).gameObject;
            //给按钮注册事件方法
            if (goButton != null)
            {
                EventTriggerListener.Get(goButton).onClick += delHandle;
            }
        }

        /// <summary>
        /// 注册进入按钮事件
        /// </summary>
        protected void RegisterButtonEnterEvent(string buttonName, EventTriggerListener.VoidDelegate delHandle)
        {
            GameObject goButton = UnityHelper.FindTheChildNode(this.gameObject, buttonName).gameObject;
            //给按钮注册事件方法
            if (goButton != null)
            {
                EventTriggerListener.Get(goButton).onEnter += delHandle;
            }
        }

        #region 键盘事件
        /// <summary>
        /// 注册当前UI键盘事件
        /// </summary>
        protected void RegisterKeyBoardEvent()
        {
            InputManager.Instance().RegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
            InputManager.Instance().RegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
            InputManager.Instance().RegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
            InputManager.Instance().RegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
            InputManager.Instance().RegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
            InputManager.Instance().RegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);   
        }

        /// <summary>
        /// 注销当前UI键盘事件
        /// </summary>
        protected void UnRegisterKeyBoardEvent()
        {
            InputManager.Instance().UnRegisterKeyDownEvent(OnConfirmDown, EventType.KEY_Z);
            InputManager.Instance().UnRegisterKeyDownEvent(OnCancelDown, EventType.KEY_X);
            InputManager.Instance().UnRegisterKeyDownEvent(OnUpArrowDown, EventType.KEY_UPARROW);
            InputManager.Instance().UnRegisterKeyDownEvent(OnDownArrowDown, EventType.KEY_DOWNARROW);
            InputManager.Instance().UnRegisterKeyDownEvent(OnLeftArrowDown, EventType.KEY_LEFTARROW);
            InputManager.Instance().UnRegisterKeyDownEvent(OnRightArrowDown, EventType.KEY_RIGHTARROW);   
        }

        public virtual void OnConfirmDown() { }
        public virtual void OnCancelDown() { }
        public virtual void OnUpArrowDown() { }
        public virtual void OnDownArrowDown() { }
        public virtual void OnLeftArrowDown() { }
        public virtual void OnRightArrowDown() { }
        #endregion


        #region 子类常用的方法：打开/关闭UI窗体
        /// <summary>
        /// 打开UI窗体
        /// </summary>
        protected void OpenUIForm(string uiFormName)
        {
            UIManager.Instance().ShowUIForms(uiFormName);
        }

        /// <summary>
        /// 关闭当前UI窗体
        /// </summary>
        protected void CloseUIForm()
        {
            string strUIFromName = string.Empty;            //处理后的UIFrom 名称
            int intPosition = -1;

            strUIFromName = GetType().ToString();             //命名空间+类名
            intPosition = strUIFromName.IndexOf('.');
            if (intPosition != -1)
            {
                //剪切字符串中“.”之间的部分
                strUIFromName = strUIFromName.Substring(intPosition + 1);
            }

            UIManager.Instance().CloseUIForms(strUIFromName);
        }

        /// <summary>
        /// 获取canvas
        /// </summary>
        /// <returns></returns>
        protected Canvas GetCanvas()
        {
            return UIManager.Instance().m_Canvas;
        }
        #endregion

        #region 窗体的四种生命周期状态
        /// <summary>
        /// 显示状态
        /// </summary>
        public virtual void Display()
        {
            this.gameObject.SetActive(true);
            //设置模态窗体调用(必须是弹出窗体)
            if (_CurrentUIType.UIForms_Type == UIFormType.PopUp)
            {
                UIMaskMgr.GetInstance().SetMaskWindow(this.gameObject, _CurrentUIType.UIForm_LucencyType);
            }
        }

        /// <summary>
        /// 隐藏状态
        /// </summary>
        public virtual void Hiding()
        {
            this.gameObject.SetActive(false);
            //取消模态窗体调用
            if (_CurrentUIType.UIForms_Type == UIFormType.PopUp)
            {
                UIMaskMgr.GetInstance().CancelMaskWindow();
            }
        }

        /// <summary>
        /// 重新显示状态
        /// </summary>
        public virtual void Redisplay()
        {
            this.gameObject.SetActive(true);
            //设置模态窗体调用(必须是弹出窗体)
            if (_CurrentUIType.UIForms_Type == UIFormType.PopUp)
            {
                UIMaskMgr.GetInstance().SetMaskWindow(this.gameObject, _CurrentUIType.UIForm_LucencyType);
            }
        }

        /// <summary>
        /// 冻结状态
        /// </summary>
        public virtual void Freeze()
        {
            this.gameObject.SetActive(true);
        }

        #endregion
    }
}
