using System.Windows;
using System.Windows.Controls;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 헤더, 전체 선택, 빈 목록 표시 및 행 선택 체크박스를 제공하는 목록입니다.
    /// EN: A list view providing a header, select-all, empty state, and row selection check boxes.
    /// </summary>
    public class BasicListView : ListView
    {
        private bool _updatingSelection;

        static BasicListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicListView), new FrameworkPropertyMetadata(typeof(BasicListView)));
        }

        public BasicListView()
        {
            SelectionChanged += (_, _) => UpdateSelectAllState();
        }

        /// <summary>
        /// KO: Header 종속성 속성 식별자입니다. 가져오거나 설정합니다.
        /// EN: Identifies the Header dependency property. Gets or sets the display content.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(BasicListView));
        /// <summary>
        /// KO: EmptyContent 종속성 속성 식별자입니다. 가져오거나 설정합니다.
        /// EN: Identifies the EmptyContent dependency property. Gets or sets the display content.
        /// </summary>
        public static readonly DependencyProperty EmptyContentProperty =
            DependencyProperty.Register(nameof(EmptyContent), typeof(object), typeof(BasicListView), new FrameworkPropertyMetadata("No items"));
        /// <summary>
        /// KO: IsCheckBoxVisible 종속성 속성 식별자입니다. 가져오거나 설정합니다.
        /// EN: Identifies the IsCheckBoxVisible dependency property. Gets or sets the IsCheckBoxVisible value.
        /// </summary>
        public static readonly DependencyProperty IsCheckBoxVisibleProperty =
            DependencyProperty.Register(nameof(IsCheckBoxVisible), typeof(bool), typeof(BasicListView), new FrameworkPropertyMetadata(true));
        /// <summary>
        /// KO: IsSelectAllChecked 종속성 속성 식별자입니다. 가져오거나 설정합니다.
        /// EN: Identifies the IsSelectAllChecked dependency property. Gets or sets the IsSelectAllChecked value.
        /// </summary>
        public static readonly DependencyProperty IsSelectAllCheckedProperty =
            DependencyProperty.Register(nameof(IsSelectAllChecked), typeof(bool?), typeof(BasicListView),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectAllChanged));
        /// <summary>
        /// KO: ItemHeight 종속성 속성 식별자입니다. 가져오거나 설정합니다.
        /// EN: Identifies the ItemHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(BasicListView), new FrameworkPropertyMetadata(44d));
        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(BasicListView), new FrameworkPropertyMetadata(new CornerRadius(5)));
        /// <summary>
        /// KO: HeaderHeight 종속성 속성 식별자입니다. 가져오거나 설정합니다.
        /// EN: Identifies the HeaderHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(nameof(HeaderHeight), typeof(double), typeof(BasicListView), new FrameworkPropertyMetadata(40d));

        /// <summary>
        /// KO: 가져오거나 설정합니다.
        /// EN: Gets or sets the display content.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the display content.")]
        public object? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
        /// <summary>
        /// KO: 가져오거나 설정합니다.
        /// EN: Gets or sets the display content.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the display content.")]
        public object? EmptyContent { get => GetValue(EmptyContentProperty); set => SetValue(EmptyContentProperty, value); }
        /// <summary>
        /// KO: 가져오거나 설정합니다.
        /// EN: Gets or sets the IsCheckBoxVisible value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the IsCheckBoxVisible value.")]
        public bool IsCheckBoxVisible { get => (bool)GetValue(IsCheckBoxVisibleProperty); set => SetValue(IsCheckBoxVisibleProperty, value); }
        /// <summary>
        /// KO: 가져오거나 설정합니다.
        /// EN: Gets or sets the IsSelectAllChecked value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the IsSelectAllChecked value.")]
        public bool? IsSelectAllChecked { get => (bool?)GetValue(IsSelectAllCheckedProperty); set => SetValue(IsSelectAllCheckedProperty, value); }
        /// <summary>
        /// KO: 가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double ItemHeight { get => (double)GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }
        /// <summary>
        /// KO: 가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the corner radius.")]
        public CornerRadius CornerRadius { get => (CornerRadius)GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }
        /// <summary>
        /// KO: 가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double HeaderHeight { get => (double)GetValue(HeaderHeightProperty); set => SetValue(HeaderHeightProperty, value); }

        /// <summary>
        /// KO: OnSelectAllChanged 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by OnSelectAllChanged.
        /// </summary>
        private static void OnSelectAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = (BasicListView)d;
            if (list._updatingSelection || e.NewValue is not bool isChecked)
                return;

            list._updatingSelection = true;
            if (isChecked)
                list.SelectAll();
            else
                list.UnselectAll();
            list._updatingSelection = false;
        }

        /// <summary>
        /// KO: UpdateSelectAllState 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by UpdateSelectAllState.
        /// </summary>
        private void UpdateSelectAllState()
        {
            if (_updatingSelection)
                return;

            _updatingSelection = true;
            SetCurrentValue(IsSelectAllCheckedProperty,
                Items.Count == 0 ? false :
                SelectedItems.Count == 0 ? false :
                SelectedItems.Count == Items.Count ? true : null);
            _updatingSelection = false;
        }
    }
}
