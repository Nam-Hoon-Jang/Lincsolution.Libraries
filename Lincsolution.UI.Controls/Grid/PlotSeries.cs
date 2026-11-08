using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>KO: 시간(초)과 표시값 한 점. null 값은 선의 단절입니다. EN: A time/value point; null breaks the line.</summary>
    public readonly struct PlotPoint
    {
        /// <summary>KO: 공통 기준으로부터 경과 초. EN: Seconds from the shared origin.</summary>
        public double Seconds { get; }
        /// <summary>KO: 표시값 또는 누락. EN: Value or missing sample.</summary>
        public double? Value { get; }
        /// <summary>KO: 점 생성. EN: Creates a point.</summary>
        public PlotPoint(double seconds, double? value) { Seconds=seconds; Value=value; }
    }

    /// <summary>KO: Plot의 센서 선택 및 표시 데이터. EN: Series selection and display data.</summary>
    public sealed class PlotSeries : INotifyPropertyChanged
    {
        private bool visible;
        private IReadOnlyList<PlotPoint> points=Array.Empty<PlotPoint>();
        /// <summary>KO: 센서 식별 ID. EN: Sensor identifier.</summary>
        [LocalizedDescription("센서 식별 ID", "Sensor identifier")]
        public int Id { get; }
        /// <summary>KO: 범례 이름. EN: Legend name.</summary>
        [LocalizedDescription("범례 이름", "Legend name")]
        public string Name { get; }
        /// <summary>KO: 단위. EN: Unit.</summary>
        [LocalizedDescription("단위", "Unit")]
        public string Unit { get; }
        /// <summary>KO: 선 및 범례 색. EN: Line and legend brush.</summary>
        [LocalizedDescription("선 및 범례 색", "Line and legend brush")]
        public Brush Stroke { get; }
        /// <summary>KO: 상태 값을 계단 형태로 표시. EN: Draws state values as steps.</summary>
        [LocalizedDescription("계단형 표시", "Step rendering")]
        public bool IsStep { get; }
        /// <summary>KO: 체크박스 표시 여부. EN: Checkbox selection.</summary>
        [LocalizedDescription("센서 표시 여부", "Series visibility")]
        public bool IsVisible { get=>visible; set { if(visible==value)return; visible=value; Changed(); } }
        /// <summary>KO: 시간순 표시 점. 변경 시 목록 자체를 교체합니다. EN: Ordered points; replace the list to update.</summary>
        [LocalizedDescription("시간순 표시 데이터", "Ordered display points")]
        public IReadOnlyList<PlotPoint> Points { get=>points; set { points=value??Array.Empty<PlotPoint>(); Changed(); } }
        /// <summary>KO: 변경 알림. EN: Property notification.</summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>KO: 센서 표시 모델 생성. EN: Creates a sensor display model.</summary>
        public PlotSeries(int id,string name,string unit,Brush stroke,bool isVisible=false,bool isStep=false)
        { Id=id; Name=name; Unit=unit; Stroke=stroke; visible=isVisible; IsStep=isStep; }
        private void Changed([CallerMemberName]string? property=null)=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(property));
    }
}
