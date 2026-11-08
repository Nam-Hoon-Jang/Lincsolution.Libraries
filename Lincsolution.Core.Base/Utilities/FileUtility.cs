using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Lincsolution.Core.Base.Utilities
{
    public class FileUtility
    {
        /// <summary>
        /// 파일 경로 읽어서 존재하는지 확인하는 함수
        /// </summary>
        /// <param name="path">경로값</param>
        /// <returns>존재하면 true</returns>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 파일 경로 읽어서 존재하는지 확인하는 함수
        /// </summary>
        /// <param name="path">경로값</param>
        /// <returns>존재하면 true</returns>
        public static bool FileExists(FileInfo path)
        {
            return path.Exists;
        }

        /// <summary>
        /// 폴더 경로 읽어서 존재하는지 확인하는 함수
        /// </summary>
        /// <param name="path">경로값</param>
        /// <returns>존재하면 true</returns>
        public static bool DirExists(string dir)
        {
            return Directory.Exists(dir);
        }

        /// <summary>
        /// 폴더 경로 읽어서 존재하는지 확인하는 함수
        /// </summary>
        /// <param name="path">경로값</param>
        /// <returns>존재하면 true</returns>
        public static bool DirExists(DirectoryInfo dir)
        {
            return dir.Exists;
        }

        /// <summary>
        /// 특정 경로에서, 특정 확장명식의 파일들을 가져오는 함수
        /// </summary>
        /// <param name="dir">탐색 경로</param>
        /// <param name="formatName">확장명</param>
        /// <param name="files">찾은 파일들 전체 경로 배열</param>
        /// <returns>파일을 찾으면 true, 찾지 못하면 false</returns>
        public static bool GetFormattedFiles(string dir, string formatName, out string[] files)
        {
            files = Array.Empty<string>();

            if(string.IsNullOrWhiteSpace(dir) || string.IsNullOrWhiteSpace(formatName))
            {
                throw new InvalidOperationException("Directory or format name cannot be empty.");
            }

            if(!Directory.Exists(dir))
            {
                return false;
            }

            string format = formatName.Trim();
            if(format.StartsWith("."))
            {
                format = "*" + format;
            }
            else
            {
                format = "*." + format;
            }

            try
            {
                files = Directory.EnumerateFiles(dir, format, SearchOption.TopDirectoryOnly).ToArray<string>();
            }
            catch(UnauthorizedAccessException ex)
            {
                return false;
            }

            if(files.Length > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 특정 경로에서, 특정 확장명식의 파일들을 가져오는 함수
        /// </summary>
        /// <param name="dir">탐색 경로</param>
        /// <param name="formatName">확장명</param>
        /// <param name="files">찾은 파일들 전체 경로 배열</param>
        /// <returns>파일을 찾으면 true, 찾지 못하면 false</returns>
        public static bool GetFormattedFiles(string dir, string formatName, out List<string> files)
        {
            files = new List<string>();

            if(string.IsNullOrWhiteSpace(dir) || string.IsNullOrWhiteSpace(formatName))
            {
                throw new InvalidOperationException("Directory or format name cannot be empty.");
            }

            if(!Directory.Exists(dir))
            {
                return false;
            }

            string format = formatName.Trim();
            if(format.StartsWith("."))
            {
                format = "*" + format;
            }
            else
            {
                format = "*." + format;
            }

            try
            {
                files = Directory.EnumerateFiles(dir, format, SearchOption.TopDirectoryOnly).ToList<string>();
            }
            catch(UnauthorizedAccessException ex)
            {
                return false;
            }

            if(files.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 폴더를 읽고, 내부 파일 Dictionary로 저장.
        /// GetPath로 읽으려는 경로가 존재하는지부터 체크. 없으면 return false
        /// </summary>
        /// <param name="path">읽으려는 경로</param>
        /// <param name="dic">key=파일명, value=파일 전체 경로</param>
        /// <returns>Dictionary 저장까지 성공하면 true, 실패하면 false</returns>
        public static bool ReadFolderAndGetFiles(string path, ref Dictionary<string, string> dic)
        {
            if(DirExists(path))
            {
                if(dic == null)
                {
                    dic = new Dictionary<string, string>();
                    string[] files = Directory.GetFiles(path);
                    foreach(var fullPath in files)
                    {
                        dic[Path.GetFileName(fullPath)] = fullPath;
                    }
                    return true;
                }
                else
                {
                    dic.Clear();
                    string[] files = Directory.GetFiles(path);
                    foreach(var fullPath in files)
                    {
                        dic[Path.GetFileName(fullPath)] = fullPath;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 파일 크기 가져오는 함수
        /// </summary>
        /// <param name="size">파일 크기값 입력</param>
        /// <returns>B~TB 까지 텍스트로 출력</returns>
        public static string GetFileSize(long size)
        {
            string[] units = {"B", "KB", "MB", "GB", "TB"};
            int index = 0;
            /// size 값이 1024 이상(즉, 아직 올라갈 단위가 있다면) + 인덱스 마지막 번호가 되기 전까지.
            while(size >= 1024 && index < units.Length - 1)
            {
                size = size / 1024;
                index++;
            }
            return $"{size:0.##}{units[index]}";
        }
    }
}
