using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace DataParser
{
	class DataConverter
	{
		Action<string> DebugMessage;
		Action<int> UpdateProgressBar;

		Dictionary<ENUM_LANGUAGE, string> languageColumnQueryString = new Dictionary<ENUM_LANGUAGE, string>() {
				{ ENUM_LANGUAGE.ko_KR, "ko_kr" },
				{ ENUM_LANGUAGE.en_US, "en_us" },
				{ ENUM_LANGUAGE.ja_JP, "ja_jp" },
				{ ENUM_LANGUAGE.th_TH, "th_th" },
				{ ENUM_LANGUAGE.zh_CN, "zh_cn" },
				{ ENUM_LANGUAGE.zh_TW, "zh_tw" }
		};

		public DataConverter(Action<string> _debugMessage, Action<int> _updateProgressBar)
		{
			DebugMessage = _debugMessage;
			UpdateProgressBar = _updateProgressBar;
		}

		public async Task<bool> ConvertEventDataExcelToSQLite(CancellationToken cancellationToken, string dbFilePathString, string excelFilePathString, ENUM_LANGUAGE currnetLanguage)
		{
			bool? result = null;
			DataReader dataReader = new DataReader(DebugMessage, UpdateProgressBar);

			using (SQLiteConnection sqliteConnection = new SQLiteConnection($"Data Source={dbFilePathString};"))
			{
				Excel.Application excelApp = null;
				Workbook workbook = null;
				List<System.Data.DataTable> dataTable = new List<System.Data.DataTable>();
				try
				{
					excelApp = new Excel.Application();
					excelApp.ScreenUpdating = false;
					workbook = excelApp.Workbooks.Open(excelFilePathString, ReadOnly: true);
					dataTable = dataReader.ReadAllDataFromExcel(excelApp, workbook, excelFilePathString);
				}
				catch (Exception ex)
				{
					DebugMessage(ex.Message);
					result = false;
					DebugMessage("\r\n변환 실패.\r\n");
				}
				finally
				{
					if (workbook != null)
					{
						workbook.Close(false);
						Marshal.ReleaseComObject(workbook);
					}
					if (excelApp != null)
					{
						excelApp.Quit();
						Marshal.ReleaseComObject(excelApp);
					}
				}

				try
				{
					await sqliteConnection.OpenAsync(cancellationToken);
					// Repeat for the count of sheets asynchronously
					int sheetsCount = dataTable.Count;
					var tasks = new List<Task<bool>>(sheetsCount - 2); // Task 목록 생성
					int completeCount = 1;
					for (int sheetsIndex = 3; sheetsIndex <= sheetsCount; sheetsIndex++)
					{
						int __sheetsIndex = sheetsIndex;
						UpdateProgressBar(0);
						tasks.Add(Task.Run(() =>
						{
							//Data
							var _dataTable = dataTable;
							UpdateProgressBar((int)Math.Ceiling(((float)(completeCount++) / (float)(sheetsCount - 2)) * 100f));
							var processResult = ProcessEventDataSheet(__sheetsIndex, dataReader, sqliteConnection, _dataTable, currnetLanguage, cancellationToken);
							return processResult;
						})); // Task 추가
					}

					// 모든 Task가 완료될 때까지 기다림
					var taskResults = await Task.WhenAll(tasks);

					if (result == null)
					{
						result = taskResults.All(r => r); // 모든 Task가 성공했는지 확인
					}

					// 결과 확인 및 통합

					if (result == true)
					{
						DebugMessage("\r\n변환 성공.\r\n");
					}
					else
					{
						DebugMessage("\r\n변환 실패.\r\n");
					}

				}
				catch (Exception ex)
				{
					DebugMessage(ex.Message);
					result = false;
					DebugMessage("\r\n변환 실패.\r\n");
				}
				finally
				{
					sqliteConnection.Close();
				}
			}

			return result.Value;
		}

		private bool ProcessEventDataSheet(int sheetsIndex, DataReader dataReader, SQLiteConnection sqliteConnection, List<System.Data.DataTable> dataTable, ENUM_LANGUAGE currnetLanguage, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				DebugMessage($"\r\n작업이 강제로 종료 되었습니다.\r\n");

				return false;
			}
			bool result = true;
			//DBQueryData
			Dictionary<string, (Type, object)> dataDictionary = new Dictionary<string, (Type, object)>();

			//Repeat for the count of columns
			int dataCycleSize = 4;
			int dataCycleCount = ((dataReader.GetColumnsCount(dataTable[sheetsIndex - 1]) + 1) / dataCycleSize);
			if (dataCycleCount > 0)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					DebugMessage($"\r\n작업이 강제로 종료 되었습니다.\r\n");

					return false;
				}
				object eventIndexObjectData;
				if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, 4, 1, out eventIndexObjectData) == false) return false;
				if (eventIndexObjectData == null) return false;
				int eventIndex = ParseInt(eventIndexObjectData);
				dataDictionary.Clear();
				//Story Event
				QuickLoadDataNoCycle(6, 1, QueryColumnNameStrings.InitialStorySentenceIndexColumn, typeof(int));
				QuickLoadDataNoCycle(8, 1, QueryColumnNameStrings.IsInitialColumn, typeof(string));
				QuickLoadDataNoCycle(10, 1, QueryColumnNameStrings.WeightColumn, typeof(int));
				QuickLoadDataNoCycle(18, 2, QueryColumnNameStrings.EventTypeColumn, typeof(string));
				QuickLoadDataNoCycle(12, 2, QueryColumnNameStrings.BgmSoundIndexColumn, typeof(int));
				QuickLoadDataNoCycle(2, 2, QueryColumnNameStrings.PropertyColumn, typeof(string));
				QuickLoadDataNoCycle(28, 2, QueryColumnNameStrings.ProductIDColumn, typeof(string));

				//Reputation
				List<int> reputationPresetIndexList = new List<int>();
				for (int i = 0; i < 3; i++)
				{
					var index = i;
					if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, 12 + (index * 3), 1, out object reputationIndexObjectData) == true)
					{
						if (reputationIndexObjectData != null)
						{
							Dictionary<string, (Type, object)> reputationDataDictionary = new Dictionary<string, (Type, object)>();
							QuickLoadDataNoCycleWithDic(12 + (index * 6), 1, QueryColumnNameStrings.ReputationIndexColumn, typeof(int), reputationDataDictionary);
							QuickLoadDataNoCycleWithDic(14 + (index * 6), 1, QueryColumnNameStrings.ReputationMinColumn, typeof(int), reputationDataDictionary);
							QuickLoadDataNoCycleWithDic(16 + (index * 6), 1, QueryColumnNameStrings.ReputationMaxColumn, typeof(int), reputationDataDictionary);
							if (reputationDataDictionary.Count > 0)
							{
								int reputationPresetIndex;
								//UpdateData
								dataReader.InsertData(sqliteConnection, QueryTableNameStrings.ReputationRequirementPresetTable, QueryColumnNameStrings.ReputationRequirementPresetIndexColumn, reputationDataDictionary, out reputationPresetIndex);
								reputationPresetIndexList.Add(reputationPresetIndex);
								reputationDataDictionary.Clear();
							}
						}
					}
				}
				if (reputationPresetIndexList.Count > 0)
				{
					string reputationDataQueryString = ParseIntListToQueryString(reputationPresetIndexList);
					dataDictionary.Add(QueryColumnNameStrings.ReputationRequirementPresetIndexListColumn, (typeof(string), reputationDataQueryString));
				}

				//UpdateData
				if (dataDictionary.Count > 0)
				{
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StoryEventTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, dataDictionary);
					dataDictionary.Clear();
				}

				//Story Event Title Text
				QuickLoadDataNoCycle(2, 1, languageColumnQueryString[currnetLanguage], typeof(string));
				if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, 2, 1, out object _tempData))
				{
					DebugMessage($"이벤트 번호 : {eventIndex} / 이벤트 제목 : {_tempData}\r\n 작업시작.");
				}

				//UpdateData
				if (dataDictionary.Count > 0)
				{
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StoryEventTitleTextTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, dataDictionary);
					dataDictionary.Clear();
				}

				//Story Event Hint Title
				QuickLoadDataNoCycle(6, 2, languageColumnQueryString[currnetLanguage], typeof(string));

				//UpdateData
				if (dataDictionary.Count > 0)
				{
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StoryEventHintTitleTextTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, dataDictionary);
					dataDictionary.Clear();

					//Story Event Hint
					QuickLoadDataNoCycle(14, 2, QueryColumnNameStrings.TargetAreaIndexListColumn, typeof(string));

					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StoryEventHintTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, dataDictionary);
					dataDictionary.Clear();
				}
				//Story Event Hint Desc
				QuickLoadDataNoCycle(8, 2, languageColumnQueryString[currnetLanguage], typeof(string));

				//UpdateData
				if (dataDictionary.Count > 0)
				{
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StoryEventHintDescriptionTextTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, dataDictionary);
					dataDictionary.Clear();
				}

				//Story Event Encounter
				QuickLoadDataNoCycle(16, 2, QueryColumnNameStrings.IsExclusiveColumn, typeof(string));
				QuickLoadDataNoCycle(20, 2, QueryColumnNameStrings.FactionEnumColumn, typeof(string));
				QuickLoadDataNoCycle(26, 2, QueryColumnNameStrings.IsRestColumn, typeof(string));
				QuickLoadDataNoCycle(24, 2, QueryColumnNameStrings.IsShopColumn, typeof(string));
				QuickLoadDataNoCycle(22, 2, QueryColumnNameStrings.IsDangerColumn, typeof(string));
				QuickLoadDataNoCycle(30, 2, QueryColumnNameStrings.NeedEnergyColumn, typeof(int));

				//UpdateData
				if (dataDictionary.Count > 0)
				{
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StoryEventEncounterTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, dataDictionary);
					dataDictionary.Clear();
				}
			}

			for (int dataCycleIndex = 0; dataCycleIndex < dataCycleCount; dataCycleIndex++)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					DebugMessage($"\r\n작업이 강제로 종료 되었습니다.\r\n");

					return false;
				}
				dataDictionary.Clear();

				//MainSentenceIndex
				object sentenceIndexObjectData;
				if (QuickReadDataFromExcel(1, 4, out sentenceIndexObjectData) == false) continue;
				if (sentenceIndexObjectData == null) continue;
				int sentenceIndex = ParseInt(sentenceIndexObjectData);
				#region Sentence Table
				//Illust data
				if (QuickLoadData(2, 5, QueryColumnNameStrings.IllustIndexColumn, typeof(int)) == false) continue;

				//Prefab data
				if (QuickLoadData(2, 6, QueryColumnNameStrings.PrefabIndexColumn, typeof(int)) == false) continue;

				//Sound data
				if (QuickLoadData(2, 7, QueryColumnNameStrings.SoundIndexColumn, typeof(int)) == false) continue;

				//Selection index data
				List<int> selectionIndexList = new List<int>();
				object selectionIndexListObjectData;
				if (QuickReadDataFromExcel(2, 8, out selectionIndexListObjectData) == false) continue;
				if (selectionIndexListObjectData != null)
				{
					selectionIndexList.Add(ParseInt(selectionIndexListObjectData));
					selectionIndexListObjectData = null;
				}
				if (QuickReadDataFromExcel(3, 8, out selectionIndexListObjectData) == false) continue;
				if (selectionIndexListObjectData != null)
				{
					selectionIndexList.Add(ParseInt(selectionIndexListObjectData));
					selectionIndexListObjectData = null;
				}
				if (QuickReadDataFromExcel(4, 8, out selectionIndexListObjectData) == false) continue;
				if (selectionIndexListObjectData != null)
				{
					selectionIndexList.Add(ParseInt(selectionIndexListObjectData));
					selectionIndexListObjectData = null;
				}
				if (selectionIndexList.Count > 0)
				{
					dataDictionary.Add(QueryColumnNameStrings.StorySelectionIndexListColumn, (typeof(string), ParseIntListToQueryString(selectionIndexList)));
				}

				//UpdateData
				if (dataDictionary.Count > 0)
				{
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySentenceTable, QueryColumnNameStrings.StorySentenceIndexColumn, sentenceIndex, dataDictionary);
					dataDictionary.Clear();
				}
				else
				{
					dataDictionary.Add(QueryColumnNameStrings.StorySentenceIndexColumn, (typeof(int), sentenceIndex));
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySentenceTable, QueryColumnNameStrings.StorySentenceIndexColumn, sentenceIndex, dataDictionary);
					dataDictionary.Clear();
				}

				string sentenceText;
				if (QuickReadDataFromExcel(2, 4, out sentenceText) == false) continue;
				DebugMessage($"문장 번호 : {sentenceIndex} / 문장 : {sentenceText}\r\n");
				sentenceText = ParseStringForQuery(sentenceText);
				dataDictionary.Add(languageColumnQueryString[currnetLanguage], (typeof(string), sentenceText));
				dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySentenceTextTable, QueryColumnNameStrings.StorySentenceIndexColumn, sentenceIndex, dataDictionary);
				dataDictionary.Clear();
				#endregion

				#region Selection Section

				int maxSelectionCount = 3;
				for (int selectionCountIndex = 0; selectionCountIndex < maxSelectionCount; selectionCountIndex++)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						DebugMessage($"\r\n작업이 강제로 종료 되었습니다.\r\n");

						return false;
					}
					//Selection Index
					object selectionIndexObjectdata;
					if (QuickReadDataFromExcel(2 + (selectionCountIndex), 8, out selectionIndexObjectdata) == false) continue;
					if (selectionIndexObjectdata == null) continue;
					int _selectionIndex = ParseInt(selectionIndexObjectdata);

					//Selection Text
					string selectionText;
					if (QuickReadDataFromExcel(2 + (selectionCountIndex), 9, out selectionText) == false) continue;
					selectionText = ParseStringForQuery(selectionText);
					dataDictionary.Add(languageColumnQueryString[currnetLanguage], (typeof(string), selectionText));
					dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySelectionTextTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, dataDictionary);
					dataDictionary.Clear();

					//Selection NextSentenceIndex
					if (QuickLoadData(2 + (selectionCountIndex), 10, QueryColumnNameStrings.NextStorySentenceIndexListColumn, typeof(string)) == false) continue;

					//Selection BattleIndex
					if (QuickLoadData(2 + (selectionCountIndex), 14, QueryColumnNameStrings.BattleIndexColumn, typeof(int)) == false) continue;

					//Next event
					if (QuickLoadData(2 + (selectionCountIndex), 50, QueryColumnNameStrings.NextStoryEventColumn, typeof(string)) == false) continue;

					//Minigame
					if (QuickLoadData(2 + (selectionCountIndex), 51, QueryColumnNameStrings.MinigamePrefabIndexColumn, typeof(int)) == false) continue;

					//Teleport
					if (QuickLoadData(2 + (selectionCountIndex), 54, QueryColumnNameStrings.TeleportAreaIndexColumn, typeof(int)) == false) continue;

					//Dead ending
					if (QuickLoadData(2 + (selectionCountIndex), 52, QueryColumnNameStrings.DeadEndingIndexColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 64, QueryColumnNameStrings.DeadEndingSentenceIndex, typeof(int)) == false) continue;

					//Add ending list
					if (QuickLoadData(2 + (selectionCountIndex), 53, QueryColumnNameStrings.AddEndingIndexListColumn, typeof(string)) == false) continue;

					//Shop
					if (QuickLoadData(2 + (selectionCountIndex), 60, QueryColumnNameStrings.StoryShopIndexColumn, typeof(int)) == false) continue;

					//reveal area index
					if (QuickLoadData(2 + (selectionCountIndex), 63, QueryColumnNameStrings.RevealAreaIndex, typeof(int)) == false) continue;

					//map index
					if (QuickLoadData(2 + (selectionCountIndex), 65, QueryColumnNameStrings.MapIndexColumn, typeof(int)) == false) continue;

					//Delete Next Story Event
					if (QuickLoadData(2 + (selectionCountIndex), 66, QueryColumnNameStrings.DeleteNextStoryEventColumn, typeof(string)) == false) continue;

					//End Game
					if (QuickLoadData(2 + (selectionCountIndex), 67, QueryColumnNameStrings.EndGameColumn, typeof(string)) == false) continue;
					//UpdateData
					if (dataDictionary.Count > 0)
					{
						dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySelectionTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, dataDictionary);
						dataDictionary.Clear();
					}
					else
					{
						dataDictionary.Add(QueryColumnNameStrings.StorySelectionIndexColumn, (typeof(int), _selectionIndex));
						dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySelectionTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, dataDictionary);
						dataDictionary.Clear();
					}

					//Shop Selection Table
					object selectionShopIndexObjectData;
					if (QuickReadDataFromExcel(2 + (selectionCountIndex), 60, out selectionShopIndexObjectData) == false) continue;
					if (selectionShopIndexObjectData != null)
					{
						if (QuickLoadData(2 + (selectionCountIndex), 61, QueryColumnNameStrings.StoryShopBuySelectionIndexColumn, typeof(int)) == false) continue;
						if (QuickLoadData(2 + (selectionCountIndex), 62, QueryColumnNameStrings.StoryShopNonBuySelectionIndexColumn, typeof(int)) == false) continue;
					}

					//UpdateData
					if (dataDictionary.Count > 0)
					{
						dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StoryShopSelectionTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, dataDictionary);
						dataDictionary.Clear();
					}

					//Selection Requirement Table

					//Requirement text
					string requirementText;
					if (QuickReadDataFromExcel(2 + (selectionCountIndex), 13, out requirementText) == false) continue;
					if (requirementText != null)
					{
						requirementText = ParseStringForQuery(requirementText);
						dataDictionary.Add(languageColumnQueryString[currnetLanguage], (typeof(string), requirementText));
						dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySelectionRequirementTextTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, dataDictionary);
						dataDictionary.Clear();
					}

					//currentHp
					if (QuickLoadData(2 + (selectionCountIndex), 29, QueryColumnNameStrings.StorySelectionRequireCurrentHpMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 30, QueryColumnNameStrings.StorySelectionRequireCurrentHpMaxColumn, typeof(int)) == false) continue;

					//AP
					if (QuickLoadData(2 + (selectionCountIndex), 31, QueryColumnNameStrings.StorySelectionRequireApMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 32, QueryColumnNameStrings.StorySelectionRequireApMaxColumn, typeof(int)) == false) continue;

					//hp
					if (QuickLoadData(2 + (selectionCountIndex), 33, QueryColumnNameStrings.StorySelectionRequireHpMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 34, QueryColumnNameStrings.StorySelectionRequireHpMaxColumn, typeof(int)) == false) continue;

					//str
					if (QuickLoadData(2 + (selectionCountIndex), 35, QueryColumnNameStrings.StorySelectionRequireStrMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 36, QueryColumnNameStrings.StorySelectionRequireStrMaxColumn, typeof(int)) == false) continue;

					//dex
					if (QuickLoadData(2 + (selectionCountIndex), 37, QueryColumnNameStrings.StorySelectionRequireDexMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 38, QueryColumnNameStrings.StorySelectionRequireDexMaxColumn, typeof(int)) == false) continue;

					//int
					if (QuickLoadData(2 + (selectionCountIndex), 39, QueryColumnNameStrings.StorySelectionRequireIntMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 40, QueryColumnNameStrings.StorySelectionRequireIntMaxColumn, typeof(int)) == false) continue;

					//gold
					if (QuickLoadData(2 + (selectionCountIndex), 43, QueryColumnNameStrings.StorySelectionRequireGoldMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 44, QueryColumnNameStrings.StorySelectionRequireGoldMaxColumn, typeof(int)) == false) continue;

					//card
					if (QuickLoadData(2 + (selectionCountIndex), 41, QueryColumnNameStrings.StorySelectionRequireCardNumberColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 42, QueryColumnNameStrings.StorySelectionRequireCardTableIndexColumn, typeof(int)) == false) continue;

					//achievement point
					if (QuickLoadData(2 + (selectionCountIndex), 45, QueryColumnNameStrings.StorySelectionRequireAchievementPointMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 46, QueryColumnNameStrings.StorySelectionRequireAchievementPointMaxColumn, typeof(int)) == false) continue;

					//reputation
					if (QuickLoadData(2 + (selectionCountIndex), 47, QueryColumnNameStrings.StorySelectionRequireReputationIndexColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 48, QueryColumnNameStrings.StorySelectionRequireReputationMinColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 49, QueryColumnNameStrings.StorySelectionRequireReputationMaxColumn, typeof(int)) == false) continue;

					//UpdateData
					if (dataDictionary.Count > 0)
					{
						dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySelectionRequirementTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, dataDictionary);
						dataDictionary.Clear();
					}

					//Reward Table
					//base reward
					if (QuickLoadData(2 + (selectionCountIndex), 16, QueryColumnNameStrings.RewardGoldColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 17, QueryColumnNameStrings.RewardExpColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 18, QueryColumnNameStrings.RewardHpColumn, typeof(int)) == false) continue;

					//card
					if (QuickLoadData(2 + (selectionCountIndex), 19, QueryColumnNameStrings.RewardCardCountColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 20, QueryColumnNameStrings.RewardCardTableIndexColumn, typeof(int)) == false) continue;

					//lost card
					if (QuickLoadData(2 + (selectionCountIndex), 23, QueryColumnNameStrings.LostCardCountColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 24, QueryColumnNameStrings.LostCardTableIndexColumn, typeof(int)) == false) continue;

					//equipment
					if (QuickLoadData(2 + (selectionCountIndex), 21, QueryColumnNameStrings.RewardEquipmentCountColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 22, QueryColumnNameStrings.RewardEquipmentTableIndexColumn, typeof(int)) == false) continue;

					//reputation
					if (QuickLoadData(2 + (selectionCountIndex), 25, QueryColumnNameStrings.ReputationIndexColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 26, QueryColumnNameStrings.ReputationSetValueColumn, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 27, QueryColumnNameStrings.ReputationAddValueColumn, typeof(int)) == false) continue;

					//collection
					if (QuickLoadData(2 + (selectionCountIndex), 28, QueryColumnNameStrings.RewardJounalIndexListColumn, typeof(string)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 55, QueryColumnNameStrings.RewardCollectionCharacterIndexList, typeof(string)) == false) continue;

					//forced reward card
					if (QuickLoadData(2 + (selectionCountIndex), 56, QueryColumnNameStrings.ForcedRewardCardCount, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 57, QueryColumnNameStrings.ForcedRewardCardTableIndex, typeof(int)) == false) continue;

					//forced lost card
					if (QuickLoadData(2 + (selectionCountIndex), 58, QueryColumnNameStrings.ForcedLostCardCount, typeof(int)) == false) continue;
					if (QuickLoadData(2 + (selectionCountIndex), 59, QueryColumnNameStrings.ForcedLostCardTableIndex, typeof(int)) == false) continue;

					//UpdateData
					if (dataDictionary.Count > 0)
					{
						dataReader.UpdateOrInsertData(sqliteConnection, QueryTableNameStrings.StorySelectionRewardTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, dataDictionary);
						dataDictionary.Clear();
					}
				}
				#endregion

				#region Instant method
				bool QuickReadDataFromExcel<T>(int _row, int _column, out T _data)
				{
					return dataReader.ReadDataFromTables(dataTable, sheetsIndex, _row + (dataCycleIndex * dataCycleSize), _column, out _data);
				}
				bool QuickLoadData(int _row, int _column, string _columnNameString, Type _inputType)
				{
					object __data;
					if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, _row + (dataCycleIndex * dataCycleSize), _column, out __data) == false) return false;
					if (__data != null)
					{
						if (__data.GetType() == typeof(double) && _inputType == typeof(string))
						{
							dataDictionary.Add(_columnNameString, (_inputType, (ParseInt(__data)).ToString()));
						}
						else
						{
							dataDictionary.Add(_columnNameString, (_inputType, __data));
						}
					}
					return true;
				}

				#endregion
			}

			return result;

			bool QuickLoadDataNoCycle(int _row, int _column, string _columnNameString, Type _inputType)
			{
				object __data;
				if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, _row, _column, out __data) == false) return false;
				if (__data != null)
				{
					if (__data.GetType() == typeof(double) && _inputType == typeof(string))
					{
						dataDictionary.Add(_columnNameString, (_inputType, (ParseInt(__data)).ToString()));
					}
					else
					{
						dataDictionary.Add(_columnNameString, (_inputType, __data));
					}
				}
				return true;
			}
			bool QuickLoadDataNoCycleWithDic(int _row, int _column, string _columnNameString, Type _inputType, Dictionary<string, (Type, object)> __dataDictionary)
			{
				object __data;
				if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, _row, _column, out __data) == false) return false;
				if (__data != null)
				{
					if (__data.GetType() == typeof(double) && _inputType == typeof(string))
					{
						__dataDictionary.Add(_columnNameString, (_inputType, (ParseInt(__data)).ToString()));
					}
					else
					{
						__dataDictionary.Add(_columnNameString, (_inputType, __data));
					}
				}
				return true;
			}
		}

		public bool SwipeEventData(CancellationToken cancellationToken, string dbFilePathString)
		{

			bool result = true;
			DataReader dataReader = new DataReader(DebugMessage, UpdateProgressBar);
			List<string> deleteTableList = new List<string>
			{
				QueryTableNameStrings.StorySentenceTable,
				QueryTableNameStrings.StorySentenceTextTable,
				QueryTableNameStrings.StorySelectionTable,
				QueryTableNameStrings.StorySelectionRequirementTable,
				QueryTableNameStrings.StorySelectionRequirementTextTable,
				QueryTableNameStrings.StorySelectionRewardTable,
				QueryTableNameStrings.StorySelectionTextTable,
				QueryTableNameStrings.StoryEventTable,
				QueryTableNameStrings.StoryEventHintTable,
				QueryTableNameStrings.StoryEventHintDescriptionTextTable,
				QueryTableNameStrings.StoryEventHintTitleTextTable,
				QueryTableNameStrings.StoryShopSelectionTable,
				QueryTableNameStrings.StoryEventTitleTextTable,
				QueryTableNameStrings.StoryEventEncounterTable,
				QueryTableNameStrings.ReputationRequirementPresetTable,
			};
			using (SQLiteConnection sqliteConnection = new SQLiteConnection($"Data Source={dbFilePathString};"))
			{
				try
				{
					sqliteConnection.Open();
					for (int i = 0; i < deleteTableList.Count; i++)
					{
						int progress = (int)Math.Round((double)i / deleteTableList.Count * 100);

						// 루프의 마지막에서 진행도를 100으로 설정
						if (i == deleteTableList.Count - 1)
						{
							progress = 100;
						}
						UpdateProgressBar(progress);

						var index = i;
						DebugMessage($"[{deleteTableList[index]}] Table swiped.");
						if (cancellationToken.IsCancellationRequested)
						{
							DebugMessage($"\r\n작업이 강제로 종료 되었습니다.");
							result = false;
							break;
						}
						result = dataReader.DeleteAllDataFromTable(sqliteConnection, deleteTableList[index]);
						if (result == false)
						{
							return result;
						}
					}
				}
				catch (Exception ex)
				{
					DebugMessage(ex.ToString());
					DebugMessage(dbFilePathString);
					result = false;
				}
				finally
				{
					sqliteConnection.Close();
				}
			}
			if (result) DebugMessage($"총 {deleteTableList.Count}개의 테이블 수정됨.\n");
			return result;
		}

		public async Task<bool> ConvertEventTextDataExcelToSQLite(CancellationToken cancellationToken, string dbFilePathString, string excelFilePathString, ENUM_LANGUAGE currnetLanguage)
		{
			bool? result = null;
			DataReader dataReader = new DataReader(DebugMessage, UpdateProgressBar);

			using (SQLiteConnection sqliteConnection = new SQLiteConnection($"Data Source={dbFilePathString};"))
			{
				Excel.Application excelApp = null;
				Workbook workbook = null;
				List<System.Data.DataTable> dataTable = new List<System.Data.DataTable>();
				try
				{
					excelApp = new Excel.Application();
					excelApp.ScreenUpdating = false;
					workbook = excelApp.Workbooks.Open(excelFilePathString, ReadOnly: true);
					dataTable = dataReader.ReadAllDataFromExcel(excelApp, workbook, excelFilePathString);
				}
				catch (Exception ex)
				{
					DebugMessage(ex.Message);
					result = false;
					DebugMessage("\r\n변환 실패.\r\n");
				}
				finally
				{
					if (workbook != null)
					{
						workbook.Close(false);
						Marshal.ReleaseComObject(workbook);
					}
					if (excelApp != null)
					{
						excelApp.Quit();
						Marshal.ReleaseComObject(excelApp);
					}
				}

				try
				{
					await sqliteConnection.OpenAsync(cancellationToken);
					// Repeat for the count of sheets asynchronously
					int sheetsCount = dataTable.Count;
					var tasks = new List<Task<bool>>(sheetsCount - 2); // Task 목록 생성
					int completeCount = 1;
					for (int sheetsIndex = 3; sheetsIndex <= sheetsCount; sheetsIndex++)
					{
						int __sheetsIndex = sheetsIndex;
						UpdateProgressBar(0);
						tasks.Add(Task.Run(() =>
						{
							//Data
							var _dataTable = dataTable;
							UpdateProgressBar((int)Math.Ceiling(((float)(completeCount++) / (float)(sheetsCount - 2)) * 100f));
							var processResult = ProcessEventTextDataSheet(__sheetsIndex, dataReader, sqliteConnection, _dataTable, currnetLanguage, cancellationToken);
							return processResult;
						})); // Task 추가
					}

					// 모든 Task가 완료될 때까지 기다림
					var taskResults = await Task.WhenAll(tasks);

					if (result == null)
					{
						result = taskResults.All(r => r); // 모든 Task가 성공했는지 확인
					}

					// 결과 확인 및 통합

					if (result == true)
					{
						DebugMessage("\r\n변환 성공.\r\n");
					}
					else
					{
						DebugMessage("\r\n변환 실패.\r\n");
					}

				}
				catch (Exception ex)
				{
					DebugMessage(ex.Message);
					result = false;
					DebugMessage("\r\n변환 실패.\r\n");
				}
				finally
				{
					sqliteConnection.Close();
				}
			}

			return result.Value;
		}

		public bool ProcessEventTextDataSheet(int sheetsIndex, DataReader dataReader, SQLiteConnection sqliteConnection, List<System.Data.DataTable> dataTable, ENUM_LANGUAGE currnetLanguage, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				throw new Exception($"\r\n작업이 강제로 종료 되었습니다.\r\n마지막 작업이 완료된 지점 : {sheetsIndex - 1}번째 Sheet.");
			}

			DebugMessage($"\r\n{sheetsIndex}번째 시트 작업중");

			//Repeat for the count of columns
			int dataCycleSize = 4;
			int dataCycleCount = ((dataReader.GetColumnsCount(dataTable[sheetsIndex - 1]) + 1) / dataCycleSize);
			if (dataCycleCount > 0)
			{
				object eventIndexObjectData;
				if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, 4, 1, out eventIndexObjectData) == false) return false;
				if (eventIndexObjectData == null) return false;
				int eventIndex = ParseInt(eventIndexObjectData);
				DebugMessage($"{eventIndex}번 이벤트 작업중");

				//Story Event Title
				if (QuickLoadDataWithoutCycle<string>(2, 1, out object eventTitle))
				{
					DebugMessage($"\r\n{eventTitle}");
					dataReader.UpdateDataCluster(sqliteConnection, QueryTableNameStrings.StoryEventTitleTextTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, languageColumnQueryString[currnetLanguage], eventTitle);
				}

				//Story Event Hint Title
				if (QuickLoadDataWithoutCycle<string>(6, 2, out object eventDataTitle))
				{
					dataReader.UpdateDataCluster(sqliteConnection, QueryTableNameStrings.StoryEventHintTitleTextTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, languageColumnQueryString[currnetLanguage], eventDataTitle);
				}

				//Story Event Hint Desc
				if (QuickLoadDataWithoutCycle<string>(8, 2, out object eventDataDesc))
				{
					dataReader.UpdateDataCluster(sqliteConnection, QueryTableNameStrings.StoryEventHintDescriptionTextTable, QueryColumnNameStrings.StoryEventIndexColumn, eventIndex, languageColumnQueryString[currnetLanguage], eventDataDesc);
				}
			}

			for (int dataCycleIndex = 0; dataCycleIndex < dataCycleCount; dataCycleIndex++)
			{
				//MainSentenceIndex
				object sentenceIndexObjectData;
				if (QuickLoadData<object>(1, 4, out sentenceIndexObjectData) == false) continue;
				if (sentenceIndexObjectData == null) continue;
				int sentenceIndex = ParseInt(sentenceIndexObjectData);

				#region Sentence Table

				//Selection index data
				List<int> selectionIndexList = new List<int>();
				object selectionIndexListObjectData;
				if (QuickLoadData<object>(2, 8, out selectionIndexListObjectData) == false) continue;
				if (selectionIndexListObjectData != null)
				{
					selectionIndexList.Add(ParseInt(selectionIndexListObjectData));
					selectionIndexListObjectData = null;
				}
				if (QuickLoadData<object>(3, 8, out selectionIndexListObjectData) == false) continue;
				if (selectionIndexListObjectData != null)
				{
					selectionIndexList.Add(ParseInt(selectionIndexListObjectData));
					selectionIndexListObjectData = null;
				}
				if (QuickLoadData<object>(4, 8, out selectionIndexListObjectData) == false) continue;
				if (selectionIndexListObjectData != null)
				{
					selectionIndexList.Add(ParseInt(selectionIndexListObjectData));
					selectionIndexListObjectData = null;
				}

				if (QuickLoadData<string>(2, 4, out object sentenceText))
				{
					sentenceText = ParseStringForQuery((string)sentenceText);
					if (dataCycleIndex == 0) DebugMessage($"{sentenceText}");
					dataReader.UpdateDataCluster(sqliteConnection, QueryTableNameStrings.StorySentenceTextTable, QueryColumnNameStrings.StorySentenceIndexColumn, sentenceIndex, languageColumnQueryString[currnetLanguage], sentenceText);
				}
				#endregion

				#region Selection Section

				int maxSelectionCount = 3;
				for (int selectionCountIndex = 0; selectionCountIndex < maxSelectionCount; selectionCountIndex++)
				{
					//Selection Index
					object selectionIndexObjectdata;
					if (QuickLoadData<object>(2 + (selectionCountIndex), 8, out selectionIndexObjectdata) == false) continue;
					if (selectionIndexObjectdata == null) continue;
					int _selectionIndex = ParseInt(selectionIndexObjectdata);

					//Selection Text
					if (QuickLoadData<string>(2 + (selectionCountIndex), 9, out object selectionText))
					{
						selectionText = ParseStringForQuery((string)selectionText);
						dataReader.UpdateDataCluster(sqliteConnection, QueryTableNameStrings.StorySelectionTextTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, languageColumnQueryString[currnetLanguage], selectionText);
					};

					//Selection Requirement Table

					//Requirement text
					if (QuickLoadData<string>(2 + (selectionCountIndex), 13, out object requirementText))
					{
						requirementText = ParseStringForQuery((string)requirementText);
						dataReader.UpdateDataCluster(sqliteConnection, QueryTableNameStrings.StorySelectionRequirementTextTable, QueryColumnNameStrings.StorySelectionIndexColumn, _selectionIndex, languageColumnQueryString[currnetLanguage], requirementText);
					}
				}
				#endregion

				#region Instant method
				bool QuickLoadData<T>(int _row, int _column, out object _data)
				{
					object _tempData;
					_data = default;

					if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, _row + (dataCycleIndex * dataCycleSize), _column, out _tempData) == false) return false;
					if (_tempData != null)
					{
						if (_tempData.GetType() == typeof(double) && typeof(T) == typeof(string))
						{
							_data = ParseInt(_tempData).ToString();
						}
						else
						{
							_data = _tempData;
						}
					}
					return true;
				}
				#endregion
			}

			return true;

			bool QuickLoadDataWithoutCycle<T>(int _row, int _column, out object _data)
			{
				object _tempData;
				_data = default;

				if (dataReader.ReadDataFromTables(dataTable, sheetsIndex, _row, _column, out _tempData) == false) return false;

				if (_tempData != null)
				{
					if (_tempData.GetType() == typeof(double) && typeof(T) == typeof(string))
					{
						_data = ParseInt(_tempData).ToString();
					}
					else
					{
						_data = _tempData;
					}
				}
				return true;
			}
		}

		public bool CreateLanguageData(CancellationToken cancellationToken, string excelFilePathString)
		{
			bool result = true;
			DataReader dataReader = new DataReader(DebugMessage, UpdateProgressBar);

			Excel.Application excelApp = null;
			Excel.Workbook workbook = null;
			Excel.Worksheet worksheet = null;

			try
			{
				excelApp = new Excel.Application();
				workbook = excelApp.Workbooks.Open(excelFilePathString, ReadOnly: true);
				worksheet = (Excel.Worksheet)workbook.Sheets[1]; // 첫 번째 시트

				Excel.Range range = worksheet.UsedRange;
				int rowCount = range.Rows.Count;
				int colCount = range.Columns.Count;

				var languageData = new Dictionary<ENUM_LANGUAGE, Dictionary<ENUM_DEFINE_STRING, string>>();
				DebugMessage($"Reading String Define...\r\n");
				for (int col = 2; col <= colCount; col++) // 2번째 열부터 시작
				{
					var languageEnumStringCell = range.Cells[1, col].Value2?.ToString();
					if (string.IsNullOrWhiteSpace(languageEnumStringCell)) break; // 1행의 데이터가 null이면 반복문 탈출

					if (!Enum.TryParse<ENUM_LANGUAGE>(languageEnumStringCell, out ENUM_LANGUAGE languageEnum)) continue; // Enum 변환 실패 시 다음 열로

					var stringData = new Dictionary<ENUM_DEFINE_STRING, string>();

					for (int row = 2; row <= rowCount; row++) // 2행부터 시작
					{
						cancellationToken.ThrowIfCancellationRequested();
						var defineEnumStringCell = range.Cells[row, 1].Value2?.ToString();
						var valueCell = range.Cells[row, col].Value2?.ToString();

						if (string.IsNullOrWhiteSpace(defineEnumStringCell)) break; // 빈 칸이 나오면 반복 중단

						if (Enum.TryParse<ENUM_DEFINE_STRING>(defineEnumStringCell, out ENUM_DEFINE_STRING defineEnum) && !string.IsNullOrWhiteSpace(valueCell))
						{
							valueCell = valueCell.Replace("\\n", "\n");
							stringData[defineEnum] = valueCell;
						}
					}
					DebugMessage($"{languageEnum.ToString()} has {stringData.Count} data. {Enum.GetValues(typeof(ENUM_DEFINE_STRING)).Length - stringData.Count} missing.\r");

					languageData[languageEnum] = stringData;
				}
				DebugMessage($"String Define converted.\r\n");
				UpdateProgressBar(30);
				worksheet = (Excel.Worksheet)workbook.Sheets[2]; // 두 번째 시트
				Excel.Range listRange = worksheet.UsedRange;
				int listRowCount = listRange.Rows.Count;
				int listColCount = listRange.Columns.Count;

				var languageListData = new Dictionary<ENUM_LANGUAGE, Dictionary<ENUM_DEFINE_LIST_STRING, string[]>>();

				DebugMessage($"Reading List String Define...\r\n");
				for (int col = 2; col <= listColCount; col++)
				{
					var languageEnumStringCell = listRange.Cells[1, col].Value2?.ToString();
					if (string.IsNullOrWhiteSpace(languageEnumStringCell)) break;

					if (!Enum.TryParse<ENUM_LANGUAGE>(languageEnumStringCell, out ENUM_LANGUAGE languageEnum)) continue;

					var listStringData = new Dictionary<ENUM_DEFINE_LIST_STRING, string[]>();

					for (int row = 2; row <= listRowCount; row++)
					{
						cancellationToken.ThrowIfCancellationRequested();
						var defineListEnumStringCell = listRange.Cells[row, 1].Value2?.ToString();
						var valueCell = listRange.Cells[row, col].Value2?.ToString();

						if (string.IsNullOrWhiteSpace(defineListEnumStringCell)) break;

						if (Enum.TryParse<ENUM_DEFINE_LIST_STRING>(defineListEnumStringCell, out ENUM_DEFINE_LIST_STRING defineListEnum))
						{
							// 쉼표로 구분하여 배열 생성, 빈 문자열 포함
							if (valueCell != null) valueCell = valueCell.Replace("\\n", "\n");
							string[] values = valueCell?.Split(new[] { ',' }, StringSplitOptions.None) ?? new string[0];
							listStringData[defineListEnum] = values;
						}
					}
					DebugMessage($"{languageEnum} has {listStringData.Count} data. {Enum.GetValues(typeof(ENUM_DEFINE_LIST_STRING)).Length - listStringData.Count} missing.\r");

					if (listStringData.Count > 0)
					{
						languageListData[languageEnum] = listStringData;
					}
				}
				DebugMessage($"List String Define converted.\r\n");
				UpdateProgressBar(60);

				// excelFilePathString의 디렉토리 경로 추출
				string directoryPath = Path.GetDirectoryName(excelFilePathString);

				// ENUM_LANGUAGE 값에 대한 순회
				foreach (ENUM_LANGUAGE languageEnum in Enum.GetValues(typeof(ENUM_LANGUAGE)))
				{
					cancellationToken.ThrowIfCancellationRequested();
					string filePath = Path.Combine(directoryPath, $"{languageEnum.ToString()}.json");

					// CommonDefineStringData 인스턴스 생성
					CommonDefineStringData commonDefineString = new CommonDefineStringData
					{
						stringData = languageData.ContainsKey(languageEnum) ? languageData[languageEnum] : new Dictionary<ENUM_DEFINE_STRING, string>(),
						listStringData = languageListData.ContainsKey(languageEnum) ? languageListData[languageEnum] : new Dictionary<ENUM_DEFINE_LIST_STRING, string[]>()
					};

					DebugMessage($"{languageEnum.ToString()}.json File created. \r");

					// Json 파일로 저장
					dataReader.WriteJsonData(filePath, commonDefineString);
				}
				UpdateProgressBar(100);
			}
			catch (Exception ex)
			{
				if (ex is OperationCanceledException && ((OperationCanceledException)ex).CancellationToken == cancellationToken)
				{
					DebugMessage("Canceled.\r");
				}
				else
				{
					DebugMessage($"{ex.Message}");
				}

				result = false;
			}
			finally
			{
				if (workbook != null)
				{
					workbook.Close(false);
					Marshal.ReleaseComObject(workbook);
				}
				if (excelApp != null)
				{
					excelApp.Quit();
					Marshal.ReleaseComObject(excelApp);
				}
			}

			return result;
		}

		public class CommonDefineStringData
		{
			public Dictionary<ENUM_DEFINE_STRING, string> stringData;
			public Dictionary<ENUM_DEFINE_LIST_STRING, string[]> listStringData;
		}

		private int ParseInt(object data)
		{
			int result;
			if (data.GetType() == typeof(string))
			{
				int.TryParse((string)data, out result);
			}
			else if (data.GetType() == typeof(double))
			{
				result = (int)((double)data);
			}
			else
			{
				result = (int)data;
			}
			return result;
		}

		private string ParseStringForQuery(string input)
		{
			string returnValue = input;
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}
			returnValue = returnValue.Replace("“", "\"");
			returnValue = returnValue.Replace("”", "\"");
			return returnValue;
		}

		private string ParseIntListToQueryString(List<int> dataIndexList)
		{
			string result = string.Join(",", dataIndexList.Select(x => x.ToString()).ToArray());
			return result;
		}
	}
}
