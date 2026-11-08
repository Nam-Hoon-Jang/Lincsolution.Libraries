using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lincsolution.UI.Base.Common;

/// <summary>
/// 사용자 설정을 JSON 파일로 관리합니다.<br/>
/// Manages user preferences in a JSON file.
/// </summary>
/// <typeparam name="TPreference">사용자 설정 형식입니다. / The user preference type.</typeparam>
public sealed class PreferenceManager<TPreference>
    where TPreference : class, new()
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// 사용자 설정 파일 경로를 지정하여 관리자를 생성합니다.<br/>
    /// Creates a manager for the specified preference file path.
    /// </summary>
    /// <param name="filePath">사용자 설정 JSON 파일 경로입니다. / The preference JSON file path.</param>
    public PreferenceManager(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        FilePath = Path.GetFullPath(filePath);
    }

    /// <summary>
    /// 사용자 설정 파일의 절대 경로를 가져옵니다.<br/>
    /// Gets the absolute path of the preference file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// 현재 사용자 설정을 가져옵니다.<br/>
    /// Gets the current user preferences.
    /// </summary>
    public TPreference Current { get; private set; } = new();

    /// <summary>
    /// 마지막 불러오기 실패 원인을 가져옵니다. 파일이 없거나 정상적으로 읽혔다면 null입니다.<br/>
    /// Gets the last load error. Null when the file is absent or loaded successfully.
    /// </summary>
    public Exception? LastLoadError { get; private set; }

    /// <summary>
    /// 사용자 설정을 불러옵니다. 파일이 없거나 손상되었다면 기본값을 반환합니다.<br/>
    /// Loads user preferences. Returns defaults when the file is absent or invalid.
    /// </summary>
    /// <returns>불러온 사용자 설정입니다. / The loaded user preferences.</returns>
    public TPreference Load()
    {
        LastLoadError = null;

        if (!File.Exists(FilePath))
        {
            Current = new TPreference();
            return Current;
        }

        try
        {
            using FileStream stream = File.OpenRead(FilePath);
            Current = JsonSerializer.Deserialize<TPreference>(stream, SerializerOptions)
                ?? new TPreference();
        }
        catch (Exception exception) when (exception is JsonException
                                          or IOException
                                          or UnauthorizedAccessException)
        {
            LastLoadError = exception;
            Current = new TPreference();
        }

        return Current;
    }

    /// <summary>
    /// 현재 사용자 설정을 저장합니다.<br/>
    /// Saves the current user preferences.
    /// </summary>
    public void Save() => Save(Current);

    /// <summary>
    /// 지정한 사용자 설정을 저장하고 현재 설정으로 지정합니다.<br/>
    /// Saves the specified preferences and makes them current.
    /// </summary>
    /// <param name="preference">저장할 사용자 설정입니다. / The user preferences to save.</param>
    public void Save(TPreference preference)
    {
        ArgumentNullException.ThrowIfNull(preference);

        string? directoryPath = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string temporaryPath = $"{FilePath}.tmp";

        try
        {
            using (FileStream stream = new(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, preference, SerializerOptions);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, FilePath, overwrite: true);
            Current = preference;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
