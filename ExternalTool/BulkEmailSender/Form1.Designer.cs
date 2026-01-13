namespace ExlixMail
{
	partial class Form1
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.MailTextBox = new System.Windows.Forms.TextBox();
			this.MailAdressTextbox = new System.Windows.Forms.TextBox();
			this.EmailAdressLabel = new System.Windows.Forms.Label();
			this.PasswordLabel = new System.Windows.Forms.Label();
			this.PasswordTextbox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ExelFileDirectionTextBox = new System.Windows.Forms.TextBox();
			this.FindExelPathButton = new System.Windows.Forms.Button();
			this.MailSubjectTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SendMailButton = new System.Windows.Forms.Button();
			this.DebugTextBox = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(8, 12);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(867, 23);
			this.progressBar1.TabIndex = 0;
			// 
			// MailTextBox
			// 
			this.MailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MailTextBox.Location = new System.Drawing.Point(8, 103);
			this.MailTextBox.Multiline = true;
			this.MailTextBox.Name = "MailTextBox";
			this.MailTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MailTextBox.Size = new System.Drawing.Size(609, 341);
			this.MailTextBox.TabIndex = 1;
			// 
			// MailAdressTextbox
			// 
			this.MailAdressTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MailAdressTextbox.Location = new System.Drawing.Point(629, 59);
			this.MailAdressTextbox.Name = "MailAdressTextbox";
			this.MailAdressTextbox.Size = new System.Drawing.Size(241, 21);
			this.MailAdressTextbox.TabIndex = 2;
			// 
			// EmailAdressLabel
			// 
			this.EmailAdressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EmailAdressLabel.AutoSize = true;
			this.EmailAdressLabel.Location = new System.Drawing.Point(630, 44);
			this.EmailAdressLabel.Name = "EmailAdressLabel";
			this.EmailAdressLabel.Size = new System.Drawing.Size(57, 12);
			this.EmailAdressLabel.TabIndex = 3;
			this.EmailAdressLabel.Text = "메일 주소";
			// 
			// PasswordLabel
			// 
			this.PasswordLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordLabel.AutoSize = true;
			this.PasswordLabel.Location = new System.Drawing.Point(630, 89);
			this.PasswordLabel.Name = "PasswordLabel";
			this.PasswordLabel.Size = new System.Drawing.Size(29, 12);
			this.PasswordLabel.TabIndex = 4;
			this.PasswordLabel.Text = "암호";
			// 
			// PasswordTextbox
			// 
			this.PasswordTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordTextbox.Location = new System.Drawing.Point(629, 104);
			this.PasswordTextbox.Name = "PasswordTextbox";
			this.PasswordTextbox.PasswordChar = '*';
			this.PasswordTextbox.Size = new System.Drawing.Size(241, 21);
			this.PasswordTextbox.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(630, 163);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 12);
			this.label1.TabIndex = 6;
			this.label1.Text = "엑셀 파일 위치";
			// 
			// ExelFileDirectionTextBox
			// 
			this.ExelFileDirectionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExelFileDirectionTextBox.Location = new System.Drawing.Point(629, 186);
			this.ExelFileDirectionTextBox.Name = "ExelFileDirectionTextBox";
			this.ExelFileDirectionTextBox.ReadOnly = true;
			this.ExelFileDirectionTextBox.Size = new System.Drawing.Size(241, 21);
			this.ExelFileDirectionTextBox.TabIndex = 7;
			// 
			// FindExelPathButton
			// 
			this.FindExelPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FindExelPathButton.Location = new System.Drawing.Point(778, 158);
			this.FindExelPathButton.Name = "FindExelPathButton";
			this.FindExelPathButton.Size = new System.Drawing.Size(92, 22);
			this.FindExelPathButton.TabIndex = 8;
			this.FindExelPathButton.Text = "찾아보기";
			this.FindExelPathButton.UseVisualStyleBackColor = true;
			this.FindExelPathButton.Click += new System.EventHandler(this.FindExelPathButton_Click);
			// 
			// MailSubjectTextBox
			// 
			this.MailSubjectTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MailSubjectTextBox.Location = new System.Drawing.Point(8, 59);
			this.MailSubjectTextBox.Name = "MailSubjectTextBox";
			this.MailSubjectTextBox.Size = new System.Drawing.Size(609, 21);
			this.MailSubjectTextBox.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 44);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 12);
			this.label2.TabIndex = 10;
			this.label2.Text = "메일 제목";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 89);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 12);
			this.label3.TabIndex = 11;
			this.label3.Text = "메일 내용";
			// 
			// SendMailButton
			// 
			this.SendMailButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendMailButton.Location = new System.Drawing.Point(630, 393);
			this.SendMailButton.Name = "SendMailButton";
			this.SendMailButton.Size = new System.Drawing.Size(170, 51);
			this.SendMailButton.TabIndex = 12;
			this.SendMailButton.Text = "메일 보내기";
			this.SendMailButton.UseVisualStyleBackColor = true;
			this.SendMailButton.Click += new System.EventHandler(this.SendMailButton_Click);
			// 
			// DebugTextBox
			// 
			this.DebugTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DebugTextBox.Location = new System.Drawing.Point(629, 228);
			this.DebugTextBox.Multiline = true;
			this.DebugTextBox.Name = "DebugTextBox";
			this.DebugTextBox.ReadOnly = true;
			this.DebugTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DebugTextBox.Size = new System.Drawing.Size(241, 152);
			this.DebugTextBox.TabIndex = 13;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(806, 393);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(64, 51);
			this.button1.TabIndex = 14;
			this.button1.Text = "취소";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(887, 480);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.DebugTextBox);
			this.Controls.Add(this.SendMailButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.MailSubjectTextBox);
			this.Controls.Add(this.FindExelPathButton);
			this.Controls.Add(this.ExelFileDirectionTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.PasswordTextbox);
			this.Controls.Add(this.PasswordLabel);
			this.Controls.Add(this.EmailAdressLabel);
			this.Controls.Add(this.MailAdressTextbox);
			this.Controls.Add(this.MailTextBox);
			this.Controls.Add(this.progressBar1);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(903, 519);
			this.Name = "Form1";
			this.Text = "네오 암스트롱 사이클론 제트 암스트롱 메일 추적 연속 발사 시스템 컨트롤 프로그램";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.TextBox MailTextBox;
		private System.Windows.Forms.TextBox MailAdressTextbox;
		private System.Windows.Forms.Label EmailAdressLabel;
		private System.Windows.Forms.Label PasswordLabel;
		private System.Windows.Forms.TextBox PasswordTextbox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox ExelFileDirectionTextBox;
		private System.Windows.Forms.Button FindExelPathButton;
		private System.Windows.Forms.TextBox MailSubjectTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button SendMailButton;
		private System.Windows.Forms.TextBox DebugTextBox;
		private System.Windows.Forms.Button button1;
	}
}

