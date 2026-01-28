using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CloseAnimationPopup : MonoBehaviour, IPopup {
    [SerializeField] protected GameObject closePopupObject = null;
    [SerializeField] protected Image backgoundButtonObject = null;
    [SerializeField] protected bool isAnimated = true;

    protected Vector3 originalScale; // 팝업 창의 원래 스케일
    protected float originalBackgroundAlpha; // 팝업 창 검은 배경 알파값

    protected bool isAnimationPlaying = false;

	public event Action CloseRequestCalled = null;

    // Start is called before the first frame update
    protected virtual void Start() {
        if (closePopupObject == null) closePopupObject = gameObject;
        originalScale = closePopupObject.transform.localScale;
        if (backgoundButtonObject != null) originalBackgroundAlpha = backgoundButtonObject.color.a;
    }

    public virtual void ClosePopupRequest() {
        if (isAnimated == false) {
            SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.BUTTON_POPUP_CLOSE);
            ClosePopup();
            return;
        }
        if (closePopupObject == null) return;

		if (isAnimationPlaying) return;
		//애니메이션 시작
		closePopupObject.transform.DOScale(0.8f, 0.2f).SetEase(Ease.InBack).SetId("SceneClosingTween")
			.OnStart(() => {
            //애니메이션 중복 방지
            isAnimationPlaying = true;
        }).OnComplete(() => {
            SoundManager.GetInstance().PlayEffectSound(ENUM_EFFECT_SOUND.BUTTON_POPUP_CLOSE);

            //배경 페이드
            if (backgoundButtonObject == null) {
                isAnimationPlaying = false;
                ClosePopup();
            }
            else {
                backgoundButtonObject.DOFade(0, 0.1f).SetId("SceneClosingTween").OnComplete(() => {
                    isAnimationPlaying = false;
                    backgoundButtonObject.SetAlpha(originalBackgroundAlpha);
                    closePopupObject.SetActive(true);
                    ClosePopup();
				});
                closePopupObject.SetActive(false);
            }
            //사이즈 재 설정
            closePopupObject.transform.localScale = originalScale;
		});
		CloseRequestCalled?.Invoke();
	}

    protected virtual void ClosePopup() {
        gameObject.SetActive(false);
    }

    protected virtual void OnEnable() {
        GameManager.GetInstance()?.PushPopup(this);
    }

    protected virtual void OnDisable() {
        GameManager.GetInstance()?.PopPopup();
    }
}
