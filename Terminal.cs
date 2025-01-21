using System.Security;
using System.Security.Cryptography;

using MenuHook = System.Action<MTLib.Terminal.Menu, System.String?>;

namespace MTLib;

public static class Terminal {
    public interface IConsoleWriter {
        void Write(String text);
        void WriteLine(String text);
        void Clear();
    }
    public sealed class NormalConsoleWriter: IConsoleWriter {
        public void Write(String text) => Console.Write(text);
        public void WriteLine(String text) => Console.WriteLine(text);
        public void Clear() => Console.Clear();
    }
    public sealed class TypewriterConsoleWriter: IConsoleWriter {
        public (Int32 min, Int32 max) WaitRange = (5, 64);
        public TypewriterConsoleWriter(Int32 minCharWait, Int32 maxCharWait) {
            this.WaitRange = (minCharWait, maxCharWait);
        }
        public TypewriterConsoleWriter(Int32 maxCharWait) {
            this.WaitRange.max = maxCharWait;
        }
        public TypewriterConsoleWriter() { }
        private Int32 _GetWaitMS() {
            return this.WaitRange.max < 5
                ? 0
                : RandomNumberGenerator.GetInt32(
                this.WaitRange.min, this.WaitRange.max);
        }
        public void Write(String text) {
            ArgumentNullException.ThrowIfNull(text);
            foreach (Char current in text) {
                Console.Write(current);
                Thread.Sleep(this._GetWaitMS());
            }
        }
        public void WriteLine(String text) {
            this.Write(text);
            Console.Write('\n');
        }
        public void Clear() {
            Console.Clear();
            //Thread.Sleep(getWaitMS() * Console.WindowHeight * Console.WindowWidth);
        }
    }
    public static class Writers {
        public static readonly NormalConsoleWriter NormalConsoleWriter = new();
        public static readonly TypewriterConsoleWriter TypewriterConsoleWriter = new();
    }
    /// <summary>
    /// Represents a <see cref="ConsoleColor"/> Style-Sheet to use whilst printing.
    /// </summary>
    public sealed class Style {
        public ConsoleColor Foreground = ConsoleColor.White;
        public ConsoleColor Background = ConsoleColor.Black;
        /// <summary>
        /// Instantiates a new <see cref="Style"/>, with default parameters.
        /// </summary>
        public Style() { }

        public Style(ConsoleColor? fg = null, ConsoleColor? bg = null) {
            this.Foreground = fg ?? this.Foreground;
            this.Background = bg ?? this.Background;
        }

        /// <summary>
        /// Applies this <see cref="Style"/> to the <see cref="Console"/>.
        /// </summary>
        /// <returns>The previous <see cref="Style"/> that was being used.</returns>
        public Style Push() {
            Style oldStyle = new(Console.ForegroundColor, Console.BackgroundColor);
            Console.ForegroundColor = this.Foreground;
            Console.BackgroundColor = this.Background;
            return oldStyle;
        }
        /// <summary>
        /// Writes a <see cref="String"/> to the <see cref="Console"/>, using this
        /// <see cref="Style"/>.
        /// </summary>
        /// <param name="msg">The <see cref="String"/> to write.</param>
        /// <param name="writer">The <see cref="IConsoleWriter"/> to use for writing.</param>
        public void Write(String msg, IConsoleWriter writer) {
            ArgumentNullException.ThrowIfNullOrEmpty(msg);
            writer ??= Writers.NormalConsoleWriter;
            Style old_Style = this.Push();
            writer.Write(msg);
            _ = old_Style.Push();
        }
        public void WriteLine(String msg, IConsoleWriter writer) {
            ArgumentNullException.ThrowIfNullOrEmpty(msg);
            writer ??= Writers.NormalConsoleWriter;
            Style old_Style = this.Push();
            writer.WriteLine(msg);
            _ = old_Style.Push();
        }

        /// <summary>
        /// Implicitly casts a <see cref="ConsoleColor"/> <see cref="Tuple"/> from
        /// a <see cref="Style"/> instance.
        /// </summary>
        /// <param name="style">The <see cref="Style"/> to implicitly cast.</param>
        public static implicit operator (ConsoleColor fg, ConsoleColor bg)(Style style) {
            ArgumentNullException.ThrowIfNull(style);
            return new(style.Foreground, style.Background);
        }

