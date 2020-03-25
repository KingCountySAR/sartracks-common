using System;

namespace SarData.Server.Apis
{
  public class ApiException : Exception
  {
    public ApiException(int code, string status, string message) : base(message)
    {
      Code = code;
      Status = status;
    }

    public int Code { get; }
    public string Status { get; }
  }
}
