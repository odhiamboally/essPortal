namespace ESSPortal.Infrastructure.Utilities;

public static class EmailTemplates
{
    public static string GetPasswordResetEmailTemplate(string firstName, string resetUrl, string logoUrl = "")
    {
        return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Password Reset - ESS Portal</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            background-color: #f8f9fa;
                            margin: 0;
                            padding: 20px;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background: white;
                            border-radius: 16px;
                            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }}
                        .header {{
                            background: linear-gradient(135deg, #009edb 0%, #5b92e5 100%);
                            color: white;
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .logo {{
                            width: 64px;
                            height: 64px;
                            margin-bottom: 16px;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 24px;
                            font-weight: 700;
                        }}
                        .header p {{
                            margin: 8px 0 0 0;
                            opacity: 0.9;
                            font-size: 14px;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .greeting {{
                            font-size: 16px;
                            margin-bottom: 20px;
                        }}
                        .message {{
                            font-size: 14px;
                            margin-bottom: 30px;
                            line-height: 1.6;
                        }}
                        .button {{
                            display: inline-block;
                            background: #dc3545;
                            color: white;
                            padding: 16px 32px;
                            text-decoration: none;
                            border-radius: 8px;
                            font-weight: 600;
                            font-size: 14px;
                            margin-bottom: 30px;
                        }}
                        .button:hover {{
                            background: #c82333;
                        }}
                        .security-notice {{
                            background: #fff3cd;
                            border: 1px solid #ffeaa7;
                            border-radius: 8px;
                            padding: 16px;
                            margin-bottom: 20px;
                        }}
                        .security-notice h3 {{
                            color: #856404;
                            margin: 0 0 8px 0;
                            font-size: 14px;
                        }}
                        .security-notice p {{
                            color: #856404;
                            margin: 0;
                            font-size: 13px;
                        }}
                        .security-notice ul {{
                            color: #856404;
                            margin: 8px 0 0 16px;
                            padding: 0;
                            font-size: 13px;
                        }}
                        .footer {{
                            background: #f8f9fa;
                            padding: 20px 30px;
                            text-align: center;
                            font-size: 12px;
                            color: #6c757d;
                        }}
                        .link-text {{
                            word-break: break-all;
                            background: #f8f9fa;
                            padding: 12px;
                            border-radius: 4px;
                            font-family: monospace;
                            font-size: 12px;
                            margin-top: 16px;
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            {(string.IsNullOrWhiteSpace(logoUrl) ? "" : $@"<img src=""{logoUrl}"" alt=""United Nations"" class=""logo"" />")}
                            <h1>ESS Portal</h1>
                            <p>Building a better world together</p>
                        </div>
                        
                        <div class=""content"">
                            <div class=""greeting"">
                                Hello {firstName ?? "there"},
                            </div>
                            
                            <div class=""message"">
                                You requested a password reset for your ESS Portal account. Click the button below to create a new password:
                            </div>
                            
                            <div style=""text-align: center;"">
                                <a href=""{resetUrl}"" class=""button"">Reset Your Password</a>
                            </div>
                            
                            <div class=""security-notice"">
                                <h3>🔒 Security Notice</h3>
                                <p><strong>Important:</strong></p>
                                <ul>
                                    <li>This link will expire in <strong>1 hour</strong></li>
                                    <li>If you didn't request this reset, you can safely ignore this email</li>
                                    <li>Never share this link with anyone</li>
                                    <li>For security, this link can only be used once</li>
                                </ul>
                            </div>
                            
                            <div class=""message"">
                                If the button above doesn't work, you can copy and paste this link into your browser:
                                <div class=""link-text"">{resetUrl}</div>
                            </div>
                            
                            <div class=""message"">
                                If you continue to have problems or didn't request this reset, please contact IT support immediately.
                            </div>
                        </div>
                        
                        <div class=""footer"">
                            <p>&copy; {DateTime.Now.Year} United Nations. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";
    }

    public static string GetPasswordResetConfirmationEmailTemplate(string firstName)
    {
        return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""utf-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Password Reset Successful - ESS Portal</title>
                    <style>
                        body {{
                            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                            line-height: 1.6;
                            color: #333;
                            background-color: #f8f9fa;
                            margin: 0;
                            padding: 20px;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background: white;
                            border-radius: 16px;
                            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }}
                        .header {{
                            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
                            color: white;
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 24px;
                            font-weight: 700;
                        }}
                        .header p {{
                            margin: 8px 0 0 0;
                            opacity: 0.9;
                            font-size: 14px;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .success-icon {{
                            text-align: center;
                            font-size: 48px;
                            margin-bottom: 20px;
                        }}
                        .greeting {{
                            font-size: 16px;
                            margin-bottom: 20px;
                        }}
                        .message {{
                            font-size: 14px;
                            margin-bottom: 20px;
                            line-height: 1.6;
                        }}
                        .security-notice {{
                            background: #fff3cd;
                            border: 1px solid #ffeaa7;
                            border-radius: 8px;
                            padding: 16px;
                            margin-bottom: 20px;
                        }}
                        .security-notice h3 {{
                            color: #856404;
                            margin: 0 0 8px 0;
                            font-size: 14px;
                        }}
                        .security-notice p {{
                            color: #856404;
                            margin: 0;
                            font-size: 13px;
                        }}
                        .footer {{
                            background: #f8f9fa;
                            padding: 20px 30px;
                            text-align: center;
                            font-size: 12px;
                            color: #6c757d;
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h1>Password Reset Successful</h1>
                            <p>Your account is now secure</p>
                        </div>
                        
                        <div class=""content"">
                            <div class=""success-icon"">✅</div>
                            
                            <div class=""greeting"">
                                Hello {firstName ?? "there"},
                            </div>
                            
                            <div class=""message"">
                                Your password has been successfully reset for your ESS Portal account.
                            </div>
                            
                            <div class=""message"">
                                You can now sign in to your account using your new password.
                            </div>
                            
                            <div class=""security-notice"">
                                <h3>🔒 Security Notice</h3>
                                <p>If you did not request this password reset, please contact IT support immediately. Your account security is important to us.</p>
                            </div>
                            
                            <div class=""message"">
                                Thank you for keeping your account secure.
                            </div>
                        </div>
                        
                        <div class=""footer"">
                            <p>&copy; {DateTime.Now.Year} United Nations. All rights reserved.</p>
                            <p>This is an automated message, please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";
    }


}
