using Lincsolution.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryTester
{
    /// <summary>
    /// DashboardView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DashboardView : BaseSubView
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        //private void OnPosTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    if(sender is not TextBox textBox)
        //        return;
        //
        //    bool isNewInput = CoordinateNumPad.Visibility != Visibility.Visible || !ReferenceEquals(CoordinateNumPad.TargetTextBox, textBox);
        //
        //    CoordinateNumPad.TargetTextBox = textBox;
        //    CoordinateNumPad.Visibility = Visibility.Visible;
        //
        //    if(isNewInput)
        //    {
        //        textBox.SelectAll();
        //    }
        //}
    }
}
