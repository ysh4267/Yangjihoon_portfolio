using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

// 현재 게임의 진행상황을 UnnityCloud 를 이용한 클라우드 저장소에 저장하기 위한 데이터 클래스
[Serializable]
public class PlayerCloudSaveData {

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ENUM_UNLOCKED_CONTENTS {
		Card,
		Character,
		Ending,
		Journal
	}

	// 현재 보유 포인트
	public int achievementPoint;
	// 현재 해금 도감 상태 (해금여부 뿐만아니라 확인여부도 저장함)
	public Dictionary<ENUM_UNLOCKED_CONTENTS, Dictionary<int, (bool isUnlocked, bool isUnchecked)>> collectionContentDataList;
	// 상점 구매 목록
	public Dictionary<ENUM_UNLOCKED_CONTENTS_TYPE, List<int>> purchasedAchievementShopItemList;
	// 업그레이드 목록
	public Dictionary<ENUM_ASSIST_ABILITY, int> purchasedAssistAbility;
	// 업적 진행도 (플랫폼 업적 시스템의 갱신은 이루어지지 않으므로 게임 내 업적 진행도만 불러옴)
	public Dictionary<int, (bool isUnlocked, bool isChecked, int? progressValue)> achievementDataList;
	// 저장 시점
	public DateTime savedTime;

	public PlayerCloudSaveData GetPlayerData() {
		var userInfo = UserInfoDao.GetUserInfo();
		this.achievementPoint = userInfo.achievementPoint;
		this.collectionContentDataList = new Dictionary<ENUM_UNLOCKED_CONTENTS, Dictionary<int, (bool isUnlocked, bool isChecked)>>();

		var allContentData = new Dictionary<ENUM_UNLOCKED_CONTENTS, (List<int> Unlocked, List<int> Unchecked)>();
		allContentData[ENUM_UNLOCKED_CONTENTS.Card] = (CollectionDao.GetUnlockedIndexList<Card>(), CollectionDao.GetUncheckedIndexList<Card>());
		allContentData[ENUM_UNLOCKED_CONTENTS.Character] = (CollectionDao.GetUnlockedIndexList<Character>(), CollectionDao.GetUncheckedIndexList<Character>());
		allContentData[ENUM_UNLOCKED_CONTENTS.Ending] = (CollectionDao.GetUnlockedIndexList<Ending>(), CollectionDao.GetUncheckedIndexList<Ending>());
		allContentData[ENUM_UNLOCKED_CONTENTS.Journal] = (CollectionDao.GetUnlockedIndexList<Journal>(), CollectionDao.GetUncheckedIndexList<Journal>());

		// 전체 항목에 대해 해금되었거나 확인이 된 항목만을 모아 재 정렬함 (데이터 용량 절약을 위해 해금이나 확인이 되지 않은 항목은 리스트에서 제외함)
		foreach (var item in allContentData.Keys) {
			var unlockedSet = new HashSet<int>(allContentData[item].Unlocked);
			var uncheckedSet = new HashSet<int>(allContentData[item].Unchecked);

			Dictionary<int, (bool isUnlocked, bool isChecked)> unlockedData =
				unlockedSet.Union(uncheckedSet)
					.ToDictionary(
						x => x,
						x => (unlockedSet.Contains(x), uncheckedSet.Contains(x))
					);

			this.collectionContentDataList.Add(item, unlockedData);
		}

		// 모든 항목을 불러온 뒤 Linq문을 이용해 구매항목만 Select한 뒤 오름차순 정렬함 
		this.purchasedAchievementShopItemList = new Dictionary<ENUM_UNLOCKED_CONTENTS_TYPE, List<int>> {
			{ ENUM_UNLOCKED_CONTENTS_TYPE.STARTER_PACK, AchievementShopDao.GetAchievementShopItemList<StarterPack>(IRenderableData.ItemType.starterPack).Where(value => value.is_purchased).Select(value => value.Index).OrderByDescending(index => index).ToList() },
			{ ENUM_UNLOCKED_CONTENTS_TYPE.PORTRAIT, AchievementShopDao.GetAchievementShopItemList<Portrait>(IRenderableData.ItemType.portrait).Where(value => value.is_purchased).Select(value => value.Index).OrderByDescending(index => index).ToList() },
			{ ENUM_UNLOCKED_CONTENTS_TYPE.SKILL, AchievementShopDao.GetAchievementShopItemList<AchievementSkillSet>(IRenderableData.ItemType.skill).Where(value => value.is_purchased).Select(value => value.Index).OrderByDescending(index => index).ToList() },
			{ ENUM_UNLOCKED_CONTENTS_TYPE.EQUIPMENT, AchievementShopDao.GetAchievementShopItemList<AchievementEquipSet>(IRenderableData.ItemType.equipment).Where(value => value.is_purchased).Select(value => value.Index).OrderByDescending(index => index).ToList() }
		};
		this.purchasedAssistAbility = new Dictionary<ENUM_ASSIST_ABILITY, int> {
			{ ENUM_ASSIST_ABILITY.gold_boost, AchievementShopDao.GetAbilityPurchaseCount(ENUM_ASSIST_ABILITY.gold_boost) },
			{ ENUM_ASSIST_ABILITY.exp_boost, AchievementShopDao.GetAbilityPurchaseCount(ENUM_ASSIST_ABILITY.exp_boost) },
			{ ENUM_ASSIST_ABILITY.skill_point, AchievementShopDao.GetAbilityPurchaseCount(ENUM_ASSIST_ABILITY.skill_point) },
			{ ENUM_ASSIST_ABILITY.encounter_reroll_ticket, AchievementShopDao.GetAbilityPurchaseCount(ENUM_ASSIST_ABILITY.encounter_reroll_ticket) }
		};
		// 업적 데이터에서 클라우드 저장에 필요한 데이터만을 따로 클래스화 하여 다시 저장
		this.achievementDataList = AchievementDao.GetUserAchievementList().ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.isUnlocked, kvp.Value.isChecked, kvp.Value.progressValue));

		savedTime = DateTime.Now;
		return this;
	}

	public string GetPlayerDataJson() {
		return JsonConvert.SerializeObject(this);
	}
}
