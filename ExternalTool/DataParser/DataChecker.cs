using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExlixDataConverter
{
	public struct EventData
	{
		public int index;
		public string table;
	}

	internal class DataChecker
	{
		Action<string> DebugMessage;
		Action<int> UpdateProgressBar;

		public DataChecker(Action<string> _debugMessage, Action<int> _updateProgressBar)
		{
			DebugMessage = _debugMessage;
			UpdateProgressBar = _updateProgressBar;
		}

		public void CheckEventData()
		{

		}

		public void CheckEventIllustData(string dbFilePathString, string assetFolderPathString)
		{
			DataReader dataReader = new DataReader(DebugMessage, UpdateProgressBar);
			string query = $"SELECT {QueryTableNameStrings.EventIllustTable}.{QueryColumnNameStrings.EventIllustIndexColumn}, " +
						   $"{QueryTableNameStrings.EventIllustTable}.{QueryColumnNameStrings.ImagePathColumn} " +
						   $"FROM {QueryTableNameStrings.EventIllustTable}";

			using (SQLiteConnection sqliteConnection = new SQLiteConnection($"Data Source={dbFilePathString};"))
			{
				sqliteConnection.Open();
				UpdateProgressBar(20);

				if (dataReader.ReadCustomQueryDataFromDB(sqliteConnection, query, out List<List<object>> data, out List<string> columnData))
				{

					if (!Directory.Exists(assetFolderPathString))
					{
						DebugMessage("지정된 경로의 폴더가 존재하지 않습니다.\r\n");
						return;
					}

					if (!dataReader.GetFileNames(assetFolderPathString, out List<string> fileNames))
					{
						DebugMessage("파일 목록을 가져오는 데 실패했습니다.\r\n");
						return;
					}

					List<string> missingFiles = new List<string>();

					foreach (var row in data)
					{
						string illustIndex = row[0].ToString();
						string imagePath = row[1].ToString();
						string fullFilePath = Path.Combine(assetFolderPathString, imagePath);

						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFilePath);

						bool fileExists = fileNames.Any(fn => Path.GetFileNameWithoutExtension(fn).Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));

						if (!fileExists)
						{
							missingFiles.Add($"항목: {illustIndex}\n");
						}
					}

					if (missingFiles.Count > 0)
					{
						DebugMessage($"{missingFiles.Count}개의 파일이 누락되었습니다.\r\n");
						foreach (var missingFile in missingFiles)
						{
							DebugMessage(missingFile);
						}
					}
					else
					{
						DebugMessage("모든 파일이 존재합니다.\r\n");
					}
				}
				else
				{
					DebugMessage("데이터베이스에서 데이터를 읽는 데 실패했습니다.\r\n");
				}
			}
		}


		public void CheckEventIllustDBData(string dbFilePathString)
		{
			DataReader dataReader = new DataReader(DebugMessage, UpdateProgressBar);
			string query = $"SELECT {QueryTableNameStrings.StorySentenceTable}.{QueryColumnNameStrings.IllustIndexColumn} " +
						   $"FROM {QueryTableNameStrings.StorySentenceTable} " +
						   $"LEFT JOIN {QueryTableNameStrings.EventIllustTable} ON {QueryTableNameStrings.StorySentenceTable}.{QueryColumnNameStrings.IllustIndexColumn} = {QueryTableNameStrings.EventIllustTable}.{QueryColumnNameStrings.EventIllustIndexColumn} " +
						   $"WHERE {QueryTableNameStrings.StorySentenceTable}.{QueryColumnNameStrings.IllustIndexColumn} IS NOT NULL AND {QueryTableNameStrings.EventIllustTable}.{QueryColumnNameStrings.EventIllustIndexColumn} IS NULL;";

			using (SQLiteConnection sqliteConnection = new SQLiteConnection($"Data Source={dbFilePathString};"))
			{
				sqliteConnection.Open();
				UpdateProgressBar(10);
				if (dataReader.ReadCustomQueryDataFromDB(sqliteConnection, query, out List<List<object>> data, out List<string> columnData))
				{
					UpdateProgressBar(50);

					if (data.Count == 0)
					{
						DebugMessage("event_illust에서 누락된 항목이 없습니다.\r\n");
					}
					else
					{
						DebugMessage($"{data.Count}개의 항목이 event_illust에서 누락되었습니다.\r\n");
						foreach (var row in data)
						{
							DebugMessage($"항목: {row[0]}\n");
						}
					}
				}
				else
				{
					DebugMessage("데이터를 읽는 데 실패했습니다.\r\n");
				}
			}
			UpdateProgressBar(100);
		}
	}
}
