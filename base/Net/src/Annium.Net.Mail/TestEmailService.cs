using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.Mail
{
    public class TestEmailService : IEmailService
    {
        public IReadOnlyCollection<TestEmail> Emails => emails;
        private readonly List<TestEmail> emails = new List<TestEmail>();

        public Task<IBooleanResult> SendAsync<T>(MailMessage message, string template, T data)
        where T : notnull
        {
            emails.Add(new TestEmail(message, template, data));

            return Task.FromResult(Result.Success());
        }

        public class TestEmail
        {
            public MailMessage Message { get; }
            public string Template { get; }
            public object Data { get; }

            public TestEmail(
                MailMessage message,
                string template,
                object data
            )
            {
                Message = message;
                Template = template;
                Data = data;
            }
        }
    }
}