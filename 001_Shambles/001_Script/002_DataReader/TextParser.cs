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
