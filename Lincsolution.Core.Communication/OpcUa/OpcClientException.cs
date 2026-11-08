using System;
using System.Diagnostics;
using Opc.Ua;

namespace Lincsolution.Core.Communication.OpcUa
{
    public enum OpcClientExceptionType
    {
        UnknownException,               //알 수 없는 오류
        isNullSession,                  //세션이 null 일 때
        SessionNotConnected,            //세션 연결 안 됐을 때
        CertificateCreateFailed,        //인증서 생성 실패
        SubscriptionCreateFailed,       //구독 생성 실패
        ReconnectFailed,                //재연결 실패
        WriteFailed,                    //노드 쓰기 실패
        MonitoredItemNotFound,          //모니터링 항목 찾기 실패
        WrongData,                      //입력 데이터 이상
        ServerCertificateRejected,      //서버 인증서 거절 실패
        BadCertificateUntrusted,        //인증서 파일 에러
        BadCertificateUriInvalid,       //인증서 파일 내 URI 이상
        isNullReconnectHandler,         //ReconnectHandler null 일 때
        ServiceResultException,         //ServiceResultException 발생 시
    }
    public class OpcClientException : Exception
    {
        public OpcClientExceptionType ExceptionType { get; }
        public StatusCode? StatusCode { get; }
        public string DiagnosticInfo { get; } = string.Empty;

        public OpcClientException(OpcClientExceptionType type, string message = null) : base(message)
        {
            ExceptionType = type;
            if(message != null)
            {
                Debug.WriteLine($"Exception: OpcClientException => {message}");
            }
        }

        public OpcClientException(OpcClientExceptionType type, Exception innerException) : base(innerException?.Message, innerException)
        {
            ExceptionType = type;
            if(innerException is ServiceResultException serviceResultException)
            {
                StatusCode = serviceResultException.StatusCode;
                DiagnosticInfo = serviceResultException.Result?.ToLongString();
            }
            Debug.WriteLine(ToDiagnosticsString());
        }

        public OpcClientException(OpcClientExceptionType type, string message, Exception innerException) : base(message, innerException)
        {
            ExceptionType = type;
            if(innerException is ServiceResultException serviceResultException)
            {
                StatusCode = serviceResultException.StatusCode;
                DiagnosticInfo = serviceResultException.Result?.ToLongString();
            }
            Debug.WriteLine(ToDiagnosticsString());
        }

        public string ToDiagnosticsString()
        {
            string status = StatusCode.HasValue ? $", StatusCode={StatusCode.Value}" : string.Empty;
            string diagnostics = string.IsNullOrWhiteSpace(DiagnosticInfo) ? string.Empty : $", Diagnostics={DiagnosticInfo}";
            string innerEx = InnerException == null ? string.Empty : $", InnerException={InnerException.GetType().FullName}: {InnerException.Message}";
            return $"Exception: OpcClientException => Type={ExceptionType}, Message={Message}{status}{diagnostics}{innerEx}";
        }
    }
}
