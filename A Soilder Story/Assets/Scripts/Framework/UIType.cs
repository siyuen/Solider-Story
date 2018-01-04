using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    public class UIType
    {
        //是否清空“栈集合”
        public bool IsClearReverseChange = false;
        //UI窗体（位置）类型
        public UIFormType UIForms_Type = UIFormType.Normal;
        //UI窗体显示类型
        public UIFormShowMode UIForms_ShowMode = UIFormShowMode.Normal;
        //UI窗体透明度类型
        public UIFormLucenyType UIForm_LucencyType = UIFormLucenyType.Lucency;
    }
}
