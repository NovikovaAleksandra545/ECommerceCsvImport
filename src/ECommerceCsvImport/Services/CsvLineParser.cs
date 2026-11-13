using System.Text;

namespace ECommerceCsvImport.Services
{
    /// <summary>
    /// Разбор строки CSV. Поддерживаются пустые поля, значения в кавычках
    /// (внутри может быть разделитель) и двойные кавычки "" внутри кавычек.
    /// </summary>
    public static class CsvLineParser
    {
        public static List<string> Parse(string line, char separator = ',')
        {
            var result = new List<string>();
            if (line == null)
                return result;

            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // "" внутри кавычек - это одна кавычка
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == separator && !inQuotes)
                {
                    result.Add(field.ToString().Trim());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            result.Add(field.ToString().Trim());
            return result;
        }

        // Разделитель определяем по заголовку: если там есть ";", значит файл из Excel
        public static char DetectSeparator(string headerLine)
        {
            if (headerLine != null && headerLine.Contains(';') && !headerLine.Contains(','))
                return ';';

            return ',';
        }
    }
}
