using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lincsolution.Core.Base.Alarms
{

    /// <summary>
    /// UI, WPF, 특정 장비에 종속되지 않는 Alarm의 최소 공통 데이터와 해제 상태를 제공한다.
    /// 제품별 Alarm은 이 클래스를 상속해 장비명, 오류 코드, 복구 정보 등을 추가한다.
    /// </summary>
    public abstract class AlarmBase : INotifyPropertyChanged
    {
        /// <summary>Alarm 해제 여부를 보관한다.</summary>
        private bool _isResolved;

        /// <summary>
        /// 메시지와 중요도를 사용해 Alarm을 생성한다.
        /// </summary>
        /// <param name="message">로그와 UI에 전달할 Alarm 메시지다.</param>
        /// <param name="severity">Alarm의 중요도다.</param>
        /// <exception cref="ArgumentException">메시지가 비어 있을 때 발생한다.</exception>
        protected AlarmBase(string message, AlarmSeverity severity = AlarmSeverity.Error)
        {
            if(string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Alarm 메시지는 비어 있을 수 없습니다.", nameof(message));
            }

            Id = Guid.NewGuid();
            OccurredAt = DateTimeOffset.Now;
            Message = message;
            Severity = severity;
        }

        /// <summary>
        /// Exception의 메시지를 사용해 Alarm을 생성한다.
        /// 원본 Exception은 진단과 로그 기록을 위해 보관한다.
        /// </summary>
        /// <param name="exception">Alarm의 원인이 된 Exception이다.</param>
        /// <param name="severity">Alarm의 중요도다.</param>
        protected AlarmBase(Exception exception, AlarmSeverity severity = AlarmSeverity.Error)
            : this(
                (exception ?? throw new ArgumentNullException(nameof(exception))).Message,
                severity)
        {
            SourceException = exception;
        }

        /// <summary>프로세스 안에서 Alarm을 식별하는 고유 ID다.</summary>
        public Guid Id
        {
            get;
        }

        /// <summary>Alarm이 생성된 로컬 시각과 UTC 오프셋이다.</summary>
        public DateTimeOffset OccurredAt
        {
            get;
        }

        /// <summary>로그와 UI에 전달할 Alarm 메시지다.</summary>
        public string Message
        {
            get;
        }

        /// <summary>Alarm의 중요도다.</summary>
        public AlarmSeverity Severity
        {
            get;
        }

        /// <summary>Alarm의 원인이 된 원본 Exception이다. Exception 없이 생성된 Alarm이면 null이다.</summary>
        public Exception SourceException
        {
            get;
        }

        /// <summary>
        /// Alarm이 해제됐는지 가져오거나 설정한다.
        /// 값이 변경되면 PropertyChanged 이벤트를 발생시킨다.
        /// </summary>
        public bool IsResolved
        {
            get => _isResolved;
            protected set
            {
                if(_isResolved == value)
                {
                    return;
                }

                _isResolved = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Alarm 속성 변경을 구독자에게 전달한다.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Alarm을 해제 상태로 변경한다.
        /// 이미 해제된 Alarm에 다시 호출해도 추가 변경 알림은 발생하지 않는다.
        /// </summary>
        public virtual void Resolve()
        {
            IsResolved = true;
        }

        /// <summary>
        /// 지정한 속성의 변경 사실을 PropertyChanged 구독자에게 전달한다.
        /// </summary>
        /// <param name="propertyName">변경된 속성 이름이다. 생략하면 호출한 멤버 이름을 사용한다.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}