using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextDivider;

// 문자열 데이터를 특정 규칙에 따라 파싱하여 가공하는 유틸리티 클래스
public static class TextParser {
	// 문자열을 구분자로 분리하여 리스트로 반환
	public static List<string> SplitString(this string rawText) {
		List<string> list = new List<string>();
		if (rawText.IsNullQueryString()) return list;

		string[] result = rawText.Split(textDivider);

		for (int i = 0; i < result.Length; i++) {
			list.Add(result[i]);
		}

		return list;
	}

	// 텍스트 내에서 특정 영역 관련 태그(예: 플레이어 이름)를 실제 값으로 변환
	public static string ParseInsideAreaString(this string rawText) {
		List<string> textList = SplitString(rawText);
		string result = "";

		string playerName = PlayerInfoDao.GetPlayerRawInfo().name;
		foreach (var item in textList) {
			if (item == "#player_name") {
				result += playerName;
			}
			else {
				result += (textDivider + item);
			}
		}

		result = EndParse(result);

		return result;
	}

	// 카드 설명 텍스트에서 특수 태그들을 기본 값으로 파싱
	public static string ParseCardDescriptionText(this string rawText) {
		List<string> textList = SplitString(rawText);
		string result = "";

		foreach (var item in textList) {
			if (item.Contains(damageTag) || item.Contains(shieldTag) || item.Contains(healTag) || item.Contains(recoverApTag) || item.Contains(countTag) || item.Contains(etcTag) || item.Contains(subTextTag)) {
				string[] temp = item.Split(':');
				result += temp[temp.Length - 1];
			}
			else {
				result += (textDivider + item);
			}
		}

		return result;
	}

	// 카드 설명 텍스트를 스탯 기반으로 계산하여 파싱
	public static string ParseStatusBasedCardDescriptionText(this string rawText, int[] status, Card card, bool showChangedAmount = true) {
		List<string> textList = SplitString(rawText);
		string result = "";
		foreach (var item in textList) {
			if (item.Contains(damageTag) || item.Contains(shieldTag) || item.Contains(healTag) || item.Contains(recoverApTag) || item.Contains(countTag) || item.Contains(etcTag) || item.Contains(subTextTag)) {
				string[] temp = item.Split(':');
				int baseValue = int.Parse(temp[temp.Length - 1]);
				int calcValue = Mathf.FloorToInt(StatusCalc(item, card.cardFactionEnum, baseValue));
				result += showChangedAmount ? calcValue.ColoredStringValueWithValue(baseValue, true) : $"{calcValue}";
			}
			else {
				result += (textDivider + item);
			}
		}

		return result;

		float StatusCalc(string item, ENUM_FACTION factionEnum, float value) {
			if (item.Contains(recoverApTag) || item.Contains(countTag) || item.Contains(etcTag)) {
				return value;
			}
			return value * status[(int)GetFactionStatusType(factionEnum)] * 0.1f;
			ENUM_STATUS GetFactionStatusType(ENUM_FACTION faction) {
				switch (faction) {
					case ENUM_FACTION.NORMAL: return ENUM_STATUS.STR;
					case ENUM_FACTION.IMPERIAL: return ENUM_STATUS.STR;
					case ENUM_FACTION.SAN_MAGIKA: return ENUM_STATUS.INT;
					case ENUM_FACTION.MACHINAZE: return ENUM_STATUS.INT;
					case ENUM_FACTION.GREENPIA: return ENUM_STATUS.DEX;
					case ENUM_FACTION.ROA: return ENUM_STATUS.DEX;
					case ENUM_FACTION.CITADEL: return ENUM_STATUS.HP;
					case ENUM_FACTION.ALL: return ENUM_STATUS.STR;
				}
				return ENUM_STATUS.STR;
			}
		}
	}

	// 카드 타입에 따라 아이콘 텍스트 값을 계산하여 파싱
	public static string ParseCardIconText(this string rawText, IBattleCard cardScript) => cardScript.ThisCard.CardType switch {
		ENUM_CARD_TYPE.ATTACK => (cardScript as IBattleCardDamage)?.DamageCalc(int.Parse(rawText)).damage.ToString() ?? rawText,
		ENUM_CARD_TYPE.DEBUFF => (cardScript as IBattleCardBuff)?.CountCalc(int.Parse(rawText)).ToString() ?? rawText,
		ENUM_CARD_TYPE.ACTION_POINT => (cardScript as IBattleCardRecoverAp)?.RecoverApCalc(int.Parse(rawText)).ToString() ?? rawText,
		ENUM_CARD_TYPE.DRAW => rawText,
		ENUM_CARD_TYPE.HEAL => (cardScript as IBattleCardHeal)?.HealCalc(int.Parse(rawText)).ToString() ?? rawText,
		ENUM_CARD_TYPE.SHIELD => (cardScript as IBattleCardShield)?.ShieldCalc(int.Parse(rawText)).ToString() ?? rawText,
		ENUM_CARD_TYPE.BUFF => (cardScript as IBattleCardBuff)?.CountCalc(int.Parse(rawText)).ToString() ?? rawText,
		_ => rawText,
	};

	// 배틀 카드 설명 텍스트를 인터페이스 기반으로 계산하여 파싱
	public static string ParseBattleCardDescriptionText(this string rawText, IBattleCard cardScript, bool showChangedAmount = true) {
		string result = string.Empty;
		List<string> textList = SplitString(rawText);
		foreach (var item in textList) {
			try {
				if (item.Contains(damageTag)) {
					if (!(cardScript is IBattleCardDamage)) throw new System.Exception("no Damage interface");
					string[] temp = item.Split(':');
					int baseValue = int.Parse(temp[temp.Length - 1]);
					int calcValue = (cardScript as IBattleCardDamage).DamageCalc(baseValue).damage;
					result += showChangedAmount ? calcValue.ColoredStringValueWithValue(baseValue) : $"{calcValue}";
				}
				else if (item.Contains(shieldTag)) {
					if (!(cardScript is IBattleCardShield)) throw new System.Exception("no Shield interface");
					string[] temp = item.Split(':');
					int baseValue = int.Parse(temp[temp.Length - 1]);
					int calcValue = (cardScript as IBattleCardShield).ShieldCalc(baseValue);
					result += showChangedAmount ? calcValue.ColoredStringValueWithValue(baseValue) : $"{calcValue}";
				}
				else if (item.Contains(healTag)) {
					if (!(cardScript is IBattleCardHeal)) throw new System.Exception("no Heal interface");
					string[] temp = item.Split(':');
					int baseValue = int.Parse(temp[temp.Length - 1]);
					int calcValue = (cardScript as IBattleCardHeal).HealCalc(baseValue);
					result += showChangedAmount ? calcValue.ColoredStringValueWithValue(baseValue) : $"{calcValue}";
				}
				else if (item.Contains(recoverApTag)) {
					if (!(cardScript is IBattleCardRecoverAp)) throw new System.Exception("no RecoverAp interface");
					string[] temp = item.Split(':');
					int baseValue = int.Parse(temp[temp.Length - 1]);
					int calcValue = (cardScript as IBattleCardRecoverAp).RecoverApCalc(baseValue);
					result += showChangedAmount ? calcValue.ColoredStringValueWithValue(baseValue) : $"{calcValue}";
				}
				else if (item.Contains(countTag)) {
					if (!(cardScript is IBattleCardBuff)) throw new System.Exception("no Buff interface");
					string[] temp = item.Split(':');
					int baseValue = int.Parse(temp[temp.Length - 1]);
					int calcValue = (cardScript as IBattleCardBuff).CountCalc(baseValue);
					result += showChangedAmount ? calcValue.ColoredStringValueWithValue(baseValue) : $"{calcValue}";
				}
				else if (item.Contains(etcTag)) {
					string[] temp = item.Split(':');
					int baseValue = int.Parse(temp[temp.Length - 1]);
					result += baseValue;
				}
				else if (item.Contains(subTextTag)) {
					string[] temp = item.Split(':');
					string subText = temp[temp.Length - 1];
					result += subText;
				}
				else {
					result += item;
				}
			}
			catch (System.Exception e) {
				Debug.LogError(e.Message);
				Debug.Log(cardScript.ThisCard.Index);
				string[] temp = item.Split(':');
				int baseValue = 0;
				int.TryParse(temp[temp.Length - 1], out baseValue);
				result += baseValue;
			}
		}
		return result;
	}

