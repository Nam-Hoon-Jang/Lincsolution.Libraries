using System.ComponentModel;
using System.Windows;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 상태별 아이콘, 색상 및 언어 리소스 텍스트를 표시하는 라벨입니다.
    /// EN: A label that displays state-specific icons, colors, and localized resource text.
    /// </summary>
    [LocalizedDescription("상태별 아이콘, 색상 및 언어 리소스 텍스트를 표시하는 라벨입니다.", "A label that displays state-specific icons, colors, and localized resource text.")]
    public class StatusLabel : BasicLabel
    {
        /// <summary>
        /// KO: StatusLabel 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the StatusLabel control.
        /// </summary>
        static StatusLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusLabel), new FrameworkPropertyMetadata(typeof(StatusLabel)));
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string IdleText { get => base.IdleText; set => base.IdleText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string BusyText { get => base.BusyText; set => base.BusyText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string ReadyText { get => base.ReadyText; set => base.ReadyText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string BuildText { get => base.BuildText; set => base.BuildText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string PauseText { get => base.PauseText; set => base.PauseText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string DoneText { get => base.DoneText; set => base.DoneText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string StopText { get => base.StopText; set => base.StopText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string WarningText { get => base.WarningText; set => base.WarningText = value; }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new string ErrorText { get => base.ErrorText; set => base.ErrorText = value; }
    }
}
