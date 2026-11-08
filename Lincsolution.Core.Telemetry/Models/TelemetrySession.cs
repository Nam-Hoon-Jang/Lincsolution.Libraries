using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// 하나의 Telemetry가 수집하는 Session 정보.
    /// Session ID, Build File 이름, 시작 시각을 포함.
    /// </summary>
    public sealed class TelemetrySession
    {
        /// <summary>Session 고유 ID</summary>
        public Guid SessionId { get; }
        
        /// <summary>Build 파일 이름(확장자 포함)</summary>
        public string BuildFileName { get; }
        
        /// <summary>UTC 기준, Session 시작 시간</summary>
        public DateTimeOffset StartDate { get; }

        /// <summary>
        /// TelemetrySession 생성자.
        /// </summary>
        /// <param name="sessionId">Session 고유 ID</param>
        /// <param name="buildFileName">확장자를 포함한, 빌드 파일 이름</param>
        /// <param name="startDate">Session 시작 시간</param>
        /// <exception cref="ArgumentException"></exception>
        public TelemetrySession(Guid sessionId, string buildFileName, DateTimeOffset startDate)
        {
            /// sessionId, buildFileName 검사.
            if(sessionId == Guid.Empty)
            {
                throw new ArgumentException("Session ID cannot be empty.", nameof(sessionId));
            }

            if(string.IsNullOrWhiteSpace(buildFileName))
            {
                throw new ArgumentException("Build File Name cannot be empty.", nameof(buildFileName));
            }

            SessionId = sessionId;
            BuildFileName = buildFileName;
            StartDate = startDate.ToUniversalTime();
        }
    }
}