	// 배틀 카드 설명에서 수치 값들을 추출하여 구조체로 반환
	public static BattleCardValues ParseBattleCardDescriptionValues(this string rawText) {
		List<string> textList = SplitString(rawText);
		BattleCardValues result = new BattleCardValues();
		// main인 경우는 태그 생략 가능 ex) *#damage:5* 는 *#damage:main:5* 와 일치
		foreach (var item in textList) {
			try {
				if (item.Contains(damageTag)) {
					string[] parts = item.Split(':');
					if (parts.Length < 3) {
						if (int.TryParse(parts[1], out int value))
							result.damageValues[0] = value;
					}
					else {
						if (int.TryParse(parts[2], out int value))
							result.damageValues[ParseValueOrderText(parts[1])] = value;
					}
				}
				else if (item.Contains(shieldTag)) {
					string[] parts = item.Split(':');
					if (parts.Length < 3) {
						if (int.TryParse(parts[1], out int value))
							result.shieldValues[0] = value;
					}
					else {
						if (int.TryParse(parts[2], out int value))
							result.shieldValues[ParseValueOrderText(parts[1])] = value;
					}
				}
				else if (item.Contains(healTag)) {
					string[] parts = item.Split(':');
					if (parts.Length < 3) {
						if (int.TryParse(parts[1], out int value))
							result.healValues[0] = value;
					}
					else {
						if (int.TryParse(parts[2], out int value))
							result.healValues[ParseValueOrderText(parts[1])] = value;
					}
				}
				else if (item.Contains(recoverApTag)) {
					string[] parts = item.Split(':');
					if (parts.Length < 3) {
						if (int.TryParse(parts[1], out int value))
							result.recoverApValues[0] = value;
					}
					else {
						if (int.TryParse(parts[2], out int value))
							result.recoverApValues[ParseValueOrderText(parts[1])] = value;
					}
				}
				else if (item.Contains(countTag)) {
					string[] parts = item.Split(':');
					if (parts.Length < 3) {
						if (int.TryParse(parts[1], out int value))
							result.countValues[0] = value;
					}
					else {
						if (int.TryParse(parts[2], out int value))
							result.countValues[ParseValueOrderText(parts[1])] = value;
					}
				}
				else if (item.Contains(etcTag)) {
					string[] parts = item.Split(':');
					if (parts.Length < 3) {
						if (int.TryParse(parts[1], out int value))
							result.etcValues[0] = value;
					}
					else {
						if (int.TryParse(parts[2], out int value))
							result.etcValues[ParseValueOrderText(parts[1])] = value;
					}
				}
			}
			catch (System.Exception e) {
				Debug.LogException(e);
			}
		}

		result.EnsureMinimumArrayLength();
		return result;

		int ParseValueOrderText(string text) {
			if (text.Contains("main"))
				return 0;
			else if (text.Contains("sub1")) {
				return 1;
			}
			else if (text.Contains("sub2")) {
				return 2;
			}
			else if (text.Contains("sub3")) {
				return 3;
			}
			else if (text.Contains("sub4")) {
				return 4;
			}

			return default;
		}
	}

	// 텍스트에서 서브 텍스트 태그의 내용을 추출
	public static string GetBattleCardDescriptionSubText(this string rawText) {
		List<string> textList = SplitString(rawText);
		foreach (var item in textList) {
			if (item.Contains(subTextTag)) {
				return item.Split(':')[1];
			}
		}

		return string.Empty;
	}

	// 파싱된 텍스트 리스트를 결합하고 특수 문자를 치환하여 최종 문자열 반환
	public static string EndParse(this string rawText) {
		List<string> textList = SplitString(rawText);
		string result = "";

		foreach (var item in textList) {
			result += item;
		}
		result = result.Replace("“", "\"");
		result = result.Replace("”", "\"");
		result = result.Replace("…", "...");
		return result;
	}
}
