using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupClose : CloseAnimationPopup
{
    [SerializeField] protected Button[] closeButtons = null;

    protected override void OnEnable() {
        if (BattleManager.GetInstance() != null) {
            BattleManager.GetInstance().SetUIInteraction(false);
        }
        GameManager.GetInstance().PushPopup(this);
    }

    protected override void Start() {
        base.Start();
        foreach(Button button in closeButtons) {
            button.onClick.AddListener(ClosePopupRequest);
        }
    }

    protected override void OnDisable() {
        if (BattleManager.GetInstance() != null) {
            BattleManager.GetInstance().SetUIInteraction(true);
        }
        GameManager.GetInstance()?.PopPopup();
    }
}
