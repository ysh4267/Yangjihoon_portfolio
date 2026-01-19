namespace DataParser
{
	partial class MainForm
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.DBFilePathButton = new System.Windows.Forms.Button();
			this.DBFilePathTextbox = new System.Windows.Forms.TextBox();
			this.ExcelFilePathInButton = new System.Windows.Forms.Button();
			this.DropdownComboBox = new System.Windows.Forms.ComboBox();
			this.DropdownLabel = new System.Windows.Forms.Label();
			this.ConvertButton = new System.Windows.Forms.Button();
			this.DebugTextBox = new System.Windows.Forms.TextBox();
			this.ConvertProgressBar = new System.Windows.Forms.ProgressBar();
			this.SubMenuDropdownComboBox = new System.Windows.Forms.ComboBox();
			this.CancelConvertButton = new System.Windows.Forms.Button();
			this.ExcelFilePathOutButton = new System.Windows.Forms.Button();
			this.ExcelFilePathListBox = new System.Windows.Forms.ListBox();
			this.ExcelFilePathClearButton = new System.Windows.Forms.Button();
			this.CompletionSoundCheckBox = new System.Windows.Forms.CheckBox();
			this.SuccessSoundCheckBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// DBFilePathButton
			// 
			this.DBFilePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DBFilePathButton.Location = new System.Drawing.Point(662, 248);
			this.DBFilePathButton.Name = "DBFilePathButton";
			this.DBFilePathButton.Size = new System.Drawing.Size(114, 27);
			this.DBFilePathButton.TabIndex = 3;
			this.DBFilePathButton.Text = "DBFile";
			this.DBFilePathButton.UseVisualStyleBackColor = true;
			this.DBFilePathButton.Click += new System.EventHandler(this.DBFilePathButton_Click);
			// 
			// DBFilePathTextbox
			// 
			this.DBFilePathTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DBFilePathTextbox.Location = new System.Drawing.Point(27, 252);
			this.DBFilePathTextbox.Name = "DBFilePathTextbox";
			this.DBFilePathTextbox.ReadOnly = true;
			this.DBFilePathTextbox.Size = new System.Drawing.Size(619, 21);
			this.DBFilePathTextbox.TabIndex = 6;
			this.DBFilePathTextbox.TabStop = false;
			this.DBFilePathTextbox.WordWrap = false;
			// 
			// ExcelFilePathInButton
			// 
			this.ExcelFilePathInButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExcelFilePathInButton.Location = new System.Drawing.Point(662, 291);
			this.ExcelFilePathInButton.Name = "ExcelFilePathInButton";
			this.ExcelFilePathInButton.Size = new System.Drawing.Size(114, 27);
			this.ExcelFilePathInButton.TabIndex = 4;
			this.ExcelFilePathInButton.Text = "ExcelFile In";
			this.ExcelFilePathInButton.UseVisualStyleBackColor = true;
			this.ExcelFilePathInButton.Click += new System.EventHandler(this.ExcelFilePathInButton_Click);
			// 
			// DropdownComboBox
			// 
			this.DropdownComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DropdownComboBox.FormattingEnabled = true;
			this.DropdownComboBox.Location = new System.Drawing.Point(131, 20);
			this.DropdownComboBox.Name = "DropdownComboBox";
			this.DropdownComboBox.Size = new System.Drawing.Size(121, 20);
			this.DropdownComboBox.TabIndex = 0;
			this.DropdownComboBox.SelectedIndexChanged += new System.EventHandler(this.DropdownComboBox_SelectedIndexChanged);
			// 
			// DropdownLabel
			// 
			this.DropdownLabel.AutoSize = true;
			this.DropdownLabel.Location = new System.Drawing.Point(25, 25);
			this.DropdownLabel.Name = "DropdownLabel";
			this.DropdownLabel.Size = new System.Drawing.Size(97, 12);
			this.DropdownLabel.TabIndex = 8;
			this.DropdownLabel.Text = "삽입 데이터 종류";
			// 
			// ConvertButton
			// 
			this.ConvertButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConvertButton.Location = new System.Drawing.Point(662, 60);
			this.ConvertButton.Name = "ConvertButton";
			this.ConvertButton.Size = new System.Drawing.Size(114, 118);
			this.ConvertButton.TabIndex = 2;
			this.ConvertButton.Text = "변환하기";
			this.ConvertButton.UseVisualStyleBackColor = true;
			this.ConvertButton.Click += new System.EventHandler(this.ConvertButton_Click);
			// 
			// DebugTextBox
			// 
			this.DebugTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DebugTextBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.DebugTextBox.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.DebugTextBox.Location = new System.Drawing.Point(27, 60);
			this.DebugTextBox.MaxLength = 32767000;
			this.DebugTextBox.Multiline = true;
			this.DebugTextBox.Name = "DebugTextBox";
			this.DebugTextBox.ReadOnly = true;
			this.DebugTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DebugTextBox.Size = new System.Drawing.Size(619, 173);
			this.DebugTextBox.TabIndex = 5;
			this.DebugTextBox.TabStop = false;
			// 
			// ConvertProgressBar
			// 
			this.ConvertProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConvertProgressBar.Location = new System.Drawing.Point(394, 19);
			this.ConvertProgressBar.Name = "ConvertProgressBar";
			this.ConvertProgressBar.Size = new System.Drawing.Size(361, 23);
			this.ConvertProgressBar.TabIndex = 9;
			// 
			// SubMenuDropdownComboBox
			// 
			this.SubMenuDropdownComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SubMenuDropdownComboBox.FormattingEnabled = true;
			this.SubMenuDropdownComboBox.Items.AddRange(new object[] {
            "StoryEvent"});
			this.SubMenuDropdownComboBox.Location = new System.Drawing.Point(258, 20);
			this.SubMenuDropdownComboBox.Name = "SubMenuDropdownComboBox";
			this.SubMenuDropdownComboBox.Size = new System.Drawing.Size(121, 20);
			this.SubMenuDropdownComboBox.TabIndex = 1;
			this.SubMenuDropdownComboBox.SelectedIndexChanged += new System.EventHandler(this.SubMenuDropdownComboBox_SelectedIndexChanged);
			// 
			// CancelConvertButton
			// 
			this.CancelConvertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelConvertButton.Enabled = false;
			this.CancelConvertButton.Location = new System.Drawing.Point(662, 184);
			this.CancelConvertButton.Name = "CancelConvertButton";
			this.CancelConvertButton.Size = new System.Drawing.Size(114, 49);
			this.CancelConvertButton.TabIndex = 10;
			this.CancelConvertButton.Text = "중단하기";
			this.CancelConvertButton.UseVisualStyleBackColor = true;
			this.CancelConvertButton.Click += new System.EventHandler(this.CancelConvertButton_Click);
			// 
			// ExcelFilePathOutButton
			// 
			this.ExcelFilePathOutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExcelFilePathOutButton.Location = new System.Drawing.Point(662, 324);
			this.ExcelFilePathOutButton.Name = "ExcelFilePathOutButton";
			this.ExcelFilePathOutButton.Size = new System.Drawing.Size(114, 27);
			this.ExcelFilePathOutButton.TabIndex = 11;
			this.ExcelFilePathOutButton.Text = "ExcelFile Out";
			this.ExcelFilePathOutButton.UseVisualStyleBackColor = true;
			this.ExcelFilePathOutButton.Click += new System.EventHandler(this.ExcelFilePathOutButton_Click);
			// 
			// ExcelFilePathListBox
			// 
			this.ExcelFilePathListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ExcelFilePathListBox.FormattingEnabled = true;
			this.ExcelFilePathListBox.ItemHeight = 12;
			this.ExcelFilePathListBox.Location = new System.Drawing.Point(27, 288);
			this.ExcelFilePathListBox.Name = "ExcelFilePathListBox";
			this.ExcelFilePathListBox.ScrollAlwaysVisible = true;
			this.ExcelFilePathListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.ExcelFilePathListBox.Size = new System.Drawing.Size(619, 100);
			this.ExcelFilePathListBox.TabIndex = 12;
			this.ExcelFilePathListBox.SelectedIndexChanged += new System.EventHandler(this.ExcelFilePathListBox_SelectedIndexChanged);
			// 
			// ExcelFilePathClearButton
			// 
			this.ExcelFilePathClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExcelFilePathClearButton.Location = new System.Drawing.Point(662, 357);
			this.ExcelFilePathClearButton.Name = "ExcelFilePathClearButton";
			this.ExcelFilePathClearButton.Size = new System.Drawing.Size(114, 27);
			this.ExcelFilePathClearButton.TabIndex = 13;
			this.ExcelFilePathClearButton.Text = "Clear";
			this.ExcelFilePathClearButton.UseVisualStyleBackColor = true;
			this.ExcelFilePathClearButton.Click += new System.EventHandler(this.ExcelFilePathClearButton_Click);
			// 
			// CompletionSoundCheckBox
			// 
			this.CompletionSoundCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CompletionSoundCheckBox.AutoSize = true;
			this.CompletionSoundCheckBox.Location = new System.Drawing.Point(761, 15);
			this.CompletionSoundCheckBox.Name = "CompletionSoundCheckBox";
			this.CompletionSoundCheckBox.Size = new System.Drawing.Size(15, 14);
			this.CompletionSoundCheckBox.TabIndex = 14;
			this.CompletionSoundCheckBox.Text = "Sound A";
			this.CompletionSoundCheckBox.UseVisualStyleBackColor = true;
			this.CompletionSoundCheckBox.CheckedChanged += new System.EventHandler(this.CompletionSoundCheckBox_CheckedChanged);
			// 
			// SuccessSoundCheckBox
			// 
			this.SuccessSoundCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SuccessSoundCheckBox.AutoSize = true;
			this.SuccessSoundCheckBox.Location = new System.Drawing.Point(761, 32);
			this.SuccessSoundCheckBox.Name = "SuccessSoundCheckBox";
			this.SuccessSoundCheckBox.Size = new System.Drawing.Size(15, 14);
			this.SuccessSoundCheckBox.TabIndex = 15;
			this.SuccessSoundCheckBox.Text = "Sound B";
			this.SuccessSoundCheckBox.UseVisualStyleBackColor = true;
			this.SuccessSoundCheckBox.CheckedChanged += new System.EventHandler(this.SuccessSoundCheckBox_CheckedChanged);
			// 
			// MainForm
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 409);
			this.Controls.Add(this.SuccessSoundCheckBox);
			this.Controls.Add(this.CompletionSoundCheckBox);
			this.Controls.Add(this.ExcelFilePathClearButton);
			this.Controls.Add(this.ExcelFilePathListBox);
			this.Controls.Add(this.ExcelFilePathOutButton);
			this.Controls.Add(this.CancelConvertButton);
			this.Controls.Add(this.SubMenuDropdownComboBox);
			this.Controls.Add(this.ConvertProgressBar);
			this.Controls.Add(this.DebugTextBox);
			this.Controls.Add(this.ConvertButton);
			this.Controls.Add(this.DropdownLabel);
			this.Controls.Add(this.DropdownComboBox);
			this.Controls.Add(this.ExcelFilePathInButton);
			this.Controls.Add(this.DBFilePathTextbox);
			this.Controls.Add(this.DBFilePathButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(816, 361);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "DataParser";
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.Button DBFilePathButton;
		public System.Windows.Forms.TextBox DBFilePathTextbox;
		public System.Windows.Forms.Button ExcelFilePathInButton;
		public System.Windows.Forms.ComboBox DropdownComboBox;
		public System.Windows.Forms.Label DropdownLabel;
		public System.Windows.Forms.Button ConvertButton;
		public System.Windows.Forms.ComboBox SubMenuDropdownComboBox;
		public System.Windows.Forms.ProgressBar ConvertProgressBar;
		public System.Windows.Forms.TextBox DebugTextBox;
		public System.Windows.Forms.Button CancelConvertButton;
		public System.Windows.Forms.Button ExcelFilePathOutButton;
		public System.Windows.Forms.Button ExcelFilePathClearButton;
		public System.Windows.Forms.ListBox ExcelFilePathListBox;
		private System.Windows.Forms.CheckBox CompletionSoundCheckBox;
		private System.Windows.Forms.CheckBox SuccessSoundCheckBox;
	}
}

