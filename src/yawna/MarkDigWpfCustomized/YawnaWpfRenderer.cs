namespace Yawna.MarkdigWpfCustomized
{
  public class YawnaWpfRenderer : Markdig.Renderers.WpfRenderer
  {
    private string _linkpath;

    public YawnaWpfRenderer(string linkpath) : base()
    {
      _linkpath = linkpath;
    }

    protected override void LoadRenderers()
    {
      ObjectRenderers.Add(new YawnaLinkInlineRenderer(_linkpath));
      base.LoadRenderers();
    }
  }
}