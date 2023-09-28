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
        private DTKDB _dtkdb {  get; set; }

        public Helper(IConfiguration? configuration)
        {
            _configuration = configuration ?? null;
            _dtkdb = new DTKDB(configuration);

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
            var batchControl = _dtkdb.GetBatchControls().Where(x => x.BatchId == batchId).ToList();
            if(batchControl.Count < 1)
            {
                // No Batch Id Found, Ideally should never happen.
                return;
            }
            foreach(var recipient in recipientList)
            {
                await emailClient.SendAsync(
                Azure.WaitUntil.Completed,
                senderAddress: _configuration?.GetSection("EmailService:SenderAddress").Get<string>(),
                recipientAddress: recipient,
                subject: _configuration?.GetSection("EmailService:EmailSubject").Get<string>(),
                htmlContent: @"
                            <html>
                                Hello,<br><br>
                                <p> We are pleased to inform you that a new file has been successfully uploaded to the Data Tool Kit application.<br><br>
                                File Details:<br><br>
                                Customer: " + batchControl[0].CustomerName + @"<br>
                                File Name: " + batchControl[0].InputFileName + @"<br>
                                Report: " + batchControl[0].ReportTitle + @"<br>
                                Uploaded By: " + batchControl[0].SubmitName + @"<br>
                                Batch Id: " + batchId.ToString() + @"<br><br> 
                                You can access this file by logging into your https://dtkpocn.azurewebsites.net/ account.<br><br></p>
                                Best regards<br>
                                POCN<br>
                                Data Tool Kit application.
                            </ html >");
            }
        }
    }
}