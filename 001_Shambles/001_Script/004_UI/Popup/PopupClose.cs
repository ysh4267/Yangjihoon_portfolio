using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 이벤트를 포함한 닫기 기능을 구현한 팝업 클래스
/// </summary>
public class PopupClose : CloseAnimationPopup
{
    [SerializeField] protected Button[] closeButtons = null;

	// 팝업 활성화 시 배틀 매니저의 UI 인터랙션을 차단
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

	// 팝업 비활성화 시 배틀 매니저의 UI 인터랙션을 복구
    protected override void OnDisable() {
        if (BattleManager.GetInstance() != null) {
            BattleManager.GetInstance().SetUIInteraction(true);
        }
        GameManager.GetInstance()?.PopPopup();
    }
}
