namespace gcsShared.Data
{
  public class CharacterHistory
  {
    public string type { get; set; }
    public string id { get; set; }
    public string cardID { get; set; }
    public dynamic payload { get; set; }
  }
  public class Character
  {
    public string id { get; set; }
    public CharacterHistory[] history { get; set; }
  }
}