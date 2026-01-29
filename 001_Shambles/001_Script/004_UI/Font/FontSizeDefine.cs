using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 텍스트 크기 옵션에 따른 폰트 사이즈 수치를 정의하는 클래스
/// </summary>
public class FontSizeDefine
{   
    static readonly FontSize smallFont = new FontSize((int)ENUM_TEXT_SIZE.SMALL);
    static readonly FontSize mediumFont = new FontSize((int)ENUM_TEXT_SIZE.MEDIUM);
    static readonly FontSize largeFont = new FontSize((int)ENUM_TEXT_SIZE.LARGE);

    public static FontSize CurrentFontSize {
        get {
            switch (SettingManager.GetSettingData().textSize) {
                case ENUM_TEXT_SIZE.SMALL:
                    return smallFont;
                case ENUM_TEXT_SIZE.MEDIUM:
                    return mediumFont;
                case ENUM_TEXT_SIZE.LARGE:
                    return largeFont;
                default:
                    return mediumFont;
            }
        }
    }

	/// <summary>
	/// UI 요소별 폰트 사이즈 구조체
	/// </summary>
    public struct FontSize {
        public float storyFontSize;
        public float rewardFontSize;
        public float selectionFontSize;

        public FontSize(float defaultSize) {
            storyFontSize = defaultSize;
            rewardFontSize = defaultSize * 1.3f;
            selectionFontSize = defaultSize * 1.3f;
        }
    } 
}
