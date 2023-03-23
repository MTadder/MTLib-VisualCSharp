using System.Text;

namespace MTLibrary {
    /// <summary>
    /// 
    /// </summary>
    public static class Terminal {
        /// <summary>
        /// Represents a <see cref="ConsoleColor"/> Style-Sheet to use whilst printing.
        /// </summary>
        public struct Style {
            public ConsoleColor background = ConsoleColor.Black;
            public ConsoleColor foreground = ConsoleColor.White;
            public Style() { }
        }
        public static ConsoleColor PushColor(ConsoleColor fgColor) {
            ConsoleColor old_fg = Console.ForegroundColor;
            Console.ForegroundColor=fgColor;
            return old_fg;
        }
        public static Style PushStyle(Style style) {
            Style old_style = new() {
                background=Console.BackgroundColor,
                foreground=Console.ForegroundColor,
            };
            _=PushColors((style.background, style.foreground));
            return old_style;
        }
        public static (ConsoleColor, ConsoleColor) PushColors((ConsoleColor, ConsoleColor) tup) {
            ConsoleColor old_bg = Console.BackgroundColor;
            ConsoleColor old_fg = Console.ForegroundColor;
            Console.BackgroundColor=tup.Item1;
            Console.ForegroundColor=tup.Item2;
            return (old_bg, old_fg);
        }
        public static void PrintChar(char ch) {
            Console.Write(ch);
        }
        public static void PrintString(string msg) {
            Console.Write(msg);
        }
        public static void Print(string msg, Style style) {
            var old_Style = PushStyle(style);
            PrintString(msg);
            _=PushStyle(old_Style);
        }
        public static void Print(string msg, ConsoleColor color) {
            var old_color = PushColor(color);
            PrintString(msg);
            _=PushColor(old_color);
        }
        public class Menu {
            #region Properties
            /// <summary>
            /// This controls the color scheme of default text in Menus.
            /// text such as Title, Description, and Body will be colored with this.
            /// </summary>
            public Style MasterStyle = new();
            /// <summary>
            /// This controls the color scheme of the text that is entered by the user,
            /// when <see cref="Prompt"/> is called.
            /// </summary>
            public Style PromptStyle = new();
            public string Title = string.Empty;
            public string Description = string.Empty;
            public string InvalidInputMsg = "Invalid operation.";
            public int InvalidInputWaitMS = 1000;
            public string Carriage = ">: ";
            public bool Locked;
            public bool AllowHelp = true;
            /// <summary>
            /// If true, the Console will be cleared before each <see cref="Draw"/>.
            /// </summary>
            public bool ClearConsole = true;
            public Dictionary<string, Menu> Triggers = new();
            public Dictionary<string, Action<Menu, string?>> Hooks = new();
            #endregion
            #region Internals
            protected internal Menu? LastMenu;
            #endregion
            #region Constructors
            public Menu(string title, string description = "", bool isLocked = false) {
                this.Title=title;
                this.Description=description;
                this.Locked=isLocked;
            }
            public Menu() { }
            #endregion
            #region Methods
            public bool Prompt() {
                this.Draw();
                var style = PushStyle(this.PromptStyle);
                string? input = Console.ReadLine();
                _=PushStyle(style);
                if (string.IsNullOrEmpty(input)) { return false; }
                bool called = false;
                foreach (var pair in this.Hooks) {
                    if (input.StartsWith(pair.Key)) {
                        string? arg = null;
                        string aarg = input.Replace(pair.Key, string.Empty).Trim();
                        if (aarg.Length>0) { arg=aarg; } // there's probably a much better way to do this.
                        pair.Value.Invoke(this, arg);
                        called=true;
                        break;
                    }
                }
                if (called) return true; // Ignore Triggers if a Hook was called.
                foreach (KeyValuePair<string, Menu> pair in this.Triggers) {
                    if (pair.Key.Equals(input)) {
                        this.StepForward(pair.Value);
                        return true;
                    }
                }
                if (this.AllowHelp&&input.Equals("/help")||input.Equals("/?")) {
                    this.PrintHelp();
                    PrintString("Press [ENTER] to continue.\n");
                    _=Console.ReadLine();
                    return true;
                }
                PrintString(this.InvalidInputMsg+"\n");
                Thread.Sleep(this.InvalidInputWaitMS);
                return false;
            }
            private void Draw() {
                if (this.ClearConsole) Console.Clear();
                _=PushStyle(this.MasterStyle);
                PrintString($"\t\t{this.Title}\n\t{this.Description}\n");
                PrintString($"{this.GetChoices()}\n");
                Print(this.Carriage, ConsoleColor.Blue);
            }
            public void PrintHelp() {
                //StringBuilder help = new();
                if (this.Hooks.Count>0) {
                    PrintString("Hooks (");
                    var cols = PushColors((ConsoleColor.Black, ConsoleColor.Blue));
                    PrintString(this.Hooks.Count.ToString());
                    _=PushColors(cols);
                    PrintString(")\n");
                    foreach (var hook in this.Hooks) {
                        PrintString($"\n\t[");
                        var col = PushColor(ConsoleColor.Cyan);
                        PrintString(hook.Key);
                        _=PushColor(col);
                        PrintString("]");
                    }
                    PrintString("\n");
                }
            }
            public string GetChoices() {
                StringBuilder choices = new();
                foreach (KeyValuePair<string, Menu> t in this.Triggers) {
                    if (!t.Value.Locked) {
                        _=choices.AppendLine($"[{t.Key}] -> {t.Value.Title}");
                    }
                }
                return choices.ToString();
            }
            public void StepBack() {
                if (this.LastMenu==null||this.Locked) { return; }
                // Texts
                this.Title=this.LastMenu.Title;
                this.Description=this.LastMenu.Description;
                this.Carriage=this.LastMenu.Carriage;
                this.InvalidInputMsg=this.LastMenu.InvalidInputMsg;

                // Data-Values
                this.InvalidInputWaitMS=this.LastMenu.InvalidInputWaitMS;

                // Actions
                this.Triggers=this.LastMenu.Triggers;
                this.Hooks=this.LastMenu.Hooks;

                // Styles
                this.MasterStyle=this.LastMenu.MasterStyle;
                this.PromptStyle=this.LastMenu.PromptStyle;

                // Finally
                this.LastMenu=this.LastMenu.LastMenu;
            }
            public void StepForward(Menu nextMenu) {
                if (this.Locked) { return; }
                nextMenu.Locked=false;

                this.LastMenu=(Menu) this.MemberwiseClone();
                (this.Title, this.Description)=(nextMenu.Title, nextMenu.Description);
                this.Carriage=nextMenu.Carriage;
                this.Triggers=nextMenu.Triggers;
                this.Hooks=nextMenu.Hooks;
            }
            #endregion
        }
    }
}
