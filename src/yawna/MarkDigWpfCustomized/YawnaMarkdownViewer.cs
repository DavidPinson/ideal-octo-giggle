using System.Windows;
using Markdig.Wpf;

namespace Yawna.MarkdigWpfCustomized
{
  public class YawnaMarkdownViewer : MarkdownViewer
  {
    public string UCRootPath { get; set; }

    public YawnaMarkdownViewer()
    {
    }

    protected override void RefreshDocument()
    {
      Document = Markdown != null ? Markdig.Wpf.Markdown.ToFlowDocument(Markdown, Pipeline ?? DefaultPipeline, new YawnaWpfRenderer(UCRootPath)) : null;
    }
  }
}