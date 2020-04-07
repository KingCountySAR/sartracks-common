using System;
using System.ComponentModel.DataAnnotations;

namespace SarData.Common.Apis.Database
{
  public class NameIdPair
  {
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }
  }
}
