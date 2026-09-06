namespace McSM;

using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;



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
            // Alle Zeilen durch Translation.Key() durchsuchen
            lines = lines.Select(line => Translation.Key(line)).ToArray();

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
    private static readonly Dictionary<string, string> _translations = new();

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

        _translations.Clear();

        foreach (var item in loadedTranslations)
        {
            _translations[item.Key] = item.Value;
        }
    }

    // Overload 1: Einfache Schlüssel-Übersetzung
    public static string Key(string text)
    {
        return _translations.TryGetValue(text, out string? translation)
            ? translation
            : text;
    }

    // Overload 2: Mit benannten Parametern
    public static string Key(string key, params (string name, object value)[] parameters)
    {
        string translation = Key(key); // Erst übersetzen

        foreach (var (name, value) in parameters)
        {
            translation = translation.Replace($"{{{name}}}", value?.ToString() ?? "");
        }

        return translation;
    }

    // Overload 3: Mit Objekt-Reflection
    public static string Key(string key, object? obj)
    {
        string translation = Key(key);

        if (obj == null)
            return translation;

        var properties = obj.GetType().GetProperties();

        foreach (var prop in properties)
        {
            string placeholder = $"{{{prop.Name}}}";
            string value = prop.GetValue(obj)?.ToString() ?? "";
            translation = translation.Replace(placeholder, value);
        }

        return translation;
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

    public class JavaScanner
    {
        public static List<JavaInstallation> FindJavaInstallations()
        {
            var installations = new List<JavaInstallation>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) installations.AddRange(FindJavaOnWindows());
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) installations.AddRange(FindJavaOnLinux());
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) installations.AddRange(FindJavaOnMacOS()); 
            return installations;
        }
        private static List<JavaInstallation> FindJavaOnWindows()
        {
            var installations = new List<JavaInstallation>();
            var registryPaths = new[] {
                @"HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\JDK",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\JRE",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\JavaSoft\JDK",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\JavaSoft\JRE"
            };
            foreach (var regPath in registryPaths)
            {
                try
                {
                    var key = Registry.LocalMachine.OpenSubKey(regPath.Replace(@"HKEY_LOCAL_MACHINE\", "")); if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            var subKey = key.OpenSubKey(subKeyName); var javaHome = subKey?.GetValue("JavaHome")?.ToString();
                            if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome)) { var version = GetJavaVersion(javaHome); installations.Add(new JavaInstallation { Version = version, JavaHome = javaHome, ExecutablePath = Path.Combine(javaHome, "bin", "java.exe") }); }
                        }
                    }
                }
                catch { }
            }
            var searchPaths = new[]
            {
                Environment.GetEnvironmentVariable("JAVA_HOME"),
                @"C:\Program Files\Java",
                @"C:\Program Files (x86)\Java"
            };
            foreach (var path in searchPaths) { if (!string.IsNullOrEmpty(path) && Directory.Exists(path)) { installations.AddRange(ScanDirectoryForJava(path)); } }
            return installations.DistinctBy(x => x.JavaHome).ToList();
        }
        private static List<JavaInstallation> FindJavaOnLinux()
        {
            var installations = new List<JavaInstallation>();
            var searchPaths = new[] {
                "/usr/lib/jvm",
                "/opt/java",
                Environment.GetEnvironmentVariable("JAVA_HOME")
            };
            foreach (var path in searchPaths)
            {
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))  installations.AddRange(ScanDirectoryForJava(path));
            } 
            try {
                var result = ExecuteCommand("which", "java");
                if (!string.IsNullOrEmpty(result)) 
                {
                    var javaPath = result.Trim();
                    var javaHome = Path.GetDirectoryName(Path.GetDirectoryName(javaPath));
                    var version = GetJavaVersion(javaHome);
                    installations.Add(new JavaInstallation
                    {
                        Version = version,
                        JavaHome = javaHome,
                        ExecutablePath = javaPath
                    });
                }
            }
            catch { }
            return installations.DistinctBy(x => x.JavaHome).ToList();
        }
        private static List<JavaInstallation> FindJavaOnMacOS()
        {
            var installations = new List<JavaInstallation>();
            var searchPaths = new[] { "/Library/Java/JavaVirtualMachines", "/System/Library/Java/JavaVirtualMachines", Environment.GetEnvironmentVariable("JAVA_HOME") };
            foreach (var path in searchPaths) { if (!string.IsNullOrEmpty(path) && Directory.Exists(path)) { installations.AddRange(ScanDirectoryForJava(path)); } }
            return installations.DistinctBy(x => x.JavaHome).ToList();
        }
        private static List<JavaInstallation> ScanDirectoryForJava(string basePath)
        {
            var installations = new List<JavaInstallation>();
            try
            {
                foreach (var dir in Directory.GetDirectories(basePath))
                {
                    var javaExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(dir, "bin", "java.exe") : Path.Combine(dir, "bin", "java");
                    if (File.Exists(javaExe) || Directory.Exists(Path.Combine(dir, "Contents", "Home")))
                    {
                        var javaHome = dir;                   if (Directory.Exists(Path.Combine(dir, "Contents", "Home")))                    {                        javaHome = Path.Combine(dir, "Contents", "Home");                    }
                        var version = GetJavaVersion(javaHome); installations.Add(new JavaInstallation { Version = version, JavaHome = javaHome, ExecutablePath = javaExe });
                    }
                }
            }
            catch { }
            return installations;
        }
        private static string GetJavaVersion(string javaHome)
        {
            try
            {
                var javaExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(javaHome, "bin", "java.exe") : Path.Combine(javaHome, "bin", "java");
                if (!File.Exists(javaExe)) return "Unbekannt";
                var output = ExecuteCommand(javaExe, "-version");
                var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                return lines.FirstOrDefault() ?? "Unbekannt";
            } catch { return "Fehler beim Lesen"; }
        }
        private static string ExecuteCommand(string command, string arguments = "")
        {
            try
            {
                var psi = new ProcessStartInfo { FileName = command, Arguments = arguments, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
                using (var process = Process.Start(psi)) { var output = process?.StandardOutput.ReadToEnd() ?? ""; process?.WaitForExit(); return output; }
            }
            catch { return ""; }
        }
        public static void JavaScann()
        {
            var installations = FindJavaInstallations();
            if (!installations.Any())
            {
                Lib.Text(0, 0, "gui.text.no_java_installations", ConsoleColor.Red);
                return;
            }
            var lines = new List<string> {"╔═══════════════════════════════════════════════════════════╗"};

            lines.Add("gui.text.java.headder");
            lines.Add("╠═══════════════════════════════════════════════════════════╣");

            foreach (var java in installations)
            {
                // Mit Padding VORHER machen, dann mit Translation.Key() übergeben
                lines.Add(Translation.Key("gui.text.java.v",("Version", java.Version.PadRight(48))));

                lines.Add(Translation.Key("gui.text.java.h",("JavaHome", java.JavaHome.PadRight(46))));

                lines.Add("╟───────────────────────────────────────────────────────────╢");
            }

            lines[lines.Count - 1] = "╚═══════════════════════════════════════════════════════════╝";
            Lib.Text(0, 0, lines.ToArray(), ConsoleColor.Green);
        }


    };
    public class JavaInstallation
    {
        public string Version { get; set; } = "";
        public string JavaHome { get; set; } = "";
        public string ExecutablePath { get; set; } = "";
    }

    private static List<JavaInstallation> FindJavaInstallations()
    {
        var installations = new List<JavaInstallation>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            installations.AddRange(FindJavaOnWindows());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            installations.AddRange(FindJavaOnLinux());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            installations.AddRange(FindJavaOnMac());
        }

        return installations.DistinctBy(x => x.JavaHome).ToList();
    }

    private static List<JavaInstallation> FindJavaOnWindows()
    {
        var installations = new List<JavaInstallation>();
        var paths = new[]
        {
        @"C:\Program Files\Java",
        @"C:\Program Files (x86)\Java",
        Environment.GetEnvironmentVariable("JAVA_HOME") ?? ""
    };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            if (!Directory.Exists(path)) continue;

            foreach (var dir in Directory.GetDirectories(path))
            {
                var javaBin = Path.Combine(dir, "bin", "java.exe");
                if (File.Exists(javaBin))
                {
                    var version = GetJavaVersion(javaBin);
                    installations.Add(new JavaInstallation
                    {
                        Version = version,
                        JavaHome = dir,
                        ExecutablePath = javaBin
                    });
                }
            }
        }

        return installations;
    }

    private static List<JavaInstallation> FindJavaOnLinux()
    {
        var installations = new List<JavaInstallation>();
        var paths = new[]
        {
        "/usr/lib/jvm",
        "/opt/java",
        Environment.GetEnvironmentVariable("JAVA_HOME") ?? ""
    };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            if (!Directory.Exists(path)) continue;

            foreach (var dir in Directory.GetDirectories(path))
            {
                var javaBin = Path.Combine(dir, "bin", "java");
                if (File.Exists(javaBin))
                {
                    var version = GetJavaVersion(javaBin);
                    installations.Add(new JavaInstallation
                    {
                        Version = version,
                        JavaHome = dir,
                        ExecutablePath = javaBin
                    });
                }
            }
        }

        return installations;
    }

    private static List<JavaInstallation> FindJavaOnMac()
    {
        var installations = new List<JavaInstallation>();
        var paths = new[]
        {
        "/Library/Java/JavaVirtualMachines",
        "/System/Library/Java/JavaVirtualMachines",
        Environment.GetEnvironmentVariable("JAVA_HOME") ?? ""
    };

        foreach (var path in paths.Where(p => !string.IsNullOrEmpty(p)))
        {
            if (!Directory.Exists(path)) continue;

            foreach (var dir in Directory.GetDirectories(path))
            {
                var javaBin = Path.Combine(dir, "Contents", "Home", "bin", "java");
                if (File.Exists(javaBin))
                {
                    var version = GetJavaVersion(javaBin);
                    installations.Add(new JavaInstallation
                    {
                        Version = version,
                        JavaHome = dir,
                        ExecutablePath = javaBin
                    });
                }
            }
        }

        return installations;
    }

    private static string GetJavaVersion(string javaExecutablePath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaExecutablePath,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            var match = Regex.Match(output, @"version ""([^""]+)""");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }




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