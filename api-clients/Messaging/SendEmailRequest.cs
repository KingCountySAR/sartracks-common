using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SarData.Common.Apis.Messaging
{
  public class SendEmailRequest
  {
    [Required]
    [EmailAddress]
    public string To { get; set; }

    [Required]
    [MaxLength(100)]
    public string Subject { get; set; }

    [Required]
    public string Message { get; set; }

    public List<MessageAttachment> Attachments { get; set; }
  }
}
