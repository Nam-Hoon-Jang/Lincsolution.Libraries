using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 현재 페이지와 전체 페이지를 표시하고 이전 및 다음 페이지로 이동합니다.
    /// EN: Displays the current and total page count and navigates between pages.
    /// </summary>
    public class PaginationControl : Control
    {
        public static readonly RoutedCommand PreviousPageCommand = new(nameof(PreviousPageCommand), typeof(PaginationControl));
        public static readonly RoutedCommand NextPageCommand = new(nameof(NextPageCommand), typeof(PaginationControl));

        static PaginationControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaginationControl), new FrameworkPropertyMetadata(typeof(PaginationControl)));
        }

        public PaginationControl()
        {
            CommandBindings.Add(new CommandBinding(PreviousPageCommand, (_, _) => CurrentPage--, (_, e) => e.CanExecute = CurrentPage > 1));
            CommandBindings.Add(new CommandBinding(NextPageCommand, (_, _) => CurrentPage++, (_, e) => e.CanExecute = CurrentPage < PageCount));
        }

        /// <summary>
        /// KO: CurrentPage 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the CurrentPage dependency property. Gets or sets the CurrentPage value.
        /// </summary>
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceCurrentPage));
        /// <summary>
        /// KO: PageCount 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the PageCount dependency property. Gets or sets the PageCount value.
        /// </summary>
        public static readonly DependencyProperty PageCountProperty =
            DependencyProperty.Register(nameof(PageCount), typeof(int), typeof(PaginationControl),
                new FrameworkPropertyMetadata(1, OnPageCountChanged, CoercePageCount));
        /// <summary>
        /// KO: ButtonSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ButtonSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register(nameof(ButtonSize), typeof(double), typeof(PaginationControl), new FrameworkPropertyMetadata(32d));
        /// <summary>
        /// KO: ItemSpacing 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ItemSpacing dependency property. Gets or sets the spacing.
        /// </summary>
        public static readonly DependencyProperty ItemSpacingProperty =
            DependencyProperty.Register(nameof(ItemSpacing), typeof(double), typeof(PaginationControl), new FrameworkPropertyMetadata(8d));

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the CurrentPage value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the CurrentPage value.")]
        public int CurrentPage { get => (int)GetValue(CurrentPageProperty); set => SetValue(CurrentPageProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the PageCount value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the PageCount value.")]
        public int PageCount { get => (int)GetValue(PageCountProperty); set => SetValue(PageCountProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double ButtonSize { get => (double)GetValue(ButtonSizeProperty); set => SetValue(ButtonSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the spacing.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the spacing.")]
        public double ItemSpacing { get => (double)GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }

        /// <summary>
        /// KO: CoerceCurrentPage 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by CoerceCurrentPage.
        /// </summary>
        private static object CoerceCurrentPage(DependencyObject d, object value) =>
            Math.Clamp((int)value, 1, Math.Max(1, ((PaginationControl)d).PageCount));

        /// <summary>
        /// KO: CoercePageCount 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by CoercePageCount.
        /// </summary>
        private static object CoercePageCount(DependencyObject d, object value) => Math.Max(1, (int)value);

        /// <summary>
        /// KO: OnPageCountChanged 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by OnPageCountChanged.
        /// </summary>
        private static void OnPageCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            d.CoerceValue(CurrentPageProperty);
    }
}
