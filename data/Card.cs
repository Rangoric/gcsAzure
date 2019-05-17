namespace gcsShared.Data
{
  public class Card
  {
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }

    public string attribute { get; set; }
    public long value { get; set; }

    public string[] tags { get; set; }
    public string[] children { get; set; }
  }
}