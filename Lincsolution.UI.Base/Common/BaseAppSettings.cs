using Lincsolution.Core.Base.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using static Lincsolution.Core.Base.Utilities.FileUtility;
using Lincsolution.Core.Base.Logging;

namespace Lincsolution.UI.Base.Common
{
    public abstract class BaseAppSettings
    {
        public bool IsInitialised { get; private set; } = false;

        public string ConfigPath { get; private set; } = string.Empty;

        public string BaseDir { get; private set; } = string.Empty;

        public string Log4NetConfigPath { get; private set; } = string.Empty;

        public string LogDir { get; private set; } = string.Empty;

        public string SysLogDir { get; private set; } = string.Empty;

        public ILogger? SysLog { get; set; }

        protected IConfigurationRoot? Cfg { get; set; }
        protected Dictionary<string, string> Tokens { get; set; } = new();

        public BaseAppSettings()
        {
            BaseDir = AppContext.BaseDirectory;
        }

        public void Initialise(string configPath)
        {
            ConfigPath = configPath;

            if(!FileExists(configPath))
            {
                throw new FileNotFoundException($"Configuration File(.cfg) not found on path : {ConfigPath}");
            }

            if(IsInitialised)
            {
                throw new InvalidOperationException("Settings already Initialised.");
            }

            LoadCfgAndTokens(configPath);

            ExpandSettings();

            IsInitialised = true;
        }

        protected virtual void ExpandSettings()
        {
            Log4NetConfigPath = ExpandCfgTokens(GetRequiredCfgValue("BaseDirectories:Log4NetConfigPath"), Tokens);
            LogDir = ExpandCfgTokens(GetRequiredCfgValue("BaseDirectories:LogDir"), Tokens);
            Tokens["LogDir"] = LogDir;
            SysLogDir = ExpandCfgTokens(GetRequiredCfgValue("LogDir:SysLogDir"), Tokens);
        }

        protected virtual void LoadCfgAndTokens(string configPath)
        {
            Cfg = new ConfigurationBuilder().SetBasePath(BaseDir)
                .AddIniFile(configPath, optional: false, reloadOnChange: false).Build();

            Tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["BaseDir"] = BaseDir
            };
        }

        /// <summary>
        /// ExpandCfg() 에서, .cfg 파일 내 token-path 확인 후 경로로 반환하는 함수
        /// </summary>
        /// <param name="rawString">.cfg 파일 내부의 제목:요소</param>
        /// <param name="tokens">치환할 토큰 딕셔너리</param>
        /// <returns>치환된 경로 문자열</returns>
        protected string ExpandCfgTokens(string rawString, IDictionary<string, string> tokens)
        {
            /// OS 환경변수 확인
            var cfgDatas = Environment.ExpandEnvironmentVariables(rawString);

            /// cfgDatas 내 token 확인 후 치환
            foreach(var keyValue in tokens)
            {
                cfgDatas = cfgDatas.Replace($"%{keyValue.Key}%", keyValue.Value, StringComparison.OrdinalIgnoreCase);
            }

            /// Uri 데이터인지 확인
            var isUri = Uri.TryCreate(cfgDatas.Trim(), UriKind.Absolute, out Uri? uri);

            /// 파일 아니면서 Uri이면 반환.
            if(isUri && uri != null && !uri.IsFile)
            {
                return cfgDatas;
            }

            return Path.GetFullPath(cfgDatas);
        }

        /// <summary>
        /// KO: 필수 구성 값을 가져오며 값이 없으면 구성 데이터 예외를 발생시킵니다.
        /// EN: Gets a required configuration value and throws a configuration-data exception when it is missing.
        /// </summary>
        /// <param name="key">KO: 구성 키입니다. EN: The configuration key.</param>
        /// <returns>KO: 공백이 아닌 구성 값입니다. EN: A non-empty configuration value.</returns>
        protected string GetRequiredCfgValue(string key)
        {
            var value = Cfg?[key];

            if(string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException($"Required configuration key '{key}' is missing or empty in '{ConfigPath}'.");
            }

            return value;
        }
    }
}
