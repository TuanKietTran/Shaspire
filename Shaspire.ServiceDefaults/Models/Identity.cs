namespace Shaspire.ServiceDefaults.Models;

public class Identity
{
  public Guid Id { get; set; }
  public required Role Role { get; set; }
  public static Identity Create(Guid roleId, Guid? id = null)
  {
    return new Identity
    {
      Id = id ?? Guid.NewGuid(),
      Role = new Role { Id = roleId }
    };
  }
  public static Identity Create(Role role, Guid? id = null)
  {
    return new Identity
    {
      Id = id ?? Guid.NewGuid(),
      Role = role
    };
  }
}

public class Role
{
  public Guid Id { get; set; }
}