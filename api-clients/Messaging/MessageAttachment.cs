using System;
using System.Collections.Generic;
using System.Text;

namespace SarData.Common.Apis.Messaging
{
  public class MessageAttachment
  {
    public string Base64 { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
  }
}
