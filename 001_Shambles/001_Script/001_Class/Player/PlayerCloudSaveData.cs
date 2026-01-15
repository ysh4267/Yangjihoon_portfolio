using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

[Serializable]
public class PlayerCloudSaveData {

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ENUM_UNLOCKED_CONTENTS {
		Card,
		Character,
		Ending,
		Journal
	}

	public int achievementPoint;
	public Dictionary<ENUM_UNLOCKED_CONTENTS, Dictionary<int, (bool isUnlocked, bool isUnchecked)>> collectionContentDataList;
	public Dictionary<ENUM_UNLOCKED_CONTENTS_TYPE, List<int>> purchasedAchievementShopItemList;
	public Dictionary<ENUM_ASSIST_ABILITY, int> purchasedAssistAbility;
	public Dictionary<int, (bool isUnlocked, bool isChecked, int? progressValue)> achievementDataList;
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
		this.achievementDataList = AchievementDao.GetUserAchievementList().ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.isUnlocked, kvp.Value.isChecked, kvp.Value.progressValue));

		savedTime = DateTime.Now;
		return this;
	}

	public string GetPlayerDataJson() {
		return JsonConvert.SerializeObject(this);
	}
}
