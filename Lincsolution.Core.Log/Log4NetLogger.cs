using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using Lincsolution.Core.Base.Logging;

namespace Lincsolution.Core.Logger
{
    /// <summary>
    /// KO: log4net 기반의 애플리케이션 로그 기록기를 제공합니다.
    /// EN: Provides an application logger backed by log4net.
    /// </summary>
    public sealed class Log4NetLogger : ILogger
    {
        #region Private Variables

        /// <summary>log4net Logger 인터페이스</summary>
        private readonly ILog logger;

        /// <summary>log4net.config에 정의한 logger 이름</summary>
        private readonly string appenderName;

        /// <summary>Logger 초기화 시 중복 방지</summary>
        private static readonly object InitializationLock = new object();

        /// <summary>log4net.config 파일 경로</summary>
        private static string initializedConfigPath;

        #endregion

        /// <summary>
        /// KO: log4net 설정을 검증하고 지정한 logger를 생성합니다.
        /// EN: Validates log4net configuration and creates the specified logger.
        /// </summary>
        /// <param name="configFilePath">KO: log4net 설정 파일 경로입니다. EN: The log4net configuration file path.</param>
        /// <param name="logName">KO: logger 이름입니다. EN: The logger name.</param>
        /// <param name="appenderName">KO: 파일 appender 이름입니다. EN: The file appender name.</param>
        public Log4NetLogger(string configFilePath, string logName, string appenderName)
        {
            Initialise(configFilePath);

            if (string.IsNullOrWhiteSpace(logName))
                throw new ArgumentException("Logger name cannot be empty.", nameof(logName));

            if (string.IsNullOrWhiteSpace(appenderName))
                throw new ArgumentException("Appender name cannot be empty.", nameof(appenderName));

            this.appenderName = appenderName;
            logger = LogManager.GetLogger(logName);
        }

        /// <summary>
        /// KO: log4net 설정 파일을 검증하고 현재 repository를 초기화합니다.
        /// EN: Validates the log4net configuration file and initializes the current repository.
        /// </summary>
        /// <param name="configFilePath">KO: log4net 설정 파일 경로입니다. EN: The log4net configuration file path.</param>
        public static void Initialise(string configFilePath)
        {
            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new ArgumentException("Configuration file path cannot be empty.", nameof(configFilePath));

            string fullConfigPath = Path.GetFullPath(configFilePath);
            if (!File.Exists(fullConfigPath))
                throw new FileNotFoundException("log4net configuration file was not found.", fullConfigPath);

            lock (InitializationLock)
            {
                if (initializedConfigPath != null)
                {
                    if (!string.Equals(initializedConfigPath, fullConfigPath, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("log4net has already been initialized with a different configuration file.");

                    return;
                }

                var repository = LogManager.GetRepository();
                XmlConfigurator.Configure(repository, new FileInfo(fullConfigPath));

                if (!repository.Configured)
                    throw new InvalidOperationException("log4net repository configuration failed.");

                initializedConfigPath = fullConfigPath;
            }
        }

        /// <summary>
        /// 입력되는 로그 파일 이름 변경 함수
        /// </summary>
        /// <param name="filePath">변경할 로그 파일의 전체 경로</param>
        public void ChangeLogFileName(string filePath)
        {
            if (!TryChangeLogFileName(filePath, out Exception error))
                throw new InvalidOperationException("Failed to change the log file path.", error);
        }

        /// <summary>
        /// KO: 파일 appender의 로그 파일 경로 변경을 시도합니다.
        /// EN: Attempts to change the log file path of the file appender.
        /// </summary>
        /// <param name="filePath">KO: 변경할 로그 파일의 전체 경로입니다. EN: The full path of the new log file.</param>
        /// <param name="error">KO: 변경 실패 예외입니다. EN: The exception raised when the operation fails.</param>
        /// <returns>KO: 변경 성공 여부입니다. EN: True when the path is changed successfully; otherwise, false.</returns>
        public bool TryChangeLogFileName(string filePath, out Exception error)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("Log file path cannot be empty.", nameof(filePath));

                string fullFilePath = Path.GetFullPath(filePath);
                string directoryPath = Path.GetDirectoryName(fullFilePath);
                if (string.IsNullOrWhiteSpace(directoryPath))
                    throw new InvalidOperationException("Log file path must include a directory.");

                Directory.CreateDirectory(directoryPath);

                FileAppender appender = LogManager.GetRepository()
                    .GetAppenders()
                    .FirstOrDefault(candidate =>
                        string.Equals(candidate.Name, appenderName, StringComparison.Ordinal) &&
                        candidate is FileAppender) as FileAppender;

                if (appender is null)
                    throw new InvalidOperationException($"File appender '{appenderName}' was not found.");

                appender.File = fullFilePath;
                appender.ActivateOptions();
                error = null;
                return true;
            }
            catch (Exception exception)
            {
                error = exception;
                return false;
            }
        }

        /// <summary>
        /// 디버깅 중 남길 로그
        /// </summary>
        /// <param name="msg">로그 메세지</param>
        public void Debug(string msg)
        {
            logger.Debug(msg);
        }

        /// <summary>
        /// 정보 기록 로그
        /// </summary>
        /// <param name="msg">메세지</param>
        public void Info(string msg)
        {
            logger.Info(msg);
        }

        /// <summary>
        /// 경고 기록 로그
        /// </summary>
        /// <param name="msg">메세지</param>
        public void Warn(string msg)
        {
            logger.Warn(msg);   
        }

        /// <summary>
        /// 에러 기록 로그
        /// </summary>
        /// <param name="msg">메세지</param>
        /// <param name="exception">예외 처리</param>
        public void Error(string msg, Exception exception = null)
        {
            logger.Error(msg, exception);
        }

        /// <summary>
        /// 어플리케이션의 치명적인 종료 기록 로그
        /// </summary>
        /// <param name="msg">메세지</param>
        /// <param name="exception">예외 처리</param>
        public void Fatal(string msg, Exception exception = null)
        {
            logger.Fatal(msg, exception);
        }
    }
}
