using System;
using System.Net;
using System.Net.Mail;


namespace ExlixMail
{
	internal class MailManager
	{
		private SmtpClient smtpClient;
		private string smtpServer;
		private int smtpPort;
		private string smtpUsername;
		private string smtpPassword;

		public MailManager(string smtpUsername, string smtpPassword)
		{
			this.smtpServer = "smtp.gmail.com";
			this.smtpPort = 587;
			this.smtpUsername = smtpUsername;
			this.smtpPassword = smtpPassword;

			// SMTP 클라이언트 초기화
			smtpClient = new SmtpClient(smtpServer, smtpPort);
			smtpClient.EnableSsl = true;
			smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
		}

		public string SendMail(string senderEmail, string recipientEmail, string subject, string body)
		{
			try
			{
				// 메일 발송
				MailMessage mail = new MailMessage(senderEmail, recipientEmail, subject, body);
				smtpClient.Send(mail);
				return $"메일 발송 완료: {recipientEmail}";
			}
			catch (Exception ex)
			{
				return $"메일 발송 오류: {ex.Message}";
			}
		}
	}
}
