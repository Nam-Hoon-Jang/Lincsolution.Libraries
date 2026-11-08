using System.Windows;
using System.Windows.Controls;
using Lincsolution.UI.Base.Common;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 상태에 따라 배경이 바뀌고 투명도를 조절할 수 있는 Grid입니다.
    /// EN: A Grid whose background follows its state and whose opacity can be adjusted.
    /// </summary>
    [LocalizedDescription("상태에 따라 배경이 바뀌고 투명도를 조절할 수 있는 Grid입니다.", "A Grid whose background follows its state and whose opacity can be adjusted.")]
    public class OpacityAdjustableStatusGrid : Grid
    {
        /// <summary>
        /// KO: OpacityAdjustableStatusGrid 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the OpacityAdjustableStatusGrid control.
        /// </summary>
        static OpacityAdjustableStatusGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(OpacityAdjustableStatusGrid),
                new FrameworkPropertyMetadata(typeof(OpacityAdjustableStatusGrid)));
        }

        /// <summary>
        /// KO: Status 종속성 속성 식별자입니다. 컨트롤에 적용할 UI 상태를 가져오거나 설정합니다.
        /// EN: Identifies the Status dependency property. Gets or sets the UI state applied to the control.
        /// </summary>
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                nameof(Status),
                typeof(UiState),
                typeof(OpacityAdjustableStatusGrid),
                new FrameworkPropertyMetadata(UiState.IDLE));

        /// <summary>
        /// KO: 컨트롤에 적용할 UI 상태를 가져오거나 설정합니다.
        /// EN: Gets or sets the UI state applied to the control.
        /// </summary>
        [LocalizedDescription("컨트롤에 적용할 UI 상태를 가져오거나 설정합니다.", "Gets or sets the UI state applied to the control.")]
        public UiState Status
        {
            get => (UiState)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
    }
}
