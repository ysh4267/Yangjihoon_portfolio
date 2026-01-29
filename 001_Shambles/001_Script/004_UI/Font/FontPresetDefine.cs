using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 언어별 폰트 프리셋 데이터를 정의하고 관리하는 정적 클래스
/// </summary>
public static class FontPresetDefine {
	public static FontPresetData[] FontPreset = new FontPresetData[4] { new FontPresetData(0), new FontPresetData(1), new FontPresetData(2), new FontPresetData(3) };

	// 현재 언어 설정에 해당하는 폰트 프리셋 반환
	public static FontPresetData GetFontPresetData {
		get {
			if ((int)SettingManager.CurrentLanguage >= FontPreset.Length) {
				FontPreset = new FontPresetData[4] { new FontPresetData(0), new FontPresetData(1), new FontPresetData(2), new FontPresetData(3) };
				return FontPreset[FontPreset.Length - 1];
			}
			return FontPreset[(int)SettingManager.CurrentLanguage];
		}
	}
}
