using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MTLibrary {
    public static class Terminal {
        public static void TypeWrite(String msg, int maxInterval, ConsoleColor col = ConsoleColor.White) {
            foreach (Char c in msg.ToCharArray()) {
                Put(c.ToString(), col);
                if (!Char.IsWhiteSpace(c)) {
                    Thread.Sleep(RandomNumberGenerator.GetInt32(maxInterval));
                }
            }
        }
        public static void Write(String msg, ConsoleColor col = ConsoleColor.White) {
            TypeWrite(msg, 2, col);
        }
        public static void Put(String msg, ConsoleColor col = ConsoleColor.White) {
            ConsoleColor lastCol = Console.ForegroundColor;
            Console.ForegroundColor = col;
            Console.Write(msg);
            Console.ForegroundColor = lastCol;
        }
        public class Menu {
            #region Properties
            public String Title = String.Empty;
            public String Description = String.Empty;
            public String InvalidTriggerMsg = "Invalid operation.";
            public String Carriage = ">: ";
            public Boolean Locked = false;
            public Dictionary<String, Menu> Triggers = new();
            public Dictionary<String, Action> Actions = new();
            #endregion
            #region Internals
            internal Menu? LastMenu = null;
            #endregion
            #region Constructors
            public Menu(String title, String description, Boolean isLocked = false) {
                this.Title = title;
                this.Description = description;
                this.Locked = isLocked;
            }
            #endregion
            #region Methods
            public bool Prompt() {
                Draw();
                String? input = Console.ReadLine();
                if (String.IsNullOrEmpty(input)) { input = ""; }
                if (this.Actions.ContainsKey(input)) { this.Actions[input]?.Invoke(); return true; }
                if (this.Triggers.ContainsKey(input)) { StepForward(this.Triggers[input]); return true; }
                if (input.Equals("/help") || input.Equals("/?")) {
                    Write($"Actions:");
                    Write(GetHelp(), ConsoleColor.Gray);
                    Write("\nPress [ENTER] to continue.", ConsoleColor.Gray);
                    _ = Console.ReadLine();
                    return true;
                }
                if (input.Equals("/b")) { StepBack(); return true; }
                Write($"{this.InvalidTriggerMsg} Type [/b] to go back.", ConsoleColor.DarkRed);
                Console.Beep();
                Thread.Sleep(1000);
                return false;
            }
            public void Draw() {
                Console.Clear();
                Write($"\t\t{this.Title}\n", ConsoleColor.Cyan);
                Write($"{GetChoices()}\n", ConsoleColor.DarkGreen);
                Write(this.Carriage, ConsoleColor.Green);
            }
            public String GetHelp() {
                StringBuilder help = new();
                foreach (KeyValuePair<String, Action> h in this.Actions) {
                    _ = help.Append($"\n\t[{h.Key}]");
                }
                return help.ToString();
            }
            public String GetChoices() {
                StringBuilder choices = new();
                foreach (KeyValuePair<String, Menu> t in this.Triggers) {
                    if (!t.Value.Locked) {
                        _ = choices.AppendLine($"[{t.Key}] -> {t.Value.Title} ({t.Value.Description})");
                    }
                } return choices.ToString();
            }
            public void StepBack() {
                if (this.LastMenu == null || this.Locked) { return; }
                (this.Title, this.Description) = (this.LastMenu.Title, this.LastMenu.Description);
                this.Actions = this.LastMenu.Actions;
                (this.Triggers, this.LastMenu) = (this.LastMenu.Triggers, this.LastMenu.LastMenu);
            }
            public void StepForward(Menu nextMenu) {
                if (this.Locked) { return; }
                nextMenu.Locked = false;
                this.LastMenu = (Menu) this.MemberwiseClone();
                (this.Title, this.Description) = (nextMenu.Title, nextMenu.Description);
                this.Triggers = nextMenu.Triggers;
                this.Actions = nextMenu.Actions;
            }
            #endregion
        }
    }
}
