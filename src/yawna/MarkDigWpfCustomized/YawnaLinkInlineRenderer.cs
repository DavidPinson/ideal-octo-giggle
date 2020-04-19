using System;
using System.IO;

namespace Yawna.MarkdigWpfCustomized
{
  public class YawnaLinkInlineRenderer : Markdig.Renderers.Wpf.Inlines.LinkInlineRenderer
  {
    private string _linkpath;

    public YawnaLinkInlineRenderer(string linkpath)
    {
      _linkpath = linkpath;
    }

    protected override void Write(Markdig.Renderers.WpfRenderer renderer, Markdig.Syntax.Inlines.LinkInline link)
    {
      if(link.IsImage)
      {
        if(Uri.IsWellFormedUriString(link.Url, UriKind.Absolute) == false)
        {
          link.Url = (new Uri(Path.Combine(_linkpath, link.Url))).ToString();
        }
      }

      base.Write(renderer, link);
    }
  }
}