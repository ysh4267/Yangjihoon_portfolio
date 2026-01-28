using System.Collections.Generic;

public static class CommonDefine {
	public static Dictionary<ENUM_LANGUAGE, CommonDefineString> DefineString = new Dictionary<ENUM_LANGUAGE, CommonDefineString>();

	public static CommonDefineString Current {
		get {
			if (!DefineString.TryGetValue(SettingManager.CurrentLanguage, out var defineString)) {
                defineString = new CommonDefineString(SettingManager.CurrentLanguage);
                DefineString.Add(SettingManager.CurrentLanguage, defineString);
            }
			return DefineString[SettingManager.CurrentLanguage];
		}
	}
}