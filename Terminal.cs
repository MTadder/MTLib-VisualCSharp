using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace MTLibrary {
    /// <summary>
    /// 
    /// </summary>
    public static class Terminal {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        public static void PrintChar(char ch) {
            Console.Write(ch);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static void PrintString(string msg) {
            Console.Write(msg);
        }
        /// <summary>
        /// 
        /// </summary>
        public class Menu {
            /// <summary>
            /// Represents a <see cref="ConsoleColor"/> Style-Sheet to use whilst <see cref="Draw"/>ing.
            /// </summary>
            public struct Style {
                public ConsoleColor foreground = ConsoleColor.White;
                public ConsoleColor background = ConsoleColor.Black;
                public Style() { }
            }
            #region Properties
            /// <summary>
            /// 
            /// </summary>
            public Style MasterStyle = new();
            /// <summary>
            /// 
            /// </summary>
            public Style PromptStyle = new();
            /// <summary>
            /// 
            /// </summary>
            public string Title = string.Empty;
            /// <summary>
            /// 
            /// </summary>
            public string Description = string.Empty;
            /// <summary>
            /// 
            /// </summary>
            public string InvalidTriggerMsg = "Invalid operation.";
            /// <summary>
            /// 
            /// </summary>
            public string Carriage = ">: ";
            /// <summary>
            /// 
            /// </summary>
            public bool Locked;
            public bool ClearConsole;
            /// <summary>
            /// 
            /// </summary>
            public Dictionary<string, Menu> Triggers = new();
            /// <summary>
            /// 
            /// </summary>
            public Dictionary<string, Action> Actions = new();
            /// <summary>
            /// Like <see cref="Actions"/>, except the current <see cref="Menu"/>,
            /// and the rest of the user's input string are passed as arguments to the <see cref="Action{,}"/>.
            /// </summary>
            public Dictionary<string, Action<Menu, string?>> Hooks = new();
            #endregion
            #region Internals
            /// <summary>
            /// 
            /// </summary>
            protected internal Menu? LastMenu;
            #endregion
            #region Constructors
            /// <summary>
            /// 
            /// </summary>
            public Menu(string title, string description = "", bool isLocked = false) {
                this.Title=title;
                this.Description=description;
                this.Locked=isLocked;
            }
            /// <summary>
            /// 
            /// </summary>
            public Menu() { }
            #endregion
            #region Methods
            public static ConsoleColor PushColor(ConsoleColor fgColor) {
                ConsoleColor old_fg = Console.ForegroundColor;
                Console.ForegroundColor = fgColor;
                return old_fg;
            }
            public static void PopColor(ConsoleColor fgColor) {
                Console.ForegroundColor = fgColor;
            }
            public static (ConsoleColor, ConsoleColor) PushColors(ConsoleColor bg, ConsoleColor fg) {
                ConsoleColor old_bg = Console.BackgroundColor;
                ConsoleColor old_fg = Console.ForegroundColor;
                Console.BackgroundColor=bg;
                Console.ForegroundColor=fg;
                return (old_bg, old_fg);
            }

            public static void PopColors((ConsoleColor, ConsoleColor) tup) {
                Console.BackgroundColor=tup.Item1;
                Console.ForegroundColor=tup.Item2;
            }
            /// <summary>
            /// Applies the <see cref="Menu.MasterStyle"/>, and prints the provided string.
            /// </summary>
            /// <param name="msg"></param>
            public void Print(string msg) {
                var cols = PushColors(this.MasterStyle.background, this.MasterStyle.foreground);
                PrintString(msg);
                PopColors(cols);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool Prompt() {
                this.Draw();
                var cols = PushColors(this.PromptStyle.background, this.PromptStyle.foreground);
                string? input = Console.ReadLine();
                PopColors(cols);
                if (string.IsNullOrEmpty(input)) { return false; }
                bool called = false;
                foreach (KeyValuePair<string, Action> pair in this.Actions) {
                    if (pair.Key.Equals(input)) {
                        pair.Value.Invoke();
                        called = true;
                    }
                }
                foreach (var pair in this.Hooks) {
                    if (input.StartsWith(pair.Key)) {
                        string? arg = null;
                        string aarg = input.Replace(pair.Key, string.Empty).Trim();
                        if (aarg.Length>0) { arg = aarg; } // there's probably a much better way to do this.
                        pair.Value.Invoke(this, arg);
                        called = true;
                    }
                    if (called) break; // Don't call more than one Hook at a time.
                }
                if (called) return true; // Ignore Triggers if a Action or Hook was called.
                foreach (KeyValuePair<string, Menu> pair in this.Triggers) {
                    if (pair.Key.Equals(input)) {
                        this.StepForward(pair.Value);
                        return true;
                    }
                }
                if (input.Equals("/help")||input.Equals("/?")) {
                    this.PrintHelp();
                    Print("Press [ENTER] to continue.\n");
                    _=Console.ReadLine();
                    return true;
                }
                if (input.Equals("/b")) { this.StepBack(); return true; } // is this necessary?
                Print($"{this.InvalidTriggerMsg} Type [/b] to go back.\n\n");
                Console.Beep();
                Thread.Sleep(1000);
                return false;
            }
            /// <summary>
            /// 
            /// </summary>
            public void Draw() {
                if (this.ClearConsole) Console.Clear();
                this.Print($"\t\t{this.Title}\n\t{this.Description}\n");
                this.Print($"{this.GetChoices()}\n");
                this.Print(this.Carriage);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public void PrintHelp() {
                //StringBuilder help = new();
                if (this.Hooks.Count>0) {
                    this.Print("Hooks (");
                    var cols = PushColors(ConsoleColor.Black, ConsoleColor.Blue);
                    PrintString(this.Hooks.Count.ToString());
                    PopColors(cols);
                    this.Print(")\n");
                    foreach (var hook in this.Hooks) {
                        this.Print($"\n\t[");
                        var col = PushColor(ConsoleColor.Cyan);
                        PrintString(hook.Key);
                        PopColor(col);
                        this.Print("]");
                    }
                    this.Print("\n");
                }
                    //    foreach (KeyValuePair<string, Action> action in this.Actions) {
                    //        _=help.Append($"\n\t[{action.Key}]");
                    //    }
                    //}
                    //if (this.Hooks.Count > 0) {
                    //    _=help.AppendLine($"\nHooks ({this.Hooks.Count}): ");
                    //    foreach (var hook in this.Hooks) {
                    //        _=help.AppendLine($"\n\t[{hook.Key}]");
                    //    }
                    //}
                    //return help.ToString();
                }
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public string GetChoices() {
                StringBuilder choices = new();
                foreach (KeyValuePair<string, Menu> t in this.Triggers) {
                    if (!t.Value.Locked) {
                        _=choices.AppendLine($"[{t.Key}] -> {t.Value.Title}");
                    }
                }
                return choices.ToString();
            }
            /// <summary>
            /// 
            /// </summary>
            public void StepBack() {
                if (this.LastMenu==null||this.Locked) { return; }
                (this.Title, this.Description)=(this.LastMenu.Title, this.LastMenu.Description);
                this.Carriage=this.LastMenu.Carriage;
                this.Triggers=this.LastMenu.Triggers;
                this.Actions=this.LastMenu.Actions;
                this.Hooks=this.LastMenu.Hooks;
                
                this.LastMenu = this.LastMenu.LastMenu;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="nextMenu"></param>
            public void StepForward(Menu nextMenu) {
                if (this.Locked) { return; }
                nextMenu.Locked=false;

                this.LastMenu=(Menu) this.MemberwiseClone();
                (this.Title, this.Description)=(nextMenu.Title, nextMenu.Description);
                this.Carriage=nextMenu.Carriage;
                this.Triggers=nextMenu.Triggers;
                this.Actions=nextMenu.Actions;
                this.Hooks=nextMenu.Hooks;
            }
            #endregion
        }
    }
}
