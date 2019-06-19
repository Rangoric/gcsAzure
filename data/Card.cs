namespace gcsShared.Data
{
  public class CardRequirement
  {
    public string tag { get; set; }
    public long value { get; set; }
  }
  public class Card : BasicItem
  {
    public CardRequirement[] requirements { get; set; }
    public long value { get; set; }

    public string[] tags { get; set; }
    public string[] children { get; set; }
  }
}