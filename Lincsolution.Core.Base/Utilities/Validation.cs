using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lincsolution.Core.Base.Utilities
{
    public class Validation
    {
        /// <summary>
        /// 매개변수로 입력된 텍스트 배열 확인해서 null 또는 empty 있으면 false 반환
        /// </summary>
        /// <param name="strings">확인할 string 배열</param>
        /// <returns>모든 문자열이 null 또는 empty가 아니면 true, 하나라도 null 또는 empty가 있으면 false</returns>
        public bool CheckStringArrayIsNullOrEmpty(string[] strings)
        {
            return strings.All(s => !string.IsNullOrEmpty(s));
        }

        /// <summary>
        /// 매개변수로 입력된 텍스트 배열 확인해서 null 또는 empty 있으면 false 반환
        /// </summary>
        /// <param name="strings">확인할 string List</param>
        /// <returns>모든 문자열이 null 또는 empty가 아니면 true, 하나라도 null 또는 empty가 있으면 false</returns>
        public bool CheckStringListIsNullOrEmpty(List<string> strings)
        {
            return strings.All(s => !string.IsNullOrEmpty(s));
        }
    }
}
