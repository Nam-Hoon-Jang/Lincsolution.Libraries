namespace Lincsolution.Core.Base.Logging
{
    /// <summary>
    /// KO: 로그와 Telemetry 데이터를 연결하는 작업 컨텍스트입니다.
    /// EN: Represents operation context that correlates log and telemetry data.
    /// </summary>
    public sealed class LogContext
    {
        /// <summary>
        /// KO: 작업 세션 식별자를 가져오거나 설정합니다.
        /// EN: Gets or sets the operation session identifier.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// KO: 빌드 작업 식별자를 가져오거나 설정합니다.
        /// EN: Gets or sets the build job identifier.
        /// </summary>
        public string BuildId { get; }

        /// <summary>
        /// KO: 장비 식별자를 가져오거나 설정합니다.
        /// EN: Gets or sets the device identifier.
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        /// KO: 레시피 식별자를 가져오거나 설정합니다.
        /// EN: Gets or sets the recipe identifier.
        /// </summary>
        public string RecipeId { get; }

        public LogContext(string sessionId = null, string buildId = null, string deviceId = null, string recipeId = null)
        {
            SessionId = sessionId;
            BuildId = buildId;
            DeviceId = deviceId;
            RecipeId = recipeId;
        }
    }
}
