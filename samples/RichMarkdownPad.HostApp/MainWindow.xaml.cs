using System.Windows;

namespace RichMarkdownPad.HostApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        EditorControl.DocumentText = "# RichMarkdownPad\n\nType markdown on the left. Preview is rendered on the right.";
    }

    private void EditorControl_OnOpenRequested(object sender, EventArgs e)
    {
        MessageBox.Show(this, "Open action is handled by host app.", "Host Integration");
    }

    private void EditorControl_OnSaveRequested(object sender, EventArgs e)
    {
        MessageBox.Show(this, "Save action is handled by host app.", "Host Integration");
        EditorControl.MarkDocumentSaved();
    }
}