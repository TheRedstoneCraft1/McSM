namespace McSM;
using System.Text.Json;


public static class Lib
{
    public static void ZeichneRahmen(int breite, int hoehe, string style = "solid", ConsoleColor color = ConsoleColor.White)
    {
        try
        {
            Console.Clear();

            var rahmen = GetRahmenChars(style);

            if (breite > 2)
            {
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = color;
                Console.Write(rahmen.obenLinks + new string(rahmen.obenUnten, breite - 2) + rahmen.obenRechts);
                Console.ResetColor();
            }

            for (int y = 1; y < hoehe - 1; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.ForegroundColor = color;
                Console.Write(rahmen.links);
                Console.ResetColor();

                if (breite > 1)
                {
                    Console.SetCursorPosition(breite - 1, y);
                    Console.ForegroundColor = color;
                    Console.Write(rahmen.rechts);
                    Console.ResetColor();
                }
            }

            if (hoehe > 1 && breite > 2)
            {
                Console.SetCursorPosition(0, hoehe - 1);
                Console.ForegroundColor = color;
                Console.Write(rahmen.untenLinks + new string(rahmen.obenUnten, breite - 2) + rahmen.untenRechts);
                Console.ResetColor();
            }
        }
        catch
        {
        }
    }
    static (char obenLinks, char obenRechts, char untenLinks, char untenRechts, char obenUnten, char links, char rechts) GetRahmenChars(string style)
    {
        return style.ToLower() switch
        {
            "solid" or "0" => ('┌', '┐', '└', '┘', '─', '│', '│'),
            "double" or "1" => ('╔', '╗', '╚', '╝', '═', '║', '║'),
            "thick" or "2" => ('┏', '┓', '┗', '┛', '━', '┃', '┃'),
            "rounded" or "3" => ('╭', '╮', '╰', '╯', '─', '│', '│'),
            "dashed" or "4" => ('┌', '┐', '└', '┘', '╌', '╎', '╎'),
            _ => ('┌', '┐', '└', '┘', '─', '│', '│')
        };
    }

    //static
    #region Text

    // Einzelner String
    public static void Text(int x, int y, string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        text = Translation.Key(text);

        Text(
            x,
            y,
            text.Replace("\r", "").Split('\n'),
            foreground,
            background
        );
    }


    // Mehrere Zeilen
    public static void Text(int x, int y, string[] lines, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        try
        {
            // ANSI-Escape-Sequenzen entfernen für Längenmessung
            var stripAnsi = (string str) =>
                System.Text.RegularExpressions.Regex.Replace(str, @"\x1b\[[0-9;]*m", "");

            // x = 0 bedeutet: horizontal zentrieren
            if (x == 0)
            {
                int longestLine = lines.Max(line => stripAnsi(line).Length);
                x = Math.Max(0, (Console.WindowWidth - longestLine) / 2);
            }

            // y = 0 bedeutet: vertikal zentrieren
            if (y == 0)
            {
                y = Math.Max(0, (Console.WindowHeight - lines.Length) / 2);
            }

            Console.ForegroundColor = foreground ?? ConsoleColor.White;

            Console.BackgroundColor = background ?? ConsoleColor.Black;

            for (int i = 0; i < lines.Length; i++)
            {
                if (y + i >= Console.WindowHeight) break;

                Console.SetCursorPosition(x, y + i);
                Console.Write(lines[i]);
            }

            Console.ResetColor();
        }
        catch
        {
            // Fehler ignorieren
        }
    }

    #endregion

    #region Cut
    public static void Cut(string direction, int pos, string style = "solid", ConsoleColor color = ConsoleColor.White)
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        if (direction.ToUpper() == "H")
        {
            // pos = 0 bedeutet: horizontal in der Mitte
            if (pos == 0)
                pos = height / 2;

            if (pos < 0 || pos >= height)
                return;

            // Baue die Linie mit T-Connectoren an den Enden
            string lineContent = GetLineStyle(style, width - 2);
            string fullLine = GetTConnectorLeft(style) + lineContent + GetTConnectorRight(style);
            Text(0, pos, fullLine, color);
        }
        else if (direction.ToUpper() == "V")
        {
            // pos = 0 bedeutet: vertikal in der Mitte
            if (pos == 0) pos = width / 2;

            if (pos < 0 || pos >= width) return;

            string character = GetCharStyle(style);
            string connectorTop = GetTConnectorTop(style) ?? "┬"; // Fallback
            string connectorBottom = GetTConnectorBottom(style) ?? "┴";

            // Oberer Connector
            Console.SetCursorPosition(pos, 0); Console.ForegroundColor = color; Console.Write(connectorTop); Console.ResetColor();

            // Mittlerer Teil: vertikale Linie
            for (int y = 1; y < height - 1; y++)
            {
                Text(pos, y, character, color);
            }

            // Unterer Connector
            Text(pos, height - 1, connectorBottom, color);
        }

    }
    static string GetLineStyle(string style, int length) =>
        style.ToLower() switch
        {
            "solid" or "0" => new string('─', length),
            "double" or "1" => new string('═', length),
            "dashed" or "2" => GenerateDashedLine(length),
            "dotted" or "3" => GenerateDottedLine(length),
            "rounded" or "4" => new string('─', length),
            "thick" or "5" => new string('━', length),
            _ => new string('─', length)
        };
    static string GetCharStyle(string style) => style.ToLower() switch
    {
        "solid" or "0" => "│",
        "double" or "1" => "║",
        "dashed" or "2" => "┆",
        "dotted" or "3" => "┊",
        "rounded" or "4" => "│",
        "thick" or "5" => "┃",
        _ => "│"
    };
    static string GetTConnectorLeft(string style) => style.ToLower() switch
    {
        "solid" or "0" => "├",
        "double" or "1" => "╠",
        "rounded" or "4" => "├",
        "thick" or "5" => "┣",
        _ => "├"
    };
    static string GetTConnectorRight(string style) => style.ToLower() switch
    {
        "solid" or "0" => "┤",
        "double" or "1" => "╣",
        "rounded" or "4" => "┤",
        "thick" or "5" => "┫",
        _ => "┤"
    };
    static string GetTConnectorTop(string style) => style.ToLower() switch
    {
        "solid" or "0" => "┬",
        "double" or "1" => "╦",
        "rounded" or "4" => "┬",
        "thick" or "5" => "┳",
        _ => "┬"
    };
    static string GetTConnectorBottom(string style) => style.ToLower() switch
    {
        "solid" or "0" => "┴",
        "double" or "1" => "╩",
        "rounded" or "4" => "┴",
        "thick" or "5" => "┻",
        _ => "┴"
    };

    static string GenerateDashedLine(int length)
    {
        string pattern = "─ ";
        return string.Concat(Enumerable.Repeat(pattern, (length / 2) + 1)).Substring(0, length);
    }

    static string GenerateDottedLine(int length)
    {
        string pattern = "·";
        return string.Concat(Enumerable.Repeat(pattern, length)).Substring(0, length);
    }
    #endregion

    // inputs
    #region Buttons
    // Alignment: "center", "left", "right"
    public static List<(int x, int y, string[] lines, Action action, string alignment, ConsoleColor[] colors)> buttons = new();

    static int focusedButtonIndex = 0;

    public static void AddButton(int x, int y, string text, Action onPress, string alignment = "center", ConsoleColor textColor = ConsoleColor.White)
    {
        // Text übersetzen und nach Newlines splitten
        text = Translation.Key(text);
        string[] lines = text.Replace("\r", "").Split('\n');
        AddButton(x, y, lines, onPress, alignment, textColor);
    }


    public static void AddButton(int x, int y, string[] lines, Action onPress, string alignment = "center", ConsoleColor textColor = ConsoleColor.White)
    {
        AddButton(x, y, lines, onPress, alignment, Enumerable.Repeat(textColor, lines.Length).ToArray());
    }

    public static void AddButton(int x, int y, string[] lines, Action onPress, string alignment = "center", ConsoleColor[] textColors = null)
    {
        // Übersetzung für jede Zeile
        string[] displayLines = lines.Select(line => Translation.Key(line)).ToArray();

        // Längste Zeile ermitteln für Breite
        var stripAnsi = (string str) =>
            System.Text.RegularExpressions.Regex.Replace(str, @"\x1b\[[0-9;]*m", "");

        int buttonWidth = displayLines.Max(line => stripAnsi(line).Length) + 2;
        int buttonHeight = displayLines.Length;

        // Farben initialisieren
        if (textColors == null || textColors.Length != displayLines.Length)
        {
            textColors = Enumerable.Repeat(ConsoleColor.White, displayLines.Length).ToArray();
        }

        // x = 0: horizontal zentrieren
        if (x == 0)
        {
            x = Math.Max(0, (Console.WindowWidth - buttonWidth) / 2);
            alignment = "center";
        }
        else if (alignment == "right" || alignment == "r")
        {
            x = Math.Max(0, x - buttonWidth + 1);
        }
        else if (alignment == "left" || alignment == "l")
        {
            x = Math.Max(0, x);
        }

        // y = 0: vertikal zentrieren
        if (y == 0)
        {
            y = Math.Max(0, (Console.WindowHeight - buttonHeight) / 2);
        }

        buttons.Add((x, y, displayLines, onPress, alignment, textColors));
    }

    public static void DrawButton(int index, bool focused)
    {
        var (x, y, lines, _, alignment, colors) = buttons[index];

        try
        {
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                Console.SetCursorPosition(x, y + lineIndex);
                Console.ForegroundColor = colors[lineIndex];

                if (focused)
                    Console.Write("\x1b[48;2;80;80;80m");
                else
                    Console.Write("\x1b[48;2;36;36;36m");

                Console.Write(" " + lines[lineIndex] + " ");
                Console.ResetColor();
            }
        }
        catch
        {
        }
    }
    public static void DrawAllButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            DrawButton(i, i == focusedButtonIndex);
        }
    }

    public static void ClearButtons()
    {
        buttons.Clear();
        focusedButtonIndex = 0;
    }

    public static void HandleButtonInput(ConsoleKeyInfo key)
    {
        if (buttons.Count == 0) return;

        var currentButton = buttons[focusedButtonIndex];
        int currentX = currentButton.x;
        int currentY = currentButton.y;

        switch (key.Key)
        {
            case ConsoleKey.RightArrow:
                focusedButtonIndex = FindNextButtonInDirection(currentX, currentY, "right");
                break;
            case ConsoleKey.LeftArrow:
                focusedButtonIndex = FindNextButtonInDirection(currentX, currentY, "left");
                break;
            case ConsoleKey.DownArrow:
                focusedButtonIndex = FindNextButtonInDirection(currentX, currentY, "down");
                break;
            case ConsoleKey.UpArrow:
                focusedButtonIndex = FindNextButtonInDirection(currentX, currentY, "up");
                break;
            case ConsoleKey.Tab:
                focusedButtonIndex = (focusedButtonIndex + 1) % buttons.Count;
                break;
            case ConsoleKey.Enter:
                buttons[focusedButtonIndex].action.Invoke();
                break;
        }
    }

    static int FindNextButtonInDirection(int x, int y, string direction)
    {
        int bestIndex = focusedButtonIndex;
        int bestDistance = int.MaxValue;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (i == focusedButtonIndex) continue;

            var button = buttons[i];
            int buttonX = button.x;
            int buttonY = button.y;
            int distance = 0;
            bool isInDirection = false;

            switch (direction)
            {
                case "right":
                    if (buttonX > x)  // Nur rechts
                    {
                        isInDirection = true;
                        distance = (buttonX - x) * 10 + Math.Abs(buttonY - y);  // X hat Vorrang
                    }
                    break;
                case "left":
                    if (buttonX < x)  // Nur links
                    {
                        isInDirection = true;
                        distance = (x - buttonX) * 10 + Math.Abs(buttonY - y);  // X hat Vorrang
                    }
                    break;
                case "down":
                    if (buttonY > y)  // Nur unten
                    {
                        isInDirection = true;
                        distance = (buttonY - y) * 10 + Math.Abs(buttonX - x);  // Y hat Vorrang
                    }
                    break;
                case "up":
                    if (buttonY < y)  // Nur oben
                    {
                        isInDirection = true;
                        distance = (y - buttonY) * 10 + Math.Abs(buttonX - x);  // Y hat Vorrang
                    }
                    break;
            }

            if (isInDirection && distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    #endregion

}

public static class Translation
{
    private static readonly Dictionary<string, string> translations = new();

    public static void Load(string language)
    {
        string filePath = Path.Combine(
            AppContext.BaseDirectory,
            "lang",
            $"{language}.json"
        );

        if (!File.Exists(filePath))
            return;

        string json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return;

        var loadedTranslations =
            JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (loadedTranslations == null)
            return;

        translations.Clear();

        foreach (var item in loadedTranslations)
        {
            translations[item.Key] = item.Value;
        }
    }

    public static string Key(string text)
    {
        return translations.TryGetValue(text, out string? translation)
            ? translation
            : text;
    }
}

public static class Json
{
    public class GuiData
    {
        public int page { get; set; }
        public int page_alt { get; set; }
    }

    public class UserData
    {
        public string lang { get; set; } = "";
    }

    public class SystemData
    {
        public bool needsRedraw { get; set; }
    }

    public class AppData
    {
        public GuiData gui { get; set; } = new();
        public UserData user { get; set; } = new();
        public SystemData System { get; set; } = new();
    }

    public static AppData Data { get; set; } = new()
    {
        gui = new GuiData { page = 0, page_alt = 0 },
        user = new UserData { lang = "en_gb" },
        System = new SystemData { needsRedraw = true }
    };

    private static readonly string DataPath = Path.Combine(AppContext.BaseDirectory, "data.json");

    // ====== SPEICHERN ======

    /// <summary>Speichert die komplette AppData</summary>
    public static void Save(AppData data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(DataPath, json);
    }

    /// <summary>Speichert nur die gui-Eigenschaften</summary>
    public static void SaveGui()
    {
        Data = LoadFromDisk();
        Data.gui = Data.gui ?? new GuiData();
        Save(Data);
    }

    /// <summary>Speichert nur gui.page</summary>
    public static void SaveGuiPage(int page)
    {
        Data.gui.page = page;
        SaveGui();
    }

    /// <summary>Speichert nur gui.page_alt</summary>
    public static void SaveGuiPageAlt(int pageAlt)
    {
        Data.gui.page_alt = pageAlt;
        SaveGui();
    }

    /// <summary>Speichert nur die user-Eigenschaften</summary>
    public static void SaveUser()
    {
        Data = LoadFromDisk();
        Data.user = Data.user ?? new UserData();
        Save(Data);
    }

    /// <summary>Speichert nur user.lang</summary>
    public static void SaveUserLang(string lang)
    {
        Data.user.lang = lang;
        SaveUser();
    }

    /// <summary>Speichert nur die system-Eigenschaften</summary>
    public static void SaveSystem()
    {
        Data = LoadFromDisk();
        Data.System = Data.System ?? new SystemData();
        Save(Data);
    }

    /// <summary>Speichert nur system.needsRedraw</summary>
    public static void SaveSystemNeedsRedraw(bool needsRedraw)
    {
        Data.System.needsRedraw = needsRedraw;
        SaveSystem();
    }

    // ====== LADEN ======

    public static AppData Load()
    {
        if (!File.Exists(DataPath)) { return CreateDefaultData(); }
        return LoadFromDisk();
    }

    private static AppData LoadFromDisk()
    {
        try
        {
            string json = File.ReadAllText(DataPath);
            if (string.IsNullOrWhiteSpace(json)) { return CreateDefaultData(); }
            return JsonSerializer.Deserialize<AppData>(json) ?? CreateDefaultData();
        }
        catch (JsonException) { return CreateDefaultData(); }
    }

    private static AppData CreateDefaultData()
    {
        return new AppData
        {
            gui = new GuiData { page = 0, page_alt = 0 },
            user = new UserData { lang = "de_de" },
            System = new SystemData { needsRedraw = true }
        };
    }
}

public static class Stuff {


    public static void Logo()
    {
        Random random = new Random();
        int choice = random.Next(0, 6); // 0-5 für 6 verschiedene Verläufe
        List<string> color = new List<string>();
        color.Clear();
        switch (choice)
        {
            case 0: // BLAU zu CYAN
                color.Add("\x1b[38;2;0;0;100m");
                color.Add("\x1b[38;2;0;50;130m");
                color.Add("\x1b[38;2;0;100;160m");
                color.Add("\x1b[38;2;0;150;200m");
                color.Add("\x1b[38;2;0;200;230m");
                color.Add("\x1b[38;2;0;255;255m");
                color.Add("\x1b[36m");
                break;

            case 1: // ROT zu MAGENTA
                color.Add("\x1b[38;2;100;0;0m");
                color.Add("\x1b[38;2;130;0;40m");
                color.Add("\x1b[38;2;160;0;80m");
                color.Add("\x1b[38;2;200;0;120m");
                color.Add("\x1b[38;2;230;0;160m");
                color.Add("\x1b[38;2;255;0;255m");
                color.Add("\x1b[35m");
                break;

            case 2: // ORANGE zu GELB
                color.Add("\x1b[38;2;100;50;0m");
                color.Add("\x1b[38;2;130;80;0m");
                color.Add("\x1b[38;2;160;120;0m");
                color.Add("\x1b[38;2;200;150;0m");
                color.Add("\x1b[38;2;230;190;0m");
                color.Add("\x1b[38;2;255;255;0m");
                color.Add("\x1b[33m");
                break;

            case 3: // LILA zu PINK
                color.Add("\x1b[38;2;80;0;100m");
                color.Add("\x1b[38;2;110;30;130m");
                color.Add("\x1b[38;2;140;60;160m");
                color.Add("\x1b[38;2;170;90;190m");
                color.Add("\x1b[38;2;200;120;220m");
                color.Add("\x1b[38;2;255;150;255m");
                color.Add("\x1b[35m");
                break;

            case 4: // DUNKELROT zu HELLROT
                color.Add("\x1b[38;2;80;0;0m");
                color.Add("\x1b[38;2;120;20;20m");
                color.Add("\x1b[38;2;160;40;40m");
                color.Add("\x1b[38;2;200;60;60m");
                color.Add("\x1b[38;2;230;100;100m");
                color.Add("\x1b[38;2;255;150;150m");
                color.Add("\x1b[31m");
                break;

            case 5: // DUNKELGRÜN zu LIME
                color.Add("\x1b[38;2;0;100;0m");
                color.Add("\x1b[38;2;40;130;0m");
                color.Add("\x1b[38;2;80;160;0m");
                color.Add("\x1b[38;2;120;200;0m");
                color.Add("\x1b[38;2;160;230;0m");
                color.Add("\x1b[38;2;0;255;0m");
                color.Add("\x1b[32m");
                break;
        }


        McSM.Lib.Text(0, 2, color[0] + "███╗   ███╗  ██████╗ ███████╗ ███╗   ███╗         ");
        McSM.Lib.Text(0, 3, color[1] + "████╗ ████║ ██╔════╝ ██╔════╝ ████╗ ████║         ");
        McSM.Lib.Text(0, 4, color[2] + "██╔████╔██║ ██║      ███████╗ ██╔████╔██║         ");
        McSM.Lib.Text(0, 5, color[3] + "██║╚██╔╝██║ ██║      ╚════██║ ██║╚██╔╝██║         ");
        McSM.Lib.Text(0, 6, color[4] + "██║ ╚═╝ ██║ ╚██████╗ ███████║ ██║ ╚═╝ ██║         ");
        McSM.Lib.Text(0, 7, color[5] + "╚═╝     ╚═╝  ╚═════╝ ╚══════╝ ╚═╝     ╚═╝" + color[6] + "   by TRC");
    }
}