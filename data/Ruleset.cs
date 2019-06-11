namespace gcsShared.Data
{
  public class Ruleset : BasicItem
  {
    public string owner { get; set; }
    public Card[] cards { get; set; }
    public Character[] characters { get; set; }
    public Sheet[] sheets { get; set; }
  }
}