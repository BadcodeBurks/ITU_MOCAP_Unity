using System;
using System.IO;
using System.Text;

public static class FilenameValidator
{
    // Method to sanitize the filename
    public static string SanitizeFilename(this string filename)
    {
        // Define invalid characters for the file system
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Use a StringBuilder to efficiently build the sanitized filename
        StringBuilder sanitizedFilename = new StringBuilder(filename);

        // Loop through each character and replace invalid characters with an underscore
        for (int i = 0; i < sanitizedFilename.Length; i++)
        {
            if (Array.Exists(invalidChars, c => c == sanitizedFilename[i]))
            {
                sanitizedFilename[i] = '_';  // Replace with an underscore (you could choose another character)
            }
        }

        // Remove leading and trailing whitespaces (optional but recommended)
        string result = sanitizedFilename.ToString().Trim();

        // Ensure that the filename is not too long (optional)
        if (result.Length > 255)
        {
            result = result.Substring(0, 255);
        }

        return result;
    }
}