using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FontPresetDefine {
	public static FontPresetData[] FontPreset = new FontPresetData[4] { new FontPresetData(0), new FontPresetData(1), new FontPresetData(2), new FontPresetData(3) };
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