        /// <summary>
        /// Explicitly casts a <see cref="Style"/> from a
        /// <see cref="ConsoleColor"/> <see cref="Tuple"/>.
        /// </summary>
        /// <param name="tuple">The <see cref="Tuple"/> to cast.</param>
        public static explicit operator Style((ConsoleColor fg, ConsoleColor bg) tuple) {
            return new() { Foreground = tuple.fg, Background = tuple.bg };
        }

        /// <summary>
        /// Explicitly casts a <see cref="Style"/> from a
        /// <see cref="ConsoleColor"/> instance.
        /// The background parameter is defaulted.
        /// </summary>
        /// <param name="color">The <see cref="ConsoleColor"/></param>
        public static explicit operator Style(ConsoleColor color) {
            return new(color);
        }

        public (ConsoleColor fg, ConsoleColor bg) ToValueTuple() {
            throw new NotImplementedException();
        }

        public static Style ToStyle(Style fg, Style bg) {
            return new() { Foreground = fg, Background = bg };
            throw new NotImplementedException();
        }

    }
    public class Menu {
        #region Properties
        /// <summary>
        /// This controls the color scheme of default text in Menus.
        /// text such as Title, Description, and Body will be colored with this.
        /// </summary>
        public Style MasterStyle = new();
        public IConsoleWriter Writer = Writers.NormalConsoleWriter;
        /// <summary>
        /// This controls the color scheme of the text that is entered by the user,
        /// when <see cref="Prompt"/> is called.
        /// </summary>
        public Style PromptStyle = new();
        public String Title = String.Empty;
        public String Description = String.Empty;
        public String InvalidInputMsg = "Invalid operation.";
        public Int32 InvalidInputWaitMS = 1000;
        public Boolean AllowTriggerByIndex = false;
        //public Int32 WaitMSPerChar = 0;
        public String Carriage = ">: ";
        public Style CarriageStyle = new();
        public String[] HelpKeys = ["/h", "/help", "help", "/?"];
        public Boolean Locked;
        public Boolean AllowHelp = true;
        /// <summary>
        /// If true, the Console will be cleared before each <see cref="Draw"/>.
        /// </summary>
        public Boolean ClearConsole = true;
        /// <summary>
        /// Keywords that 'Trigger' forward movement into specified <see cref="Menu"/>s
        /// </summary>
        public Dictionary<String, Menu> Triggers = [];
        /// <summary>
        /// Command-like 'Hooks' each call an <see cref="Action{Menu, String?}"/>.
        /// </summary>
        public Dictionary<String, MenuHook> Hooks = [];
        #endregion
        #region Internals
        /// <summary>
        /// Represents this instance's last used <see cref="Menu"/>. It is a shallow copy of it.
        /// </summary>
        protected internal Menu? LastMenu;
        #endregion
        #region Constructors
        public Menu(String title, String description = "", Boolean isLocked = false) {
            this.Title = title;
            this.Description = description;
            this.Locked = isLocked;
        }
        public Menu() { }
        #endregion
        #region Methods
        /// <summary>
        /// Draws this <see cref="Menu"/> and then awaits for
        /// a line to be entered by the user, and calls the according
        /// <c>Triggers</c> and <c>Hooks</c>
        /// </summary>
        /// <returns><c>True</c>, if either was properly called</returns>
        public Boolean Prompt() {
            this.Draw();
            Style style = this.PromptStyle.Push();
            String? input = Console.ReadLine();
            _ = style.Push();
            if (String.IsNullOrEmpty(input)) { return false; }
            foreach (KeyValuePair<String, MenuHook> pair in this.Hooks) {
                if (input.StartsWith(pair.Key)) {
                    String args = input.Replace(pair.Key, String.Empty).Trim();
                    pair.Value.Invoke(this, (args.Length > 0) ? args : null);
                    return true;
                }
            }
            Int32 i = 0;
            foreach (KeyValuePair<String, Menu> pair in this.Triggers) {
                if (AllowTriggerByIndex && int.TryParse(input, out Int32 iInput)) {
                    if (iInput != i)
                        continue;
                    this.StepForward(pair.Value);
                    return true;
                }
                if (pair.Key.Equals(input)) {
                    this.StepForward(pair.Value);
                    return true;
                }
                i++;
            }
            if (this.AllowHelp && this.HelpKeys.Contains(input)) {
                this.PrintHelp();
                Macros.Pause.Invoke(this, null);
                return true;
            }
            this.Writer.WriteLine(this.InvalidInputMsg);
            Thread.Sleep(this.InvalidInputWaitMS);
            return false;
        }
        /// <summary>
        /// Writes the <see cref="Menu"/> to the <see cref="IConsoleWriter"/>, using a sane
        /// and simplistic formatting.
        /// </summary>
        public void Draw() {
            if (this.ClearConsole)
                this.Writer.Clear();
            Style old_style = this.MasterStyle.Push();
            this.Writer.Write($"\t\t{this.Title}\n\t{this.Description}\n");
            this.PrintChoices();
            this.CarriageStyle.Write(this.Carriage, this.Writer);
            _ = old_style.Push();
        }
        public void PrintHelp() {
            if (this.Hooks.Count > 0) {
                Style oldStyle = this.MasterStyle.Push();
                this.Writer.Write("Hooks (");
                this.PromptStyle.Write(this.Hooks.Count.ToString(), this.Writer);
                this.Writer.Write(")");
                foreach (var hook in this.Hooks) {
                    this.Writer.Write($"\n\t[");
                    this.PromptStyle.Write(hook.Key, this.Writer); // Print(hook.Key, );
                    this.Writer.Write("]");
                }
                this.Writer.Write("\n");
                _ = oldStyle.Push();
            }
        }
        public void PrintChoices() {
            Int32 i = 0;
            foreach (KeyValuePair<String, Menu> t in this.Triggers) {
                if (!t.Value.Locked) {
                    if (AllowTriggerByIndex) {
                        this.Writer.Write("[");
                        this.PromptStyle.Write(i.ToString(), this.Writer);
                        this.Writer.Write("]");
                    }
                    this.Writer.Write("[");
                    this.PromptStyle.Write(t.Key, this.Writer);
                    this.Writer.WriteLine($"] -> {t.Value.Title}");
                    i++;
                }
            }
        }
        public void StepBack() {
            if (this.LastMenu is null || this.Locked) { return; }
            Step(this.LastMenu, this);
            this.LastMenu = this.LastMenu.LastMenu;
        }
        public void StepForward(Menu nextMenu) {
            if (this.Locked) { return; }
            this.LastMenu = (Menu) this.MemberwiseClone();
            Step(nextMenu, this);
        }
        private static void Step(Menu from, Menu to) {
            // Texts
            to.Title = from.Title;
            to.Description = from.Description;
            to.Carriage = from.Carriage;
            to.InvalidInputMsg = from.InvalidInputMsg;

            // Data-Values
            to.InvalidInputWaitMS = from.InvalidInputWaitMS;

            // Actions
            to.Triggers = from.Triggers;
            to.Hooks = from.Hooks;

            // Styles
            to.MasterStyle = from.MasterStyle;
            to.PromptStyle = from.PromptStyle;
        }
        #endregion
        /// <summary>
        /// Provides static <see cref="Menu"/>-related fields (mostly <c>MenuHooks</c>)
        /// that aid development speed, and ease of use.
        /// </summary>
        public static class Macros {
            public static readonly MenuHook Back = new((Menu m, String? input) => {
                m.StepBack();
            });
            /// <summary>
            /// Calls <code>Environment.Exit(0);</code> while catching any security violations.
            /// </summary>
            public static readonly MenuHook Exit = new((Menu m, String? input) => {
                try {
                    Environment.Exit(0);
                } catch (SecurityException e) {
                    m.Writer.Write($"Error: {e.Message}");
                }
            });
            public static readonly MenuHook Pause = new((Menu m, String? input) => {
                m.Writer.Write("Press [");
                m.PromptStyle.Write("ENTER", m.Writer);
                Console.Write("] to continue.");
                _ = Console.ReadLine();
            });
            public static readonly MenuHook Echo = new((Menu m, String? input) => {
                if (input is null) {
                    m.Writer.Write("\n");
                    return;
                } else {
                    m.Writer.WriteLine(input);
                }
            });
            public static readonly MenuHook Tree = new((Menu m, String? input) => {
                if (input is null) { return; }
                if (!Directory.Exists(input)) { return; }
                if (input.Contains("-p") || input.Contains("--pause")) {
                    Pause.Invoke(m, input);
                }
                throw new NotImplementedException();
            });
        }
    }
}
