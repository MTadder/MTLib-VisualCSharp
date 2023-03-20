using System.Security.Cryptography;
using System.Text;

namespace MTLibrary {
    public static class Terminal {
        public static async void TypeWrite(string msg, int maxInterval, ConsoleColor col = ConsoleColor.White) {
            foreach (char c in msg.ToCharArray()) {
                Put(c.ToString(), col);
                if (char.IsWhiteSpace(c).Equals(false)) {
                    await Task.Delay(RandomNumberGenerator.GetInt32(maxInterval));
                }
            }
        }
        public static void Write(string msg, ConsoleColor col = ConsoleColor.White) {
            TypeWrite(msg, 2, col);
        }
        public static void Put(string msg, ConsoleColor col = ConsoleColor.White) {
            ConsoleColor lastCol = Console.ForegroundColor;
            Console.ForegroundColor=col;
            Console.Write(msg);
            Console.ForegroundColor=lastCol;
        }
        public class Menu {
            #region Properties
            public string Title;
            public string Description;
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
            public Menu(string title, string description, bool isLocked = false) {
                this.Title=title;
                this.Description=description;
                this.Locked=isLocked;
            }
            #endregion
            #region Methods
            public bool Prompt() {
                this.Draw();
                string? input = Console.ReadLine();
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
                    Write($"Actions:");
                    Write(this.GetHelp(), ConsoleColor.Gray);
                    Write("\nPress [ENTER] to continue.", ConsoleColor.Gray);
                    _=Console.ReadLine();
                    return true;
                }
                if (input.Equals("/b")) { this.StepBack(); return true; }
                Write($"{this.InvalidTriggerMsg} Type [/b] to go back.", ConsoleColor.DarkRed);
                Console.Beep();
                Thread.Sleep(1000);
                return false;
            }
            public void Draw() {
                Console.Clear();
                Write($"\t\t{this.Title}\n", ConsoleColor.Cyan);
                Write($"{this.GetChoices()}\n", ConsoleColor.DarkGreen);
                Write(this.Carriage, ConsoleColor.Green);
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
                        _=choices.AppendLine($"[{t.Key}] -> {t.Value.Title} ({t.Value.Description})");
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
                this.LastMenu=this;
                (this.Title, this.Description)=(nextMenu.Title, nextMenu.Description);
                this.Triggers=nextMenu.Triggers;
                this.Actions=nextMenu.Actions;
            }
            #endregion
        }
    }
}
