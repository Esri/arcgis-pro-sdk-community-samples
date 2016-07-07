using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LayerPopups.Helpers {
    internal static class PopupInfoTextFactory {
        public static readonly string DefaultFormat =
            "<div><p><span style=\"font-weight:normal;text-decoration:none;\">{0}</span></p></div>";
        public static readonly string DefaultTitleFormat =
            "<div><p><span style=\"color:black;font-weight:bold;text-decoration:none;\">{0}</span></p></div>";

        public static readonly string DefaultURLFormat =
            "<a href=\"{0}\" target=\"_blank\" style=\"color:#6D6D6D;text-decoration:underline;\">{1}</a>";

        public static string FormatFieldName(string fieldName) {
            return string.Format("{0}{1}{2}", "{", fieldName, "}");
        }

        public static string FormatUrl(Uri uri, string description) {
            string url = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString;
            string content = description ?? url;
            return string.Format(DefaultURLFormat, url, content);
        }

        public static string FormatText(string text, bool asTitleOrCaption = false) {
            return string.Format(asTitleOrCaption ? DefaultTitleFormat : DefaultFormat, text);
        }

        internal static string FormatTextInternal(string plainText, string formattedText, bool isTitle = false) {
            string popupText = "";
            if (!string.IsNullOrEmpty(formattedText))
                popupText = WebUtility.HtmlEncode(formattedText);
            else if (!string.IsNullOrEmpty(plainText)) {
                popupText = WebUtility.HtmlEncode(FormatText(plainText, isTitle));
            }
            return popupText;
        }
    }
}
