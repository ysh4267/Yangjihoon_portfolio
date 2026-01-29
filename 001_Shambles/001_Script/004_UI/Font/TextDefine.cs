using System.Collections.Generic;

/// <summary>
/// 전역 UI 정의 및 문자열 데이터 관리 클래스
/// </summary>
public static class TextDefine {
	public static Dictionary<ENUM_LANGUAGE, TextDefineString> DefineString = new Dictionary<ENUM_LANGUAGE, TextDefineString>();

	// 현재 언어 설정에 맞는 문자열 정의 반환
	public static TextDefineString Current {
		get {
			if (!DefineString.TryGetValue(SettingManager.CurrentLanguage, out var defineString)) {
                defineString = new TextDefineString(SettingManager.CurrentLanguage);
                DefineString.Add(SettingManager.CurrentLanguage, defineString);
            }
			return DefineString[SettingManager.CurrentLanguage];
		}
	}
}