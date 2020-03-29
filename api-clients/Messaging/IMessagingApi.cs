﻿using System.Threading.Tasks;
using Refit;
using SarData.Common.Apis.Health;

namespace SarData.Common.Apis.Messaging
{
  public interface IMessagingApi : IHealthDependencyApi
  {
    [Post("/send/email")]
    Task SendEmail([Body] SendEmailRequest email);

    [Post("/send/text")]
    Task SendText(string phone, string message);
  }

  public static class MessagingApiExtensions
  {
    public static Task SendEmail(this IMessagingApi api, string to, string subject, string message)
    {
      return api.SendEmail(new SendEmailRequest { To = to, Subject = subject, Message = message });
    }
  }
}
