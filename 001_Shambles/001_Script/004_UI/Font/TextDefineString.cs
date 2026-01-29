using System;
using System.Collections.Generic;
using static ENUM_DEFINE_STRING;

/// <summary>
/// 게임 내 사용되는 모든 공통 문자열 리소스를 관리하는 클래스
/// </summary>
public class TextDefineString {
	// 기본 UI 문자열
	public readonly string LevelString;
	public readonly string ExpString;
	public readonly string GoldString;
	public readonly string HpString;
	public readonly string ApString;
	public readonly string StatusString;
	public readonly string ConfirmString;
	public readonly string CancelString;
	public readonly string YesString;
	public readonly string NoString;
	// 기타세부 UI 문자열 생략...

	// 드롭다운이나 탭 헤더 등 동적으로 생성되는 UI에 사용되는 배열형 문자열
	public readonly string[] EquipmentPartNameString;
	public readonly string[] CardTypeString;
	public readonly string[] DifficultyDescriptionStringList;
	// 기타 리스트형 정의 생략...

	// 생성자: 지정된 언어의 데이터를 로드하여 초기화
	public TextDefineString(ENUM_LANGUAGE languageEnum) {
		Dictionary<ENUM_DEFINE_STRING, string> textData = null;
		Dictionary<ENUM_DEFINE_LIST_STRING, string[]> listTextData = null;

		try {
			// 언어 데이터 로드 및 초기화
			// 기본 언어(한국어)를 먼저 로드한 뒤, 선택된 언어 데이터로 덮어쓰는 방식을 사용합니다.
			// 이는 특정 키가 번역본에 누락되었을 경우 기본 언어로 표시되도록 하기 위함입니다.
			var baseLanguage = ENUM_LANGUAGE.ko_KR;
			JsonDataManager.ReadLanguageData<TextDefineStringData>(baseLanguage, out var baseTextData);
			textData = baseTextData.stringData;
			listTextData = baseTextData.listStringData;

			JsonDataManager.ReadLanguageData<TextDefineStringData>(languageEnum, out var overrideTextData);

			// 데이터 병합 (Override)
			foreach (var item in overrideTextData.stringData) {
				textData[item.Key] = item.Value;
			}

			foreach (var item in overrideTextData.listStringData) {
				// 배열 복사 시 인덱스 범위 체크를 통해 안전하게 데이터를 병합합니다.
				for (var i = 0; Math.Min(item.Value.Length, listTextData[item.Key].Length) > i; i++) {
					listTextData[item.Key][i] = item.Value[i];
				}
			}

			foreach (ENUM_DEFINE_STRING key in Enum.GetValues(typeof(ENUM_DEFINE_STRING))) {
				if (!textData.ContainsKey(key)) {
					textData.Add(key, string.Empty);
				}
			}

			foreach (ENUM_DEFINE_LIST_STRING key in Enum.GetValues(typeof(ENUM_DEFINE_LIST_STRING))) {
				if (!listTextData.ContainsKey(key)) {
					listTextData.Add(key, new string[20]);
				}
			}
		}
		catch (Exception ex) {
			textData = new Dictionary<ENUM_DEFINE_STRING, string>();
			listTextData = new Dictionary<ENUM_DEFINE_LIST_STRING, string[]>();
			UnityEngine.Debug.LogError($"Error: There is a problem with the language data.\nDetailes:\n{ex.Message}");
			// ENUM_DEFINE_STRING의 모든 값에 대해 빈 문자열 추가
			foreach (ENUM_DEFINE_STRING key in Enum.GetValues(typeof(ENUM_DEFINE_STRING))) {
				textData.Add(key, string.Empty);
		}

			// ENUM_DEFINE_LIST_STRING의 모든 값에 대해 빈 배열 추가
			foreach (ENUM_DEFINE_LIST_STRING key in Enum.GetValues(typeof(ENUM_DEFINE_LIST_STRING))) {
				listTextData.Add(key, new string[20]);
			}
		}

		// 문자열 할당

		LevelString = textData[ENUM_DEFINE_STRING.Level];
		ExpString = textData[ENUM_DEFINE_STRING.Exp];
		GoldString = textData[ENUM_DEFINE_STRING.Gold];
		HpString = textData[ENUM_DEFINE_STRING.Hp];
		ApString = textData[ENUM_DEFINE_STRING.Ap];
		StatusString = textData[ENUM_DEFINE_STRING.Status];

		ConfirmString = textData[ENUM_DEFINE_STRING.Confirm];
		CancelString = textData[ENUM_DEFINE_STRING.Cancel];
		YesString = textData[ENUM_DEFINE_STRING.Yes];
		NoString = textData[ENUM_DEFINE_STRING.No];


		EquipmentPartNameString = listTextData[ENUM_DEFINE_LIST_STRING.EquipmentPartName];
		CardTypeString = listTextData[ENUM_DEFINE_LIST_STRING.CardType];
		DifficultyDescriptionStringList = listTextData[ENUM_DEFINE_LIST_STRING.DifficultyDescription];

	}

}
