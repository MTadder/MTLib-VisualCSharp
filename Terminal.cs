using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace MTLibrary {
    public static class Terminal {
        public static async void TypeWrite(string msg, int maxInterval) {
            foreach (char c in msg.ToCharArray()) {
                PrintChar(c);
                if (char.IsWhiteSpace(c).Equals(false)) {
                    await Task.Delay(RandomNumberGenerator.GetInt32(maxInterval));
                }
            }
        }
        public static void PrintChar(char ch) {
            Console.Write(ch);
        }
        public static void PrintString(string msg) {
            Console.Write(msg);
        }
        public class Menu {
            public class Style {
                protected internal ConsoleColor last_fg;
                protected internal ConsoleColor last_bg;
                public ConsoleColor foreground = ConsoleColor.White;
                public ConsoleColor background = ConsoleColor.Black;
                public Style() { }
                public void Push() { }
                public void Pop() { }
            }
            #region Properties
            public Style MasterStyle;
            public Style PromptStyle;
            public bool TypeWriterEnabled;
            public int TypeWriterWaitMS;
            public string Title = "";
            public string Description = "";
            public string InvalidTriggerMsg = "Invalid operation.";
            public string Carriage = ">: ";
            public bool Locked;
            public Dictionary<string, Menu> Triggers = new();
            public Dictionary<string, Action> Actions = new();
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
            public void Print(string msg) {
                ConsoleColor fgColor = Console.ForegroundColor;
                ConsoleColor bgColor = Console.BackgroundColor;
                Console.ForegroundColor=this.MasterStyle.foreground;
                Console.BackgroundColor=this.MasterStyle.background;
                if (this.TypeWriterEnabled) {
                    TypeWrite(msg, this.TypeWriterWaitMS);
                } else { PrintString(msg); }
                (Console.ForegroundColor, Console.BackgroundColor) = (fgColor, bgColor);
            }
            public bool Prompt() {
                this.Draw();
                ConsoleColor fgColor = Console.ForegroundColor;
                ConsoleColor bgColor = Console.BackgroundColor;
                Console.ForegroundColor=this.PromptStyle.foreground;
                Console.BackgroundColor=this.PromptStyle.background;
                string? input = Console.ReadLine();
                (Console.ForegroundColor, Console.BackgroundColor)=(fgColor, bgColor);
                if (string.IsNullOrEmpty(input)) { return false; }
                bool called = false;
                foreach (KeyValuePair<string, Action> pair in this.Actions) {
                    if (pair.Key.Equals(input)) {
                        pair.Value.Invoke();
                        called = true;
                    }
                }
                if (called) return true;
                foreach (KeyValuePair<string, Menu> pair in this.Triggers) {
                    if (pair.Key.Equals(input)) {
                        this.StepForward(pair.Value);
                        return true;
                    }
                }
                if (input.Equals("/help")||input.Equals("/?")) {
                    Print($"Actions:");
                    Print(this.GetHelp());
                    Print("\nPress [ENTER] to continue.");
                    _=Console.ReadLine();
                    return true;
                }
                if (input.Equals("/b")) { this.StepBack(); return true; }
                Print($"{this.InvalidTriggerMsg} Type [/b] to go back.");
                Console.Beep();
                Thread.Sleep(1000);
                return false;
            }
            public void Draw() {
                Console.Clear();
                Print($"\t\t{this.Title}\n\t{this.Description}\n");
                Print($"{this.GetChoices()}\n");
                Print(this.Carriage);
            }
            public string GetHelp() {
                StringBuilder help = new();
                foreach (KeyValuePair<string, Action> h in this.Actions) {
                    _=help.Append($"\n\t[{h.Key}]");
                }
                return help.ToString();
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
                (this.Title, this.Description)=(this.LastMenu.Title, this.LastMenu.Description);
                this.Actions=this.LastMenu.Actions;
                (this.Triggers, this.LastMenu)=(this.LastMenu.Triggers, this.LastMenu.LastMenu);
            }
            public void StepForward(Menu nextMenu) {
                if (this.Locked) { return; }
                nextMenu.Locked=false;
                this.LastMenu=(Menu) this.MemberwiseClone();
                (this.Title, this.Description)=(nextMenu.Title, nextMenu.Description);
                this.Triggers=nextMenu.Triggers;
                this.Actions=nextMenu.Actions;
            }
            #endregion
        }
    }
}
