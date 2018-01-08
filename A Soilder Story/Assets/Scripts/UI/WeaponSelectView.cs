using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;

public class WeaponSelectView : UIBase {

    public Image weaponLogoImg;
    public Text attackText;
    public Text mingZhongText;
    public Text biShaText;
    public Text missText;
    public GameObject uiContent;
    public Image optionCursor;

	void Awake () {
        CurrentUIType.UIForms_Type = UIFormType.Normal;
        CurrentUIType.UIForms_ShowMode = UIFormShowMode.Normal;
	}
	
}
