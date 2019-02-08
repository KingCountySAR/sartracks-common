namespace SarData.Common.Apis.Messaging
{
  public class SendEmailRequest
  {
    public string To { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
  }
}
