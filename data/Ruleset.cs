namespace gcsShared.Data
{
  public class Ruleset
  {
    public string id { get; set; }
    public string owner { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public long lastModified { get; set; }
    public Attribute[] attributes { get; set; }
    public Card[] cards { get; set; }
    public Character[] characters { get; set; }
    public Sheet[] sheets { get; set; }
  }
}