using System.IO;
using System;
public static class TextHandler {
    private static string text;
    private static string[] lines;
    public static string ReadFile (string path) {
        if (path != null) {
            FileInfo file = new FileInfo(path);
            StreamReader reader = file.OpenText();
            text = reader.ReadToEnd();
            lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            return text;
        }
        return "PathNotGivenToTextHandler";
    }
    public static int GetLineCount() {
        return lines.Length;
    }
    public static string ReadLine(int lineNumber) {
        return lines[lineNumber];
    }
}