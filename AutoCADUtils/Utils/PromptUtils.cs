using Autodesk.AutoCAD.ApplicationServices;
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

        public static string PromptString(string message)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            PromptStringOptions pStrOpts = new PromptStringOptions($"\n{message}")
            {
                AllowSpaces = true
            };

            PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);

            if(pStrRes.Status == PromptStatus.OK)
            {
                return pStrRes.StringResult;
            }

            return string.Empty;
        }
    }
}
