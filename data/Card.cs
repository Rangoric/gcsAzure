namespace gcsShared.Data
{
  public class Card : BasicItem
  {
    public long value { get; set; }

    public string[] tags { get; set; }
    public string[] children { get; set; }
  }
}