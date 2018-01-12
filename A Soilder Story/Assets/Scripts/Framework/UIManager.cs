using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using DG.Tweening;

namespace UIFramework
{
    public class UIManager : QMonoSingleton<UIManager>
    {
        /* 字段 */
        //private static UIManager _Instance = null;
        //UI窗体预设路径(参数1：窗体预设名称，2：表示窗体预设路径)
        private Dictionary<string, string> _DicFormsPaths;
        //缓存所有UI窗体
        private Dictionary<string, UIBase> _DicALLUIForms;
        //当前显示的UI窗体
        private Dictionary<string, UIBase> _DicCurrentShowUIForms;
        //“栈”结构表示的“当前UI窗体”集合。
        private Stack<UIBase> _StaCurrentUIForms;
        //UI根节点
        private Transform _TraCanvasTransfrom = null;
        //全屏幕显示的节点
        private Transform _TraNormal = null;
        //固定显示的节点
        private Transform _TraFixed = null;
        //弹出节点
        private Transform _TraPopUp = null;
        //UI管理脚本的节点
        private Transform _TraUIScripts = null;
        //uiCamera
        private Camera uiCamera = null;
        //canvase的9个方位pos
        private List<Vector3> uiPosList = new List<Vector3>();

        public Canvas m_Canvas;

        //初始化核心数据，加载“UI窗体路径”到集合中。
        public void Awake()
        {
            //字段初始化
            _DicALLUIForms = new Dictionary<string, UIBase>();
            _DicCurrentShowUIForms = new Dictionary<string, UIBase>();
            _DicFormsPaths = new Dictionary<string, string>();
            _StaCurrentUIForms = new Stack<UIBase>();

            //初始化加载（根UI窗体）Canvas预设
            InitRootCanvasLoading();

            //得到UI根节点、全屏节点、固定节点、弹出节点
            _TraCanvasTransfrom = GameObject.FindGameObjectWithTag(SysDefine.SYS_TAG_CANVAS).transform;
            _TraNormal = _TraCanvasTransfrom.Find("Normal");
            _TraFixed = _TraCanvasTransfrom.Find("Fixed");
            _TraPopUp = _TraCanvasTransfrom.Find("PopUp");
            //_TraUIScripts = _TraCanvasTransfrom.Find("_ScriptMgr");

            //把本脚本作为“根UI窗体”的子节点。
            //this.gameObject.transform.SetParent(_TraUIScripts, false);

            //"根UI窗体"在场景转换的时候，不允许销毁
            DontDestroyOnLoad(_TraCanvasTransfrom);

            //初始化“UI窗体预设”路径数据
            //先写简单的，后面我们使用Json做配置文件，来完善。
            if (_DicFormsPaths != null)
            {
                _DicFormsPaths.Add("HeroMenu", @"Prefabs\UI\Character\HeroMenu");
                _DicFormsPaths.Add("CharacterData_1", @"Prefabs\UI\Character\CharacterData_1");
                _DicFormsPaths.Add("CharacterData_2", @"Prefabs\UI\Character\CharacterData_2");
                _DicFormsPaths.Add("LandData_1", @"Prefabs\UI\Terrain\LandData_1");
                _DicFormsPaths.Add("LandData_2", @"Prefabs\UI\Terrain\LandData_2");
                _DicFormsPaths.Add("GameGoal_1", @"Prefabs\UI\GameGoal_1");
                _DicFormsPaths.Add("GameGoal_2", @"Prefabs\UI\GameGoal_2");
                _DicFormsPaths.Add("WeaponSelectMenu", @"Prefabs\UI\View\WeaponSelectMenu");
                _DicFormsPaths.Add("FightData", @"Prefabs\UI\Fight\FightData");
            }

            //获取uiCamera
            uiCamera = UnityHelper.GetTheChildNodeComponetScripts<Camera>(m_Canvas.gameObject, "UICamera");
            //设为800 * 600根据高度适配，没有找到怎么动态获取的？
            float height = 600;
            float rate = m_Canvas.pixelRect.height / height;
            float width = m_Canvas.pixelRect.width / rate;
            
            //设置三行三列九个位置的pos，方便设置ui
            uiPosList.Add(new Vector3(-width / 2, height / 2, 0));
            uiPosList.Add(new Vector3(0, height / 2, 0));
            uiPosList.Add(new Vector3(width / 2, height / 2, 0));
            uiPosList.Add(new Vector3(-width / 2, 0, 0));
            uiPosList.Add(new Vector3(0, 0, 0));
            uiPosList.Add(new Vector3(width / 2, 0, 0));
            uiPosList.Add(new Vector3(-width / 2, -height / 2, 0));
            uiPosList.Add(new Vector3(0, -height / 2, 0));
            uiPosList.Add(new Vector3(width / 2, -height / 2, 0));
        }

        /// <summary>
        /// 获取设置好的ui位置
        /// </summary>
        public Vector3 GetUIDefaultPos(int idx)
        {
            if (uiPosList.Count == 0 || idx >= uiPosList.Count)
                return Vector3.zero;
            return uiPosList[idx];
        }

        /// <summary>
        /// 获取UICamera
        /// </summary>
        public Camera GetUICamera()
        {
            return uiCamera;
        }

        /// <summary>
        /// 显示（打开）UI窗体
        /// 功能：
        /// 1: 根据UI窗体的名称，加载到“所有UI窗体”缓存集合中
        /// 2: 根据不同的UI窗体的“显示模式”，分别作不同的加载处理
        /// </summary>
        /// <param name="uiFormName">UI窗体预设的名称</param>
        public void ShowUIForms(string uiFormName)
        {
            UIBase baseUIForms = null;                    //UI窗体基类

            //参数的检查
            if (string.IsNullOrEmpty(uiFormName)) return;

            //根据UI窗体的名称，加载到“所有UI窗体”缓存集合中
            baseUIForms = LoadFormsToAllUIFormsCatch(uiFormName);

             //判断是否清空“栈”结构体集合
            if (baseUIForms.CurrentUIType.IsClearReverseChange)
            {
                ClearStackArray();
            }

            if (baseUIForms == null) return;

            //根据不同的UI窗体的显示模式，分别作不同的加载处理
            switch (baseUIForms.CurrentUIType.UIForms_ShowMode)
            {
                case UIFormShowMode.Normal:                 //“普通显示”窗口模式
                    //把当前窗体加载到“当前窗体”集合中。
                    EnterUIFormsCache(uiFormName);
                    break;
                case UIFormShowMode.ReverseChange:          //需要“反向切换”窗口模式
                    PushUIFormToStack(uiFormName);
                    break;
                case UIFormShowMode.HideOther:              //“隐藏其他”窗口模式
                    EnterUIFormsAndHideOther(uiFormName);
                    break;
                case UIFormShowMode.Tween:                  //“动画”模式
                    EnterUIFormsTween(uiFormName);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 关闭（返回上一个）窗体
        /// </summary>
        /// <param name="uiFormName"></param>
        public void CloseUIForms(string uiFormName)
        {
            UIBase baseUiForm;                          //窗体基类

            //参数检查
            if (string.IsNullOrEmpty(uiFormName)) return;

            //“所有UI窗体”集合中，如果没有记录，则直接返回
            _DicALLUIForms.TryGetValue(uiFormName, out baseUiForm);
            if (baseUiForm == null) return;

            //根据窗体不同的显示类型，分别作不同的关闭处理
            switch (baseUiForm.CurrentUIType.UIForms_ShowMode)
            {
                case UIFormShowMode.Normal:
                    //普通窗体的关闭
                    ExitUIForms(uiFormName);
                    break;
                case UIFormShowMode.ReverseChange:
                    //反向切换窗体的关闭
                    PopUIForms();
                    break;
                case UIFormShowMode.HideOther:
                    //隐藏其他窗体关闭
                    ExitUIFormsAndDisplayOther(uiFormName);
                    break;
                case UIFormShowMode.Tween:
                    ExitUIFormsTween(uiFormName);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 获取UI是否显示
        /// </summary>
        /// <param name="uiFormName"></param>
        /// <returns></returns>
        public bool GetUIActive(string uiFormName)
        {
            UIBase baseUIForms;                        //UI窗体基类

            //“正在显示UI窗体缓存”集合里有记录，则直接返回。
            _DicCurrentShowUIForms.TryGetValue(uiFormName, out baseUIForms);
            if (baseUIForms != null) 
                return true;

            return false;
        }

        /// <summary>
        /// 获取UI
        /// </summary>
        /// <param name="uiFormName"></param>
        /// <returns></returns>
        public UIBase GetUI(string uiFormName)
        {
            UIBase baseUIForms;
            _DicCurrentShowUIForms.TryGetValue(uiFormName, out baseUIForms);
            return baseUIForms;
        }

        #region 私有方法
        //初始化加载（根UI窗体）Canvas预设
        private void InitRootCanvasLoading()
        {
            Debug.Log("初始化加载canvas");
            m_Canvas = ResourcesMgr.Instance().LoadAsset(SysDefine.SYS_PATH_CANVAS, false).GetComponent<Canvas>();
        }

        /// <summary>
        /// 根据UI窗体的名称，加载到“所有UI窗体”缓存集合中
        /// 功能： 检查“所有UI窗体”集合中，是否已经加载过，否则才加载。
        /// </summary>
        private UIBase LoadFormsToAllUIFormsCatch(string uiFormsName)
        {
            UIBase baseUIResult = null;                 //加载的返回UI窗体基类

            _DicALLUIForms.TryGetValue(uiFormsName, out baseUIResult);

            if (baseUIResult == null)
            {
                //加载指定名称的“UI窗体”
                baseUIResult = LoadUIForm(uiFormsName);
            }

            return baseUIResult;
        }

        /// <summary>
        /// 加载指定名称的“UI窗体”
        /// 功能：
        ///    1：根据“UI窗体名称”，加载预设克隆体。
        ///    2：根据不同预设克隆体中带的脚本中不同的“位置信息”，加载到“根窗体”下不同的节点。
        ///    3：隐藏刚创建的UI克隆体。
        ///    4：把克隆体，加入到“所有UI窗体”（缓存）集合中。
        /// </summary>
        private UIBase LoadUIForm(string uiFormName)
        {
            string strUIFormPaths = null;                   //UI窗体路径
            GameObject goCloneUIPrefabs = null;             //创建的UI克隆体预设
            UIBase baseUiForm = null;                     //窗体基类


            //根据UI窗体名称，得到对应的加载路径
            _DicFormsPaths.TryGetValue(uiFormName, out strUIFormPaths);

            Debug.Log(strUIFormPaths);

            //根据“UI窗体名称”，加载“预设克隆体”
            if (!string.IsNullOrEmpty(strUIFormPaths))
            {
                goCloneUIPrefabs = ResourcesMgr.Instance().LoadAsset(strUIFormPaths, false);
            }

            //设置“UI克隆体”的父节点（根据克隆体中带的脚本中不同的“位置信息”）
            if (_TraCanvasTransfrom != null && goCloneUIPrefabs != null)
            {
                baseUiForm = goCloneUIPrefabs.GetComponent<UIBase>();
                if (baseUiForm == null)
                {
                    Debug.Log("baseUiForm==null! ,请先确认窗体预设对象上是否加载了baseUIForm的子类脚本！ 参数 uiFormName=" + uiFormName);
                    return null;
                }
                switch (baseUiForm.CurrentUIType.UIForms_Type)
                {
                    case UIFormType.Normal: //普通窗体节点
                        goCloneUIPrefabs.transform.SetParent(_TraNormal, false);
                        break;
                    case UIFormType.Fixed: //固定窗体节点
                        goCloneUIPrefabs.transform.SetParent(_TraFixed, false);
                        break;
                    case UIFormType.PopUp: //弹出窗体节点
                        goCloneUIPrefabs.transform.SetParent(_TraPopUp, false);
                        break;
                    default:
                        break;
                }

                //设置隐藏
                goCloneUIPrefabs.SetActive(false);
                //把克隆体，加入到“所有UI窗体”（缓存）集合中。
                _DicALLUIForms.Add(uiFormName, baseUiForm);

                return baseUiForm;
            }
            else
            {
                Debug.Log("_TraCanvasTransfrom==null Or goCloneUIPrefabs==null!! ,Plese Check!, 参数uiFormName=" + uiFormName);
            }

            Debug.Log("出现不可以预估的错误，请检查，参数 uiFormName=" + uiFormName);
            return null;
        }//Mehtod_end

        /// <summary>
        /// 加载UI窗体到“当前显示窗体集合”缓存中。
        /// </summary>
        private void EnterUIFormsCache(string strUIFormsName)
        {
            UIBase baseUIForms;                        //UI窗体基类
            UIBase baseUIFormsFromAllCache;            //"所有窗体集合"中的窗体基类

            //“正在显示UI窗体缓存”集合里有记录，则直接返回。
            _DicCurrentShowUIForms.TryGetValue(strUIFormsName, out baseUIForms);
            if (baseUIForms != null) return;

            //把当前窗体，加载到“正在显示UI窗体缓存”集合里
            _DicALLUIForms.TryGetValue(strUIFormsName, out baseUIFormsFromAllCache);
            if (baseUIFormsFromAllCache != null)
            {
                _DicCurrentShowUIForms.Add(strUIFormsName, baseUIFormsFromAllCache);
                baseUIFormsFromAllCache.Display();
            }
        }

        /// <summary>
        /// UI窗体入栈
        /// 功能1： 判断栈里是否已经有窗体，有则“冻结”
        ///     2： 先判断“UI预设缓存集合”是否有指定的UI窗体,有则处理。
        ///     3： 指定UI窗体入"栈"
        /// </summary>
        private void PushUIFormToStack(string strUIFormsName)
        {
            UIBase baseUI;                             //UI预设窗体

            //判断栈里是否已经有窗体，有则“冻结”
            if (_StaCurrentUIForms.Count > 0)
            {
                UIBase topUIForms = _StaCurrentUIForms.Peek();
                topUIForms.Freeze();
            }

            //先判断“UI预设缓存集合”是否有指定的UI窗体,有则处理。
            _DicALLUIForms.TryGetValue(strUIFormsName, out baseUI);
            if (baseUI != null)
            {
                //当前窗口显示状态
                baseUI.Display();
                //把指定的UI窗体，入栈操作。
                _StaCurrentUIForms.Push(baseUI);
            }
            else
            {
                Debug.Log("baseUIForm==null,Please Check, 参数 uiFormName=" + strUIFormsName);
            }
        }

        /// <summary>
        /// 退出指定UI窗体
        /// </summary>
        private void ExitUIForms(string strUIFormName)
        {
            UIBase baseUIForm;                          //窗体基类

            //"正在显示集合"中如果没有记录，则直接返回。
            _DicCurrentShowUIForms.TryGetValue(strUIFormName, out baseUIForm);
            if (baseUIForm == null) return;

            //指定窗体，标记为“隐藏状态”，且从"正在显示集合"中移除。
            baseUIForm.Hiding();
            _DicCurrentShowUIForms.Remove(strUIFormName);
        }

        /// <summary>
        /// UI窗体出栈逻辑
        /// </summary>
        private void PopUIForms()
        {
            if (_StaCurrentUIForms.Count >= 2)
            {
                /* 出栈逻辑 */
                UIBase topUIForms = _StaCurrentUIForms.Pop();
                //出栈的窗体，进行隐藏处理
                topUIForms.Hiding();
                //出栈窗体的下一个窗体逻辑
                UIBase nextUIForms = _StaCurrentUIForms.Peek();
                //下一个窗体"重新显示"处理
                nextUIForms.Redisplay();
            }
            else if (_StaCurrentUIForms.Count == 1)
            {
                /* 出栈逻辑 */
                UIBase topUIForms = _StaCurrentUIForms.Pop();
                //出栈的窗体，进行"隐藏"处理
                topUIForms.Hiding();
            }
        }

        /// <summary>
        /// (“隐藏其他”属性)打开窗体，且隐藏其他窗体
        /// </summary>
        private void EnterUIFormsAndHideOther(string strUIName)
        {
            UIBase baseUIForm;                          //UI窗体基类
            UIBase baseUIFormFromALL;                   //从集合中得到的UI窗体基类


            //参数检查
            if (string.IsNullOrEmpty(strUIName)) return;

            _DicCurrentShowUIForms.TryGetValue(strUIName, out baseUIForm);
            if (baseUIForm != null) return;

            //把“正在显示集合”与“栈集合”中所有窗体都隐藏。
            foreach (UIBase baseUI in _DicCurrentShowUIForms.Values)
            {
                baseUI.Hiding();
            }
            foreach (UIBase staUI in _StaCurrentUIForms)
            {
                staUI.Hiding();
            }

            //把当前窗体加入到“正在显示窗体”集合中，且做显示处理。
            _DicALLUIForms.TryGetValue(strUIName, out baseUIFormFromALL);
            if (baseUIFormFromALL != null)
            {
                _DicCurrentShowUIForms.Add(strUIName, baseUIFormFromALL);
                //窗体显示
                baseUIFormFromALL.Display();
            }
        }

        /// <summary>
        /// (“隐藏其他”属性)关闭窗体，且显示其他窗体
        /// </summary>
        private void ExitUIFormsAndDisplayOther(string strUIName)
        {
            UIBase baseUIForm;                          //UI窗体基类

            //参数检查
            if (string.IsNullOrEmpty(strUIName)) return;

            _DicCurrentShowUIForms.TryGetValue(strUIName, out baseUIForm);
            if (baseUIForm == null) return;

            //当前窗体隐藏状态，且“正在显示”集合中，移除本窗体
            baseUIForm.Hiding();
            _DicCurrentShowUIForms.Remove(strUIName);

            //把“正在显示集合”与“栈集合”中所有窗体都定义重新显示状态。
            foreach (UIBase baseUI in _DicCurrentShowUIForms.Values)
            {
                baseUI.Redisplay();
            }
            foreach (UIBase staUI in _StaCurrentUIForms)
            {
                staUI.Redisplay();
            }
        }

        /// <summary>
        /// UI动画
        /// </summary>
        private void EnterUIFormsTween(string uiFormName)
        {
            UIBase baseUIForms;                        //UI窗体基类
            UIBase baseUIFormsFromAllCache;            //"所有窗体集合"中的窗体基类

            //“正在显示UI窗体缓存”集合里有记录，则直接返回。
            _DicCurrentShowUIForms.TryGetValue(uiFormName, out baseUIForms);
            if (baseUIForms != null) return;

            //把当前窗体，加载到“正在显示UI窗体缓存”集合里
            _DicALLUIForms.TryGetValue(uiFormName, out baseUIFormsFromAllCache);
            if (baseUIFormsFromAllCache != null)
            {
                _DicCurrentShowUIForms.Add(uiFormName, baseUIFormsFromAllCache);
                baseUIFormsFromAllCache.Display();
            }
        }

        /// <summary>
        /// 隐藏动画UI
        /// </summary>
        /// <param name="uiFomrName"></param>
        private void ExitUIFormsTween(string uiFomrName)
        {
            UIBase baseUIForm;                          //窗体基类

            //"正在显示集合"中如果没有记录，则直接返回。
            _DicCurrentShowUIForms.TryGetValue(uiFomrName, out baseUIForm);
            if (baseUIForm == null) return;

            //指定窗体，标记为“隐藏状态”，且从"正在显示集合"中移除。
            baseUIForm.Hiding();
            _DicCurrentShowUIForms.Remove(uiFomrName);
        }
        /// <summary>
        /// 是否清空“栈集合”中得数据
        /// </summary>
        private bool ClearStackArray()
        {
            if (_StaCurrentUIForms != null && _StaCurrentUIForms.Count >= 1)
            {
                //清空栈集合
                _StaCurrentUIForms.Clear();
                return true;
            }

            return false;
        }

        #endregion

    }//class_end
}