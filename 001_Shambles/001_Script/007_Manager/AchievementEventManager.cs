#if PLAY_STORE
using UnityEngine.SocialPlatforms;
#endif

#if STEAMWORKS_NET
using Steamworks;
#endif

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 업적 시스템의 진행도 관리 및 해금 로직을 담당하는 매니저 클래스입니다.
/// Steam, Google Play, App Store 등 다양한 플랫폼의 업적 서비스와 연동하여 상태를 동기화합니다.
/// </summary>
public class AchievementEventManager {

	#region 업적 처리
	/// <summary>
	/// 업적 진행 값 + 1씩 업데이트
	/// </summary>
	/// <param name="achievementTuples"></param>
	public static void IncrementAchievementProgress(params ENUM_ACHIEVEMENT[] achievementTuples) {
		foreach (var achievementTuple in achievementTuples) {
			int achievementIndex = (int)achievementTuple;
			if (!AchievementDao.IsUnlocked(achievementIndex)) {
				AchievementDao.IncrementAchievementProgress(achievementIndex);
				OnAchievementProgressChanged(achievementIndex, (AchievementDao.GetAchievementProgress(achievementIndex) ?? 0) + 1);
			}
			// 업적 진행 값이 최대값을 넘어가면 바로 해금
			if ((AchievementDao.GetAchievementProgress(achievementIndex) ?? 0) >= (AchievementDao.GetAchievement(achievementIndex, true).maxProgressValue ?? 0)) {
				AchievementDao.Unlock(achievementIndex);
				OnAchievementUnlocked(achievementIndex);
			}
		}
	}
	/// <summary>
	/// 현재 진행 값을 progress 값으로 업데이트
	/// </summary>
	/// <param name="progress"></param>
	/// <param name="achievementTuples"></param>
	public static void SetAchievementsProgress(int progress, params ENUM_ACHIEVEMENT[] achievementTuples) {
		foreach (var achievementTuple in achievementTuples) {
			int achievementIndex = (int)achievementTuple;
			if (!AchievementDao.IsUnlocked(achievementIndex)) {
				// 업적 진행 값이 이전 저장된 진행률 보다 클 경우에만 업데이트
				if (progress > (AchievementDao.GetAchievementProgress(achievementIndex) ?? 0)) {
					AchievementDao.SetAchievementProgress(achievementIndex, progress);
					OnAchievementProgressChanged(achievementIndex, progress);
				}
			}
			
			// 업적 진행 값이 최대값을 넘어가면 바로 해금
			if ((AchievementDao.GetAchievementProgress(achievementIndex) ?? 0) >= (AchievementDao.GetAchievement(achievementIndex, true).maxProgressValue ?? 0)) {
				AchievementDao.Unlock(achievementIndex);
				OnAchievementUnlocked(achievementIndex);
			}
		}
	}

	/// <summary>
	/// 업적 해금 업데이트
	/// </summary>
	/// <param name="achievementTuples"></param>
	public static void UnlockAchievements(params ENUM_ACHIEVEMENT[] achievementTuples) {
		foreach (var achievementTuple in achievementTuples) {
			int achievementIndex = (int)achievementTuple;
			
			if (!AchievementDao.IsUnlocked(achievementIndex)) {
				AchievementDao.Unlock(achievementIndex);
				OnAchievementUnlocked(achievementIndex);
			}
		}
	}
	#endregion

	// 업적 진행 변경 이벤트
	public static void OnAchievementProgressChanged(int achievementIndex, int progress) {
#if DEVELOPMENT_BUILD
#elif STEAMWORKS_NET
        if (SteamManager.Initialized) {
            SteamManager.Instance.IncrementAchievementProgress($"{achievementIndex}", progress);
            // steamManager를 사용하여 필요한 작업 수행
        }
#elif STOVE
        StoveEssentialManager.Instance.SetStat($"{achievementIndex}_{AchievementDao.GetAchievement(achievementIndex, true).maxProgressValue}", progress);
#endif
	}

	// 업적 달성 이벤트
	public static void OnAchievementUnlocked(int achievementIndex) {
#if STEAMWORKS_NET
        if (SteamManager.Initialized) {
            SteamManager.Instance.UnlockAchievement($"achievement_{achievementIndex}");
        }
#elif STOVE
        // 스토브에서 달성값이 없는 업적은 스탯 업데이트를 1로 처리합니다.
        if (!AchievementDao.GetAchievement(achievementIndex, true).maxProgressValue.HasValue)
            StoveEssentialManager.Instance.SetStat($"{achievementIndex}_{AchievementDao.GetAchievement(achievementIndex, true).maxProgressValue}", 1);
#elif PLAY_STORE
        // Social.ReportProgress의 예상 동작에 따라 0.0f 값은 업적 공개를 의미하고 100.0f 진행 상황은 업적 달성을 의미합니다.
        Social.ReportProgress(GetAchievementGoogleIDFromIndex(achievementIndex), 100.0f, (bool success) => { });
#elif APP_STORE
		Social.ReportProgress(GetAchievementIDFromIndex(achievementIndex), 100.0f, (success) => { });
#endif
	}

#if PLAY_STORE
    public static Dictionary<int, string> AchievementGoogleIdDictionary = new Dictionary<int, string>
    {
        { 9901, "Example_Google_ID_01" }, // Example Achievement 1
        { 9902, "Example_Google_ID_02" }, // Example Achievement 2
        { 9903, "Example_Google_ID_03" }, // Example Achievement 3
        { 9904, "Example_Google_ID_04" }, // Example Achievement 4
        { 9905, "Example_Google_ID_05" }, // Example Achievement 5
        // ...
    };

    // 업적 인덱스를 입력받아 해당하는 업적 이름을 반환하는 함수
    public static string GetAchievementGoogleIDFromIndex(int number) {
        if (AchievementGoogleIdDictionary.TryGetValue(number, out string result)) {
            return result;
        }
        else {
            return "Example_Google_ID_Fallback";
        }
    }
#elif APP_STORE
	public static Dictionary<int, string> AchievementIdDictionary = new Dictionary<int, string> {
		{ 9901, "shambles.achievement.example.001" }, // Example Achievement 1
        { 9902, "shambles.achievement.example.002" }, // Example Achievement 2
        { 9903, "shambles.achievement.example.003" }, // Example Achievement 3
        { 9904, "shambles.achievement.example.004" }, // Example Achievement 4
        { 9905, "shambles.achievement.example.005" }, // Example Achievement 5
        // ...
    };

	// 업적 인덱스를 입력받아 해당하는 업적 이름을 반환하는 함수
	public static string GetAchievementIDFromIndex(int number) {
		if (AchievementIdDictionary.TryGetValue(number, out string result)) {
			return result;
		}
		else {
			return "shambles.achievement.example.default";
		}
	}
#endif
}
