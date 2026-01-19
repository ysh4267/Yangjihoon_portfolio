using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulkEmailSender {
	public partial class Form : Form {
		public Form() {
			InitializeComponent();
		}

		private void FindExelPathButton_Click(object sender, EventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Excel 파일 (*.xlsx, *.xls)|*.xlsx;*.xls";
			openFileDialog.Title = "Excel 파일 선택";

			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				string selectedFilePath = openFileDialog.FileName;
				ExelFileDirectionTextBox.Text = selectedFilePath;
			}
		}

		private async void SendMailButton_Click(object sender, EventArgs e) {
			var userID = MailAdressTextbox.Text;
			var userPW = PasswordTextbox.Text;
			MailManager _mailManager = new MailManager(userID, userPW);
			ExcelManager _excelManager = new ExcelManager();
			var data = _excelManager.ReadData(ExelFileDirectionTextBox.Text);

			if (data.Count > 0) {
				progressBar1.Maximum = data.Count - 1;
				progressBar1.Minimum = 0;
				progressBar1.Value = 0;
			}

			await Task.Run(() => {
				for (int i = 0; i < data.Count; i++) {
					int index = i; // for loop variable를 클로저에서 사용하기 위해 별도의 변수에 할당
					var output = _mailManager.SendMail(userID, $"{data[index].id}", MailSubjectTextBox.Text, $"{MailTextBox.Text}\n{data[index].body}");
					DebugTextBox.Invoke((MethodInvoker)(() => DebugTextBox.AppendText($"{output}\r\n")));

					progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = index));
				}
			});
		}

		private void Form_Load(object sender, EventArgs e) {

		}
	}
}
