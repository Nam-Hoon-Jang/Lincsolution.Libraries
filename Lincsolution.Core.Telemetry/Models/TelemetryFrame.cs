using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// 동일한 수집 주기에 취합한 TelemetryData Set
    /// TelemetryChannel이 100개라면, TelemetryFrame에는 100개의 TelemetryData가 들어있어야 함.
    /// </summary>
    public sealed class TelemetryFrame
    {
        /// <summary>Session 내부에서의 수집 순번</summary>
        public long FrameNumber { get; }

        /// <summary>Telemetry Data Set 수집 시각</summary>
        public TelemetryTimestamp Timestamp  { get; }

        /// <summary>Channel별로 수집한 TelemetryData List</summary>
        public IReadOnlyList<TelemetryData> Data  { get; }

        /// <summary>
        /// TelemetryFrame 생성자
        /// </summary>
        /// <param name="frameNumber">Session 내부에서의 수집 순번</param>
        /// <param name="timestamp">Telemetry Data Set 수집 시각</param>
        /// <param name="data">Channel별로 수집한 TelemetryData List</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public TelemetryFrame(long frameNumber, TelemetryTimestamp timestamp, IReadOnlyList<TelemetryData> data)
        {
            /// TelemetryFrame의 수집 순번은 반드시 0 이상.
            if(frameNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameNumber), "Frame number must be zero or greater.");
            }
            /// Timestamp는 반드시 정의되야 함
            if(timestamp is null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }
            /// Data는 반드시 정의되야 함.
            if(data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            /// 호출자가 원본 List를 변경해도 영향받지 않게 복사.
            var copiedData = new List<TelemetryData>();
            var channelIds = new HashSet<int>();

            foreach(TelemetryData item in data)
            {
                if(item is null)
                {
                    throw new ArgumentException("Data must not contain null items.", nameof(data));
                }

                if(!channelIds.Add(item.ChannelId))
                {
                    throw new ArgumentException("Frame must not contain duplicate channel IDs.", nameof(data));
                }

                copiedData.Add(item);
            }

            if(copiedData.Count == 0)
            {
                throw new ArgumentException("Frame must contain at least one data item.", nameof(data));
            }

            FrameNumber = frameNumber;
            Timestamp = timestamp;
            Data = new ReadOnlyCollection<TelemetryData>(copiedData);
        }
    }
}
