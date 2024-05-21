using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADUtils.Utils
{
    public static class PromptUtils
    {
        public static string PromptKeyword(string message, bool allowNone, params string[] keywords)
        {
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = $"{Environment.NewLine}{message}";

            foreach (string keyword in keywords)
            {
                pKeyOpts.Keywords.Add(keyword);
            }

            pKeyOpts.AllowNone = false;

            PromptResult promptResult = DocumentUtils.Editor.GetKeywords(pKeyOpts);

            return promptResult.StringResult;
        }
    }
}
