﻿using System.IO;
using System.Windows;
using Markdig;
using Markdig.Wpf;

namespace yawn
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
    {
    private bool useExtensions = true;

    public MainWindow()
    {
      InitializeComponent();
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      Viewer.Markdown = File.ReadAllText("Documents/Markdig-readme.md");
    }

    private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      //Process.Start(e.Parameter.ToString());
      Viewer.Markdown = File.ReadAllText(e.Parameter.ToString());
    }

    private void ClickOnImage(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
      MessageBox.Show($"URL: {e.Parameter}");
    }

    private void ToggleExtensionsButton_OnClick(object sender, RoutedEventArgs e)
    {
      useExtensions = !useExtensions;
      Viewer.Pipeline = useExtensions ? new MarkdownPipelineBuilder().UseSupportedExtensions().Build() : new MarkdownPipelineBuilder().Build();
    }
  }
}
