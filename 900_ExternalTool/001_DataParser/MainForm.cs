using System;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Media;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace DataParser
{
	public partial class MainForm : Form
	{
		DataConverter dataConverter;
		DataChecker dataChecker;
		CancellationTokenSource cancellationTokenSource;
		ENUM_LANGUAGE currentLanguageMenu;
		ENUM_DROPDOWN_MENU currentSelectedMenu;
		ENUM_DATA_CHECK_MENU currentSubCheckMenu;
		SoundPlayer completionSoundPlayer = new SoundPlayer(Properties.Resources.completion_sound);
		SoundPlayer successSoundPlayer = new SoundPlayer(Properties.Resources.success_sound);

		public MainForm()
		{
			//컴포넌트 초기화
			InitializeComponent();
			//데이터 컨버터 초기화
			dataConverter = new DataConverter(DebugMessage, UpdateProgressBar);
			dataChecker = new DataChecker(DebugMessage, UpdateProgressBar);
			//드롭다운 초기화
			DropdownComboBox.Items.Clear();
			// MyEnum의 모든 항목을 문자열 배열로 반환
			string[] enumValues = Enum.GetNames(typeof(ENUM_DROPDOWN_MENU));
			DropdownComboBox.Items.AddRange(enumValues);

			DropdownComboBox.SelectedIndex = 0;
			DropdownComboBox_SelectedIndexChanged(null, null);

			ExcelFilePathOutButton.Enabled = false;
			//완료 사운드
			completionSoundPlayer.Load();
			successSoundPlayer.Load();
		}

		private void DBFilePathButton_Click(object sender, EventArgs e)
		{
			//Load DBFile
			OpenFileDialog _openFileDialog = new OpenFileDialog();
			_openFileDialog.Filter = "SQLite Files (*.sqlite;*.db;*.dat)|*.sqlite;*.db;*.dat|All files (*.*)|*.*";
			_openFileDialog.FilterIndex = 1;
			_openFileDialog.RestoreDirectory = true;

			if (_openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string filePath = _openFileDialog.FileName;

				this.DBFilePathTextbox.Text = filePath;
			}
		}

		private void ExcelFilePathInButton_Click(object sender, EventArgs e)
		{
			if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataCheck && currentSubCheckMenu == ENUM_DATA_CHECK_MENU.EventIllustFile)
			{
				// 폴더 경로를 입력 받음
				FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

				if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					string folderPath = folderBrowserDialog.SelectedPath;
					// 폴더 경로 처리 로직
					// 중복 항목이 있다면 삭제 후 새로 추가
					if (this.ExcelFilePathListBox.Items.Contains(folderPath))
					{
						this.ExcelFilePathListBox.Items.Remove(folderPath);
					}
					this.ExcelFilePathListBox.Items.Add(folderPath);
				}
			}
			else
			{
				// 파일을 로드하는 로직
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Excel Files (*.xlsx;*.xlsm;*.xls)|*.xlsx;*.xlsm;*.xls|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					string filePath = openFileDialog.FileName;
					// 기존에 같은 파일 경로가 있다면 제거
					if (this.ExcelFilePathListBox.Items.Contains(filePath))
					{
						this.ExcelFilePathListBox.Items.Remove(filePath);
					}
					this.ExcelFilePathListBox.Items.Add(filePath);
				}
			}
		}

		private async void ConvertButton_Click(object sender, EventArgs e)
		{
			//Start Action
			Stopwatch stopwatch = new Stopwatch();
			StartAction();

			var filePathList = ExcelFilePathListBox.Items.Cast<string>().ToList();
			bool result = false;
			if (currentSelectedMenu == ENUM_DROPDOWN_MENU.StoryEvent)
			{
				if (filePathList.Count <= 0) DebugMessage($"등록된 엑셀 데이터 파일이 없습니다.");
				foreach (string excelFilePathText in filePathList)
				{
					DebugMessage($"{excelFilePathText} 데이터 삽입 작업 시작.");
					SelectItemToListBox(excelFilePathText);

					result = await dataConverter.ConvertEventDataExcelToSQLite(cancellationTokenSource.Token, DBFilePathTextbox.Text, excelFilePathText, currentLanguageMenu);

					if (cancellationTokenSource.IsCancellationRequested)
					{
						DebugMessage($"작업이 취소된 파일 : {excelFilePathText}");
						return;
					}
					if (result == false)
					{
						return;
					}
				}
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.StoryEventText)
			{
				if (filePathList.Count <= 0) DebugMessage($"등록된 엑셀 데이터 파일이 없습니다.");
				foreach (string excelFilePathText in filePathList)
				{
					DebugMessage($"{excelFilePathText} 텍스트 데이터 삽입 작업 시작.");
					SelectItemToListBox(excelFilePathText);

					result = await dataConverter.ConvertEventTextDataExcelToSQLite(cancellationTokenSource.Token, DBFilePathTextbox.Text, excelFilePathText, currentLanguageMenu);
					
					if (cancellationTokenSource.IsCancellationRequested)
					{
						DebugMessage($"작업이 취소된 파일 : {excelFilePathText}");
						return;
					}
					if (result == false)
					{
						return;
					}
				}
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataCheck && currentSubCheckMenu == ENUM_DATA_CHECK_MENU.EventIllustDBData)
			{
				await Task.Run(() =>
				{
					DebugMessage($"일러스트 인덱스 비교 작업 시작.");
					dataChecker.CheckEventIllustDBData(DBFilePathTextbox.Text);
				});
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataCheck && currentSubCheckMenu == ENUM_DATA_CHECK_MENU.EventIllustFile)
			{
				await Task.Run(() =>
				{
					if (filePathList.Count <= 0) DebugMessage($"등록된 폴더 경로가 없습니다.");
					DebugMessage($"{filePathList[0]} 폴더 탐색 시작.");
					SelectItemToListBox(filePathList[0]);
					dataChecker.CheckEventIllustData(DBFilePathTextbox.Text, filePathList[0]);
				});
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataSwipe)
			{
				await Task.Run(() =>
				{
					DialogResult dialogResult = MessageBox.Show("정말 데이터를 모두 지우시겠습니까?", "데이터 제거", MessageBoxButtons.YesNo);
					if (dialogResult == DialogResult.Yes)
					{
						result = dataConverter.SwipeEventData(cancellationTokenSource.Token, DBFilePathTextbox.Text);
						if (result) DebugMessage("Swipe completed.\r");
						else DebugMessage("Swipe failed.\r");
					}
					else if (dialogResult == DialogResult.No)
					{
						DebugMessage("Canceled.\r");
						UpdateProgressBar(0);
					}
				});
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.UIText)
			{
				await Task.Run(() =>
				{
					var excelFilePath = ExcelFilePathListBox.Items.Cast<string>().ToList();
					DebugMessage($"{excelFilePath[0]}엑셀 파일로부터 Json 파일 생성중.\r");
					SelectItemToListBox(excelFilePath[0]);
					result = dataConverter.CreateLanguageData(cancellationTokenSource.Token, excelFilePath[0]);
					if (result) DebugMessage("작업 완료.\r");
					else DebugMessage("작업 실패.\r");
				});
			}
			else
			{
				DebugMessage($"구현되지 않은 기능입니다.\r");
			}

			EndAction();

			void StartAction()
			{
				//드롭다운 메뉴
				DropdownComboBox.Enabled = false;
				SubMenuDropdownComboBox.Enabled = false;
				//엑셀
				ExcelFilePathInButton.Enabled = false;
				ExcelFilePathOutButton.Enabled = false;
				ExcelFilePathClearButton.Enabled = false;
				//DB
				DBFilePathButton.Enabled = false;
				//취소버튼
				CancelConvertButton.Enabled = true;
				ConvertButton.Enabled = false;
				//취소토큰 초기화
				cancellationTokenSource = new CancellationTokenSource();
				//사운드 초기화
				completionSoundPlayer.Stop();
				successSoundPlayer.Stop();
				UpdateProgressBar(0);
				stopwatch.Start();
			}
			void EndAction()
			{
				stopwatch.Stop();
				//드롭다운 메뉴
				DropdownComboBox.Enabled = true;
				SubMenuDropdownComboBox.Enabled = true;
				//엑셀 파일
				ExcelFilePathInButton.Enabled = true;
				ExcelFilePathClearButton.Enabled = true;
				ExcelFilePathListBox_SelectedIndexChanged(null, null);
				//DB 버튼
				DBFilePathButton.Enabled = true;
				//취소 버튼
				CancelConvertButton.Enabled = false;
				ConvertButton.Enabled = true;
				UpdateProgressBar(0);
				//소모 시간
				double totalSeconds = stopwatch.Elapsed.TotalSeconds;
				int minutes = (int)(totalSeconds / 60);
				double seconds = totalSeconds % 60;
				string elapsedTime = minutes > 0 ? $"{minutes}m {seconds:0.000}s" : $"{seconds:0.000}s";
				DebugMessage($"Elapsed Time: {elapsedTime}");

				//알림음
				if (result == false)
				{
					SystemSounds.Exclamation.Play();
				}
				else if (SuccessSoundCheckBox.Checked == true)
				{
					successSoundPlayer.Play();
				}
				else if (CompletionSoundCheckBox.Checked == true)
				{
					completionSoundPlayer.Play();
				}
				else
				{
					SystemSounds.Asterisk.Play();
				}
			}
		}

		private void DropdownComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			currentSelectedMenu = (ENUM_DROPDOWN_MENU)DropdownComboBox.SelectedIndex;
			if (currentSelectedMenu == ENUM_DROPDOWN_MENU.StoryEvent || currentSelectedMenu == ENUM_DROPDOWN_MENU.StoryEventText)
			{
				SubMenuDropdownComboBox.Items.Clear();
				// MyEnum의 모든 항목을 문자열 배열로 반환
				string[] enumValues = Enum.GetNames(typeof(ENUM_LANGUAGE));
				SubMenuDropdownComboBox.Items.AddRange(enumValues);
				SubMenuDropdownComboBox.SelectedIndex = 0;
				SubMenuDropdownComboBox_SelectedIndexChanged(null, null);
				SubMenuDropdownComboBox.Enabled = true;
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataCheck)
			{
				SubMenuDropdownComboBox.Items.Clear();
				// MyEnum의 모든 항목을 문자열 배열로 반환
				string[] enumValues = Enum.GetNames(typeof(ENUM_DATA_CHECK_MENU));
				SubMenuDropdownComboBox.Items.AddRange(enumValues);
				SubMenuDropdownComboBox.SelectedIndex = 0;
				SubMenuDropdownComboBox_SelectedIndexChanged(null, null);
				SubMenuDropdownComboBox.Enabled = true;
			}
			else
			{
				SubMenuDropdownComboBox.Items.Clear();
				SubMenuDropdownComboBox.Enabled = false;
				SubMenuDropdownComboBox_SelectedIndexChanged(null, null);
			}
		}

		private void SubMenuDropdownComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ExcelFilePathInButton.Text = "ExcelFile In";
			ExcelFilePathOutButton.Text = "ExcelFile Out";
			SubMenuDropdownComboBox.Enabled = true;
			ExcelFilePathInButton.Enabled = true;
			DBFilePathButton.Enabled = true;

			if (currentSelectedMenu == ENUM_DROPDOWN_MENU.StoryEvent || currentSelectedMenu == ENUM_DROPDOWN_MENU.StoryEventText)
			{
				currentLanguageMenu = (ENUM_LANGUAGE)SubMenuDropdownComboBox.SelectedIndex;
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataCheck)
			{
				currentSubCheckMenu = (ENUM_DATA_CHECK_MENU)SubMenuDropdownComboBox.SelectedIndex;
				if (currentSubCheckMenu == ENUM_DATA_CHECK_MENU.EventIllustFile)
				{
					ExcelFilePathInButton.Text = "FolderPath In";
					ExcelFilePathOutButton.Text = "FolderPath Out";
				}
				else if (currentSubCheckMenu == ENUM_DATA_CHECK_MENU.EventIllustDBData)
				{
					ExcelFilePathInButton.Enabled = false;
				}
			}
			else if (currentSelectedMenu == ENUM_DROPDOWN_MENU.UIText)
			{
				DBFilePathButton.Enabled = false;
			}
		}

		void SelectItemToListBox(string itemToSelect)
		{
			if (ExcelFilePathListBox.InvokeRequired)
			{
				ExcelFilePathListBox.Invoke(new System.Action(() => SelectItemToListBox(itemToSelect)));
			}
			else
			{
				ExcelFilePathListBox.ClearSelected();  // 모든 항목의 선택을 해제

				int index = ExcelFilePathListBox.Items.IndexOf(itemToSelect);
				if (index != -1)  // 항목이 ListBox에 있다면
				{
					ExcelFilePathListBox.SetSelected(index, true);  // 해당 항목을 선택
				}
			}
		}

		void DebugMessage(object inputData)
		{
			if (DebugTextBox.InvokeRequired)
			{
				DebugTextBox.Invoke(new System.Action(() => DebugMessage(inputData)));
			}
			else
			{
				DebugTextBox.AppendText($"{inputData}\r\n");
			}
		}

		void UpdateProgressBar(int value)
		{
			if (value >= ConvertProgressBar.Maximum) value = ConvertProgressBar.Maximum;
			if (value <= ConvertProgressBar.Minimum) value = ConvertProgressBar.Minimum;
			if (ConvertProgressBar.InvokeRequired)
			{
				ConvertProgressBar.Invoke(new System.Action(() => UpdateProgressBar(value)));
			}
			else
			{
				ConvertProgressBar.Value = value;
			}
		}

		private void CancelConvertButton_Click(object sender, EventArgs e)
		{
			cancellationTokenSource.Cancel();
			CancelConvertButton.Enabled = false;
		}

		private void ExcelFilePathOutButton_Click(object sender, EventArgs e)
		{
			// 선택된 항목들을 다른 리스트에 복사
			List<object> itemsToRemove = new List<object>();
			foreach (var item in ExcelFilePathListBox.SelectedItems)
			{
				itemsToRemove.Add(item);
			}

			// 리스트에 복사된 항목들을 기반으로 항목 제거
			foreach (var item in itemsToRemove)
			{
				ExcelFilePathListBox.Items.Remove(item);
			}
		}

		private void ExcelFilePathClearButton_Click(object sender, EventArgs e)
		{
			ExcelFilePathListBox.Items.Clear();
			ExcelFilePathOutButton.Enabled = false;
		}

		private void ExcelFilePathListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ExcelFilePathInButton.Enabled == false) return;
			if (ExcelFilePathListBox.SelectedItems.Count > 0)
			{
				ExcelFilePathOutButton.Enabled = true;
			}
			else
			{
				ExcelFilePathOutButton.Enabled = false;
			}
		}

		private void MainForm_DragEnter(object sender, DragEventArgs e)
		{
			if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataCheck && currentSubCheckMenu == ENUM_DATA_CHECK_MENU.EventIllustFile)
			{
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

					// 모든 드래그된 항목이 디렉토리인지 확인
					foreach (string filePath in filePaths)
					{
						if (!Directory.Exists(filePath))
						{
							e.Effect = DragDropEffects.None;
							return;
						}
					}

					e.Effect = DragDropEffects.Copy;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}
			else
			{
				if (e.Data.GetDataPresent(DataFormats.FileDrop))
				{
					string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

					// 허용된 확장자 목록
					List<string> allowedExtensions = new List<string> { ".xlsx", ".xlsm", ".xls", ".sqlite", ".db", ".dat" };

					// 드래그된 파일 중 허용되지 않은 확장자가 있다면 None 처리
					foreach (string filePath in filePaths)
					{
						string extension = Path.GetExtension(filePath).ToLower();
						if (!allowedExtensions.Contains(extension))
						{
							e.Effect = DragDropEffects.None;
							return;
						}
					}

					e.Effect = DragDropEffects.Copy;
				}
				else
				{
					e.Effect = DragDropEffects.None;
				}
			}
		}

		private void MainForm_DragDrop(object sender, DragEventArgs e)
		{
			if (currentSelectedMenu == ENUM_DROPDOWN_MENU.DataCheck && currentSubCheckMenu == ENUM_DATA_CHECK_MENU.EventIllustFile)
			{
				// 드롭된 항목들을 가져옵니다.
				string[] droppedItems = (string[])e.Data.GetData(DataFormats.FileDrop);

				// 각 항목이 디렉토리인지 확인합니다.
				foreach (string item in droppedItems)
				{
					if (Directory.Exists(item))
					{
						// 여기에서 디렉토리 경로로 필요한 작업을 수행합니다.
						// 예: 리스트 박스에 경로 추가
						this.ExcelFilePathListBox.Items.Add(item);
					}
				}
			}
			else
			{
				string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

				foreach (string filePath in filePaths)
				{
					string fileExtension = Path.GetExtension(filePath).ToLower();

					if (fileExtension == ".xlsx" || fileExtension == ".xlsm" || fileExtension == ".xls")
					{
						// If the list box already contains the same file path, remove it
						if (this.ExcelFilePathListBox.Items.Contains(filePath))
						{
							this.ExcelFilePathListBox.Items.Remove(filePath);
						}

						// Add the new file path
						this.ExcelFilePathListBox.Items.Add(filePath);
					}
					else if (fileExtension == ".sqlite" || fileExtension == ".db" || fileExtension == ".dat")
					{
						this.DBFilePathTextbox.Text = filePath;
					}
				}
			}
		}

		private void CompletionSoundCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			successSoundPlayer.Stop();
			if (CompletionSoundCheckBox.Checked == true)
			{
				SuccessSoundCheckBox.Checked = false;
			}
		}

		private void SuccessSoundCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			completionSoundPlayer.Stop();
			if (SuccessSoundCheckBox.Checked == true)
			{
				CompletionSoundCheckBox.Checked = false;
			}
		}
	}
}
