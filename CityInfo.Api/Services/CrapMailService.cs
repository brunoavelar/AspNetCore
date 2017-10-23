using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.Api.Services
{
    public interface IMailService
    {
        void Send(string subject, string message);
    }

    public class CrapMailService : IMailService
    {
        private string _mailTo = "admin@fakeemailaddress.com";
        private string _mailFrom = "noreply@fakeemailaddress.com";

        public void Send(string subject, string message)
        {
            Debug.WriteLine($"Mail from {_mailFrom} to {_mailTo} using CrapMailService");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Message: {message}");
        }
    }
}
