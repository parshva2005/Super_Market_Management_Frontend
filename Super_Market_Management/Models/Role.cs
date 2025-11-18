using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Super_Market_Management.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime? CreationDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public bool? IsRemove { get; set; }
    [JsonIgnore]
    public virtual ICollection<UserLog> UserLogs { get; set; } = new List<UserLog>();
    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
public class RoleDropDown
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
}