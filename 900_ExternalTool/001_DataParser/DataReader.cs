using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using Newtonsoft.Json.Converters;

namespace ExlixDataConverter
{
	class DataReader
	{
		Action<string> DebugMessage;
		Action<int> UpdateProgressBar;

		private static readonly object _dbLock = new object();

		public DataReader(Action<string> _debugMessage, Action<int> _updateProgressBar)
		{
			DebugMessage = _debugMessage;
			UpdateProgressBar = _updateProgressBar;
		}

		public bool UpdateOrInsertData(SQLiteConnection sqliteConnection, string tableName, string keyColumnName, object keyValue, Dictionary<string, (Type dataType, object data)> dataDictionary)
		{
			bool result = false;
			try
			{
				lock (_dbLock)
				{
					// Query to find a row with the given key value
					string selectQuery = $"SELECT * FROM {tableName} WHERE {keyColumnName}=@keyValue";
					using (var selectCommand = new SQLiteCommand(selectQuery, sqliteConnection))
					{
						selectCommand.Parameters.AddWithValue("@keyValue", keyValue);
						using (var reader = selectCommand.ExecuteReader())
						{
							if (reader.HasRows) // If a row with the given key value already exists
							{
								// Query to delete existing row with the given key value
								string deleteQuery = $"DELETE FROM {tableName} WHERE {keyColumnName}=@keyValue";
								using (var deleteCommand = new SQLiteCommand(deleteQuery, sqliteConnection))
								{
									deleteCommand.Parameters.AddWithValue("@keyValue", keyValue);
									deleteCommand.ExecuteNonQuery();
								}

								// Query to insert new row with data from dataDictionary
								string insertQuery = $"INSERT INTO {tableName}({keyColumnName}";
								string valuesQuery = $" VALUES(@keyValue";
								foreach (var pair in dataDictionary)
								{
									string columnName = pair.Key;
									var (dataType, data) = pair.Value;
									insertQuery += $", {columnName}";
									valuesQuery += $", @{columnName}";
								}
								insertQuery += ")";
								valuesQuery += ")";
								using (var insertCommand = new SQLiteCommand(insertQuery + valuesQuery, sqliteConnection))
								{
									insertCommand.Parameters.AddWithValue("@keyValue", keyValue);
									foreach (var pair in dataDictionary)
									{
										string columnName = pair.Key;
										var (dataType, data) = pair.Value;
										insertCommand.Parameters.AddWithValue($"@{columnName}", data);
									}
									insertCommand.ExecuteNonQuery();
								}
							}
							else // If a row with the given key value does not exist
							{
								// Query to insert a new row
								string insertQuery = $"INSERT INTO {tableName}({keyColumnName}";
								string valuesQuery = $" VALUES(@keyValue";
								foreach (var pair in dataDictionary)
								{
									string columnName = pair.Key;
									var (dataType, data) = pair.Value;
									insertQuery += $", {columnName}";
									valuesQuery += $", @{columnName}";
								}
								insertQuery += ")";
								valuesQuery += ")";
								using (var insertCommand = new SQLiteCommand(insertQuery + valuesQuery, sqliteConnection))
								{
									insertCommand.Parameters.AddWithValue("@keyValue", keyValue);
									foreach (var pair in dataDictionary)
									{
										string columnName = pair.Key;
										var (dataType, data) = pair.Value;
										insertCommand.Parameters.AddWithValue($"@{columnName}", data);
									}
									insertCommand.ExecuteNonQuery();
								}
							}
							result = true; // Success
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return result;
		}


		public bool InsertData(SQLiteConnection sqliteConnection, string tableName, string keyColumnName, Dictionary<string, (Type dataType, object data)> dataDictionary, out int primaryKeyValue)
		{
			bool result = false;
			primaryKeyValue = -1;

			lock (_dbLock)
			{
				using (var transaction = sqliteConnection.BeginTransaction()) // 트랜잭션 시작
				{
					try
					{
						while (true) // 실패 시 다시 시도
						{
							// Step 1: 사용되지 않은 가장 작은 키 값 찾기 (트랜잭션 내에서 실행)
							string findMissingKeyQuery = $@"
                    SELECT COALESCE(MIN(t1.{keyColumnName} + 1), 1) 
                    FROM {tableName} t1 
                    WHERE NOT EXISTS (
                        SELECT 1 FROM {tableName} t2 
                        WHERE t2.{keyColumnName} = t1.{keyColumnName} + 1
                    )";

							using (var keyCommand = new SQLiteCommand(findMissingKeyQuery, sqliteConnection, transaction))
							{
								object keyResult = keyCommand.ExecuteScalar();
								primaryKeyValue = keyResult != DBNull.Value ? Convert.ToInt32(keyResult) : 1;
							}

							// Step 2: INSERT 문 생성
							string insertQuery = $"INSERT INTO {tableName}({keyColumnName}, ";
							string valuesQuery = "VALUES(@PrimaryKey, ";

							foreach (var pair in dataDictionary)
							{
								string columnName = pair.Key;
								insertQuery += $"{columnName}, ";
								valuesQuery += $"@{columnName}, ";
							}

							insertQuery = insertQuery.TrimEnd(',', ' ') + ")";
							valuesQuery = valuesQuery.TrimEnd(',', ' ') + ")";

							using (var insertCommand = new SQLiteCommand(insertQuery + " " + valuesQuery, sqliteConnection, transaction))
							{
								// Step 3: 매개변수 추가
								insertCommand.Parameters.AddWithValue("@PrimaryKey", primaryKeyValue);

								foreach (var pair in dataDictionary)
								{
									string columnName = pair.Key;
									var (dataType, data) = pair.Value;
									insertCommand.Parameters.AddWithValue($"@{columnName}", data);
								}

								// Step 4: 실행
								try
								{
									insertCommand.ExecuteNonQuery();
									transaction.Commit(); // 성공하면 트랜잭션 커밋
									result = true;
									break; // 성공 시 루프 탈출
								}
								catch (SQLiteException ex) when ((SQLiteErrorCode)ex.ErrorCode == SQLiteErrorCode.Constraint)
								{
									// 기본 키 충돌 발생 시 primaryKeyValue를 +1 증가 후 재시도
									primaryKeyValue++;
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
						transaction.Rollback(); // 실패 시 롤백
					}
				}
			}
			return result;
		}

		public bool DeleteAllDataFromTable(SQLiteConnection sqliteConnection, string tableName)
		{
			try
			{
				// Query to delete all data from the table
				string deleteQuery = $"DELETE FROM {tableName}";

				using (var deleteCommand = new SQLiteCommand(deleteQuery, sqliteConnection))
				{
					// Execute the command
					deleteCommand.ExecuteNonQuery();
				}

				// If no exception was thrown, return true
				return true;
			}
			catch (Exception ex)
			{
				DebugMessage(ex.ToString());
				// Return false if an exception occurs
				return false;
			}
		}

		public bool UpdateDataCluster(SQLiteConnection sqliteConnection, string tableName, string keyColumnName, object keyValue, string columnName, object data)
		{
			bool result = false;
			try
			{
				lock (_dbLock)
				{
					// Query to find a row with the given key value
					string selectQuery = $"UPDATE {tableName} SET {columnName} = @data WHERE {keyColumnName} = @keyValue";
					using (var selectCommand = new SQLiteCommand(selectQuery, sqliteConnection))
					{
						selectCommand.Parameters.AddWithValue("@data", data);
						selectCommand.Parameters.AddWithValue("@keyValue", keyValue);

						// Execute the command
						int rowsAffected = selectCommand.ExecuteNonQuery();

						// If one or more rows were affected by the query, the data was updated
						result = rowsAffected > 0;
					}
				}
			}
			catch (Exception ex)
			{
				DebugMessage(ex.ToString());
			}
			return result;
		}

		public bool ReadSingleDataFromDB<T>(SQLiteConnection sqliteConnection, string tableName, string columnName, string keyColumnName, object keyValue, out T data) where T : class
		{
			data = null;
			try
			{
				string query = $"SELECT {columnName} FROM {tableName} WHERE {keyColumnName} = @keyValue LIMIT 1";
				using (var cmd = new SQLiteCommand(query, sqliteConnection))
				{
					cmd.Parameters.AddWithValue("@keyValue", keyValue);
					// 쿼리 실행
					using (var reader = cmd.ExecuteReader())
					{
						// 데이터가 존재하는지 체크
						if (reader.Read())
						{
							// 결과가 null이 아니면 해당 타입으로 변환하고 반환
							if (!reader.IsDBNull(0))
							{
								data = reader.GetValue(0) as T;
							}
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				DebugMessage($"Error in ReadSingleDataFromDB: {ex.Message}\r\n");
			}

			return false;
		}

		public bool ReadAllDataFromDB(SQLiteConnection sqliteConnection, string tableName, out List<List<object>> data, out List<string> columnData)
		{
			data = new List<List<object>>();
			columnData = new List<string>();
			try
			{
				string query = $"SELECT * FROM {tableName}";
				using (var cmd = new SQLiteCommand(query, sqliteConnection))
				{
					using (var reader = cmd.ExecuteReader())
					{
						// 컬럼 이름 추출
						DataTable schemaTable = reader.GetSchemaTable();
						foreach (DataRow row in schemaTable.Rows)
						{
							columnData.Add(row["ColumnName"].ToString());
						}
						// 데이터 읽기
						while (reader.Read())
						{
							var rowData = new List<object>();
							// 각 컬럼의 데이터를 읽어 rowData에 추가합니다.
							for (int i = 0; i < reader.FieldCount; i++)
							{
								rowData.Add(reader.GetValue(i));
							}

							data.Add(rowData);
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				DebugMessage($"Error in ReadAllDataFromDB: {ex.Message}\r\n");
			}
			return false;
		}

		public bool ReadCustomQueryDataFromDB(SQLiteConnection sqliteConnection, string query, out List<List<object>> data, out List<string> columnData)
		{
			data = new List<List<object>>();
			columnData = new List<string>();

			try
			{
				using (var cmd = new SQLiteCommand(query, sqliteConnection))
				{
					using (var reader = cmd.ExecuteReader())
					{
						// 첫 번째 행이 읽히기 전에 컬럼 이름을 추출합니다.
						DataTable schemaTable = reader.GetSchemaTable();
						foreach (DataRow row in schemaTable.Rows)
						{
							columnData.Add(row["ColumnName"].ToString());
						}

						// 데이터 읽기
						while (reader.Read())
						{
							var rowData = new List<object>();

							// 각 컬럼의 데이터를 읽어 rowData에 추가합니다.
							for (int i = 0; i < reader.FieldCount; i++)
							{
								rowData.Add(reader.GetValue(i));
							}

							data.Add(rowData);
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				DebugMessage($"Error in ReadCustomQueryDataFromDB: {ex.Message}\r\n");
			}
			return false;
		}

		public bool ReadDataFromExcel<T>(Excel.Application excelApp, Excel.Workbook workbook, int _sheetsIndex, int _row, int _colum, out T _data)
		{
			// _sheetsIndex start from 1
			// _cycleIndex start from 0
			_data = default(T);
			bool isSuccess = false;

			try
			{
				Excel.Worksheet worksheet = workbook.Sheets[_sheetsIndex];
				try
				{
					Excel.Range cell = worksheet.Cells[_colum, _row];
					if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						_data = cell.Value;
					}
					else
					{
						if (cell.Value == null)
						{
							_data = default;
						}
						else
						{
							_data = cell.Value;
						}
					}
					isSuccess = true;
				}
				catch (Exception ex)
				{
					DebugMessage(ex.Message);
				}
				finally
				{
					if (worksheet != null)
					{
						Marshal.ReleaseComObject(worksheet);
						worksheet = null;
					}
				}
			}
			catch (Exception ex)
			{
				DebugMessage(ex.Message);
			}

			return isSuccess;
		}

		// Excel 파일을 읽고 모든 시트를 DataTable 리스트로 변환하는 메서드
		public List<DataTable> ReadAllDataFromExcel(Excel.Application excelApp, Excel.Workbook workbook, string filePath)
		{
			List<DataTable> dataTables = new List<DataTable>();

			try
			{
				// Excel 파일 열기
				workbook = excelApp.Workbooks.Open(filePath);

				// 모든 시트 순회
				foreach (Excel.Worksheet worksheet in workbook.Sheets)
				{
					DataTable dataTable = new DataTable(worksheet.Name); // 시트명을 기반으로 DataTable 생성

					Excel.Range usedRange = worksheet.UsedRange; // 사용된 범위 가져오기
					object[,] data = usedRange.Value2 as object[,]; // 데이터를 배열로 한 번에 가져오기
					if (data == null) continue;

					int rowCount = data.GetLength(0); // 행 개수 가져오기
					int columnCount = data.GetLength(1); // 열 개수 가져오기

					// DataTable의 컬럼 생성
					for (int col = 1; col <= columnCount; col++)
					{
						dataTable.Columns.Add("Column" + col, typeof(string));
					}

					// 데이터 읽기 및 DataTable에 추가
					for (int row = 1; row <= rowCount; row++)
					{
						DataRow dataRow = dataTable.NewRow(); // 새 행 생성

						for (int col = 1; col <= columnCount; col++)
						{
							object cellValue = data[row, col]; // 셀 값 가져오기
															   //dataRow[col - 1] = cellValue?.ToString() ?? string.Empty;
							dataRow[col - 1] = cellValue ?? DBNull.Value;
							//dataRow[col - 1] = cellValue?.ToString() ?? null; // Null 값 처리 후 저장
						}

						dataTable.Rows.Add(dataRow);
					}

					dataTables.Add(dataTable); // DataTable 리스트에 추가
					Marshal.ReleaseComObject(worksheet); // 사용한 객체 해제
				}
			}
			catch (Exception ex)
			{
				DebugMessage("Error: " + ex.Message);
				Console.WriteLine("Error: " + ex.Message); // 예외 발생 시 출력
			}

			return dataTables; // DataTable 리스트 반환
		}

		// List<DataTable>을 사용하여 특정 데이터 값을 읽는 메서드
		public bool ReadDataFromTables<T>(List<DataTable> dataTables, int sheetIndex, int column, int row, out T data)
		{
			data = default(T);
			bool isSuccess = false;

			int adjustedSheetIndex = sheetIndex - 1;
			int adjustedRow = row - 1;
			int adjustedColumn = column - 1;

			try
			{
				if (adjustedSheetIndex < 0 || adjustedSheetIndex >= dataTables.Count)
					throw new ArgumentOutOfRangeException($"sheetIndex {adjustedSheetIndex}", "시트 인덱스가 범위를 벗어났습니다.");

				DataTable table = dataTables[adjustedSheetIndex];

				if (adjustedRow < 0 || adjustedRow >= table.Rows.Count || adjustedColumn < 0 || adjustedColumn >= table.Columns.Count)
					throw new ArgumentOutOfRangeException($"row/column {adjustedRow}/{adjustedColumn}", "행 또는 열 인덱스가 범위를 벗어났습니다.");

				object cellValue = table.Rows[adjustedRow][adjustedColumn];
				if (cellValue != DBNull.Value && cellValue != null)
				{
					data = (T)Convert.ChangeType(cellValue, typeof(T));
				}
				else
				{
					data = default(T);
				}
				isSuccess = true;
			}
			catch (Exception ex)
			{
				DebugMessage("Error: " + ex.Message);
				Console.WriteLine("Error: " + ex.Message);
			}

			return isSuccess;
		}

		public void WriteJsonData<T>(string filePath, in T data)
		{
			var settings = new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Include,
				Converters = new List<JsonConverter> { new StringEnumConverter() }
			};

			string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented, settings);

			File.WriteAllText(filePath, jsonData);
		}

		public int GetWorkSheetsCount(Excel.Application excelApp, Excel.Workbook workbook)
		{
			int sheetsCount = workbook.Sheets.Count;
			DebugMessage($"{sheetsCount} sheets exist.");
			return sheetsCount;
		}

		public int GetColumnsCount(Excel.Application excelApp, Excel.Workbook workbook, int _sheetsIndex)
		{
			Excel.Worksheet worksheet = workbook.Sheets[_sheetsIndex];
			//Get final column
			int lastColumn = worksheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell).Column;
			int columnsCount = -1;

			for (int i = lastColumn; i >= 1; i--)
			{
				Excel.Range cell = worksheet.Cells[4, i];
				//if Cell contains data
				if (cell.Value != null && cell.Value.ToString() != "")
				{
					columnsCount = i;
					DebugMessage($"Sheet:{_sheetsIndex} has {columnsCount} columns.");
					break;
				}

				Marshal.ReleaseComObject(cell);
			}

			Marshal.ReleaseComObject(worksheet);

			if (columnsCount < 0)
			{
				DebugMessage($"No Columns");
				return columnsCount;
			}
			return columnsCount + 2;
		}

		public int GetColumnsCount(DataTable table)
		{
			if (table == null || table.Columns.Count == 0)
			{
				Console.WriteLine($"Table '{table?.TableName ?? "Unknown"}' has no columns.");
				return -1;
			}

			int lastColumnIndex = -1;

			// 4번째 행을 기준으로 마지막 컬럼을 찾음 (엑셀 코드와 동일하게 4행 사용)
			if (table.Rows.Count >= 4)
			{
				DataRow row = table.Rows[3]; // 4번째 행 (0-based index: 3)

				for (int col = table.Columns.Count - 1; col >= 0; col--)
				{
					if (row[col] != DBNull.Value && row[col] != null && row[col].ToString().Trim() != "")
					{
						lastColumnIndex = col;
						Console.WriteLine($"Table '{table.TableName}' has {lastColumnIndex + 1} columns.");
						break;
					}
				}
			}

			if (lastColumnIndex < 0)
			{
				Console.WriteLine($"Table '{table.TableName}' has no valid columns.");
				return lastColumnIndex;
			}

			return lastColumnIndex + 2; // 기존 엑셀 코드와 동일한 +2 적용
		}


		public bool GetFileNames(string folderPath, out List<string> fileNames)
		{
			fileNames = new List<string>();

			try
			{
				// 지정된 경로에 있는 파일들의 전체 경로를 가져옵니다.
				string[] files = Directory.GetFiles(folderPath);

				// 각 파일 경로에서 파일 이름만 추출하여 리스트에 추가합니다.
				foreach (string file in files)
				{
					fileNames.Add(Path.GetFileName(file));
				}

				return true;
			}
			catch (Exception)
			{
				// 예외 발생 시 fileNames를 빈 리스트로 초기화하고 false를 반환합니다.
				fileNames = new List<string>();
				return false;
			}
		}
	}
}
