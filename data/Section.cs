namespace gcsShared.Data
{
  public class Point
  {
    public long x { get; set; }
    public long y { get; set; }
  }
  public class Section : BasicItem
  {
    public string title { get; set; }
    public Point size { get; set; }
    public Point position { get; set; }
    public string[] tags { get; set; }
  }
}