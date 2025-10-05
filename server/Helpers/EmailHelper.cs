using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace server_NET.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailHelper> _logger;

        public EmailHelper(IConfiguration configuration, ILogger<EmailHelper> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendWinnerNotificationAsync(string toEmail, string winnerName, string giftName, decimal giftPrice)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = smtpSettings["SmtpHost"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var smtpUser = smtpSettings["SmtpUser"];
                var smtpPassword = smtpSettings["SmtpPassword"];
                var fromEmail = smtpSettings["FromEmail"];
                var fromName = smtpSettings["FromName"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email configuration is missing. Cannot send winner notification.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    EnableSsl = true
                };

                var subject = "🎉 Congratulations! You Won the Lottery! 🎉";
                var body = CreateWinnerEmailBody(winnerName, giftName, giftPrice);

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUser, fromName ?? "מכירה סינית"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Winner notification email sent successfully to {Email} for gift {GiftName}", toEmail, giftName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send winner notification email to {Email} for gift {GiftName}", toEmail, giftName);
                return false;
            }
        }

        private string CreateWinnerEmailBody(string winnerName, string giftName, decimal giftPrice)
        {
            return $@"
<!DOCTYPE html>
<html dir='ltr' lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Lottery Winner - Chinese Auction</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 20px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            direction: ltr;
            min-height: 100vh;
        }}
        .email-wrapper {{
            max-width: 650px;
            margin: 40px auto;
            background: #ffffff;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 30px;
            text-align: center;
            color: white;
            position: relative;
        }}
        .header::before {{
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: url('data:image/svg+xml,<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 100 100""><defs><pattern id=""confetti"" x=""0"" y=""0"" width=""20"" height=""20"" patternUnits=""userSpaceOnUse""><circle cx=""5"" cy=""5"" r=""1"" fill=""rgba(255,255,255,0.1)""/><circle cx=""15"" cy=""15"" r=""1"" fill=""rgba(255,255,255,0.1)""/></pattern></defs><rect width=""100"" height=""100"" fill=""url(%23confetti)""/></svg>');
            opacity: 0.3;
        }}
        .trophy-container {{
            position: relative;
            z-index: 2;
            margin-bottom: 20px;
        }}
        .trophy {{
            font-size: 5rem;
            margin-bottom: 10px;
            display: block;
            text-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
            animation: bounce 2s infinite;
        }}
        @keyframes bounce {{
            0%, 20%, 50%, 80%, 100% {{ transform: translateY(0); }}
            40% {{ transform: translateY(-10px); }}
            60% {{ transform: translateY(-5px); }}
        }}
        .header h1 {{
            font-size: 2.5rem;
            font-weight: 800;
            margin-bottom: 10px;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
            position: relative;
            z-index: 2;
        }}
        .header h2 {{
            font-size: 1.3rem;
            font-weight: 400;
            opacity: 0.95;
            position: relative;
            z-index: 2;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .greeting {{
            font-size: 1.2rem;
            color: #2c3e50;
            margin-bottom: 30px;
            font-weight: 600;
        }}
        .congratulations-banner {{
            background: linear-gradient(135deg, #ff6b6b 0%, #feca57 100%);
            color: white;
            padding: 25px;
            border-radius: 15px;
            text-align: center;
            margin: 30px 0;
            font-size: 1.4rem;
            font-weight: 700;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
            position: relative;
            overflow: hidden;
        }}
        .congratulations-banner::before {{
            content: '🎉';
            position: absolute;
            left: 20px;
            top: 50%;
            transform: translateY(-50%);
            font-size: 2rem;
        }}
        .congratulations-banner::after {{
            content: '🎉';
            position: absolute;
            right: 20px;
            top: 50%;
            transform: translateY(-50%);
            font-size: 2rem;
        }}
        .prize-card {{
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border: 2px solid #e9ecef;
            border-radius: 15px;
            padding: 30px;
            margin: 30px 0;
            text-align: center;
            position: relative;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
        }}
        .prize-label {{
            font-size: 1rem;
            color: #6c757d;
            text-transform: uppercase;
            letter-spacing: 1px;
            font-weight: 600;
            margin-bottom: 15px;
        }}
        .prize-name {{
            font-size: 2rem;
            font-weight: 800;
            color: #2c3e50;
            margin-bottom: 15px;
            line-height: 1.2;
        }}
        .prize-value {{
            font-size: 1.8rem;
            color: #27ae60;
            font-weight: 700;
            background: linear-gradient(135deg, #27ae60 0%, #2ecc71 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }}
        .prize-card::before {{
            content: '✨';
            position: absolute;
            top: 15px;
            right: 15px;
            font-size: 1.5rem;
        }}
        .info-section {{
            background: #f8f9fa;
            border-left: 4px solid #667eea;
            padding: 20px;
            border-radius: 0 10px 10px 0;
            margin: 30px 0;
        }}
        .info-section p {{
            margin: 0;
            color: #495057;
            font-size: 1.1rem;
            line-height: 1.7;
        }}
        .next-steps {{
            background: linear-gradient(135deg, #e3f2fd 0%, #f3e5f5 100%);
            border-radius: 15px;
            padding: 25px;
            margin: 30px 0;
        }}
        .next-steps h3 {{
            color: #667eea;
            font-size: 1.3rem;
            margin-bottom: 15px;
            font-weight: 700;
        }}
        .next-steps ul {{
            list-style: none;
            padding: 0;
        }}
        .next-steps li {{
            padding: 8px 0;
            color: #495057;
            font-weight: 500;
            position: relative;
            padding-left: 30px;
        }}
        .next-steps li::before {{
            content: '✓';
            position: absolute;
            left: 0;
            color: #27ae60;
            font-weight: bold;
            font-size: 1.2rem;
        }}
        .footer {{
            background: #f8f9fa;
            padding: 30px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }}
        .footer .company-name {{
            font-size: 1.3rem;
            font-weight: 700;
            color: #667eea;
            margin-bottom: 10px;
        }}
        .footer .signature {{
            color: #6c757d;
            font-size: 1rem;
            margin-bottom: 15px;
        }}
        .footer .disclaimer {{
            font-size: 0.85rem;
            color: #adb5bd;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid #dee2e6;
        }}
        .social-links {{
            margin: 20px 0;
        }}
        .social-links a {{
            display: inline-block;
            margin: 0 10px;
            padding: 10px;
            background: #667eea;
            color: white;
            border-radius: 50%;
            text-decoration: none;
            width: 40px;
            height: 40px;
            line-height: 20px;
            text-align: center;
        }}
        @media (max-width: 600px) {{
            .email-wrapper {{
                margin: 10px;
                border-radius: 15px;
            }}
            .header {{
                padding: 30px 20px;
            }}
            .header h1 {{
                font-size: 2rem;
            }}
            .content {{
                padding: 30px 20px;
            }}
            .prize-name {{
                font-size: 1.5rem;
            }}
            .congratulations-banner {{
                font-size: 1.2rem;
                padding: 20px;
            }}
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <div class='trophy-container'>
                <span class='trophy'>🏆</span>
            </div>
            <h1>🎊 CONGRATULATIONS! 🎊</h1>
            <h2>You Are Our Lucky Winner!</h2>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                Dear {winnerName},
            </div>
            
            <div class='congratulations-banner'>
                We are thrilled to announce that you have won our lottery!
            </div>
            
            <div class='prize-card'>
                <div class='prize-label'>Your Amazing Prize</div>
                <div class='prize-name'>{giftName}</div>
                <div class='prize-value'>Worth ₪{giftPrice:N0}</div>
            </div>
            
            <div class='info-section'>
                <p>
                    <strong>🎯 What happens next?</strong><br>
                    Our team will contact you within 24-48 hours to coordinate the prize delivery or pickup. 
                    Please keep this email as proof of your win.
                </p>
            </div>
            
            <div class='next-steps'>
                <h3>📋 Next Steps:</h3>
                <ul>
                    <li>Keep this email safe as confirmation</li>
                    <li>Wait for our call within 24-48 hours</li>
                    <li>Prepare a valid ID for prize collection</li>
                    <li>Share the exciting news with friends & family!</li>
                </ul>
            </div>
            
            <p style='color: #495057; font-size: 1.1rem; line-height: 1.7; margin: 30px 0;'>
                Thank you for participating in our Chinese Auction. Your support makes our community events possible, 
                and we're delighted that you're our lucky winner this time!
            </p>
        </div>
        
        <div class='footer'>
            <div class='company-name'>🎪 Chinese Auction Team</div>
            <div class='signature'>Making Dreams Come True, One Draw at a Time</div>
            
            <div class='social-links'>
                <a href='#'>📧</a>
                <a href='#'>📱</a>
                <a href='#'>🌐</a>
            </div>
            
            <div class='disclaimer'>
                This email was sent automatically from our lottery system. Please do not reply to this email. 
                For any questions, please contact our support team.<br>
                <strong>Winner Notification #{DateTime.Now:yyyyMMddHHmmss}</strong>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        public async Task<bool> SendTestEmailAsync(string toEmail)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = smtpSettings["SmtpHost"];
                var smtpPort = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                var smtpUser = smtpSettings["SmtpUser"];
                var smtpPassword = smtpSettings["SmtpPassword"];
                var fromEmail = smtpSettings["FromEmail"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Email configuration is missing. Cannot send test email.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? smtpUser, "Chinese Auction"),
                    Subject = "Email System Test",
                    Body = "This is a test email from the Chinese Auction system.",
                    IsBodyHtml = false
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Test email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", toEmail);
                return false;
            }
        }
    }
}
