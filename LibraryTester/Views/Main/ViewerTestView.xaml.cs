using System.Windows;

namespace LibraryTester;

public partial class ViewerTestView : System.Windows.Controls.UserControl
{
    public ViewerTestView()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if(DataContext is not PreferenceViewModel viewModel || !viewModel.CanEditViewer)
            return;

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "3D Files (*.3mf;*.stl)|*.3mf;*.stl|" +
                     "3MF Files (*.3mf)|*.3mf|" +
                     "STL Files (*.stl)|*.stl",
            CheckFileExists = true
        };

        if(dialog.ShowDialog() == true && viewModel.LoadViewerModelCommand.CanExecute(dialog.FileName))
            viewModel.LoadViewerModelCommand.Execute(dialog.FileName);
    }
}
