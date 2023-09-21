using DataToolKit.Areas.Identity.Data;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using Azure.Communication.Email;


namespace DataToolKit.DataAccess
{
    public class Helper
    {
        private IConfiguration? _configuration { get; set; }

        public Helper(IConfiguration? configuration)
        {
            _configuration = configuration ?? null;
        }
        public bool isLoggedInUserAdmin(ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated ?? false)
            {
                return user.FindAll(ClaimTypes.Role)
                                .ToList()
                                .Exists(x => x.Value == "Admin");
            }
            return false;
        }

        public async Task sendEmailAsync(int batchId)
        {
            var emailClient = new EmailClient(_configuration?.GetConnectionString("EmailCommunicationServiceConnection"));
            var recipientList = _configuration?.GetSection("EmailService:ReciepientAddressList").Get<List<string>>();
            foreach(var recipient in recipientList)
            {
                await emailClient.SendAsync(
                Azure.WaitUntil.Completed,
                senderAddress: _configuration?.GetSection("EmailService:SenderAddress").Get<string>(),
                recipientAddress: recipient,
                subject: _configuration?.GetSection("EmailService:EmailSubject").Get<string>(),
                htmlContent: @"
                            <html>
                                Hello,<br>
                                <p>New File uploaded by user.<br><br>
                                Batch Id: " + batchId.ToString() + @"<br></p>
                                Thanks
                            </ html >");
            }
        }
    }
}