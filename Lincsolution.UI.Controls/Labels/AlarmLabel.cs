using System.Windows;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 알람 종류에 맞는 색상을 표시하는 라벨입니다.
    /// EN: A label that displays colors appropriate to the alarm type.
    /// </summary>
    [LocalizedDescription("알람 종류에 맞는 색상을 표시하는 라벨입니다.", "A label that displays colors appropriate to the alarm type.")]
    public class AlarmLabel : StatusLabel
    {
        /// <summary>
        /// KO: AlarmLabel 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the AlarmLabel control.
        /// </summary>
        static AlarmLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AlarmLabel),
                new FrameworkPropertyMetadata(typeof(AlarmLabel)));
        }
    }
}
