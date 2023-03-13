using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MTLib
{
    public class Interaction
    {
        interface ILayer
        {
            void Draw();
            bool Input(int Choice);
        }
        public class LayerLockedException : Exception { }
        public class LayerLock
        {
            public bool Locked { private set; get; }
            private string Key;

            /// <summary>
            /// Attempt to Lock with new Key and returns Success
            /// </summary>
            /// <param name="Key">new Key</param>
            /// <returns>Success</returns>
            public bool Lock(string Key)
            {
                if (this.Locked)
                {
                    return false;
                }
                else
                {
                    this.Key = Key;
                    this.Locked = true;
                    return true;
                }
            }

            /// <summary>
            /// Attempts to Unlock with Key and returns Success
            /// </summary>
            /// <param name="Key">Key</param>
            /// <returns>Success</returns>
            public bool Unlock(string Key)
            {
                if (this.Locked)
                {
                    if (this.Key.Equals(Key))
                    {
                        this.Locked = false;
                        return true; // Just right
                    }
                    else
                    {
                        return false; // Wrong Key
                    }
                }
                else
                {
                    return true; // Already unlocked
                }
            }
        }

        public class LayerProperties // Allow for maximum customizablility
        {
            public string Title;
            public ConsoleColor TitleForeColor = ConsoleColor.White;
            public ConsoleColor TitleBackColor = ConsoleColor.Black;

            public char Underline = '-';

            public Action DrawContent;

            public ConsoleColor ActivatorForeColor = ConsoleColor.White;
            public ConsoleColor ActivatorBackColor = ConsoleColor.Black;

            public ConsoleColor SelectableForeColor = ConsoleColor.White;
            public ConsoleColor SelectableBackColor = ConsoleColor.Black;

            public bool QueryChoice = true;
            public int NoChoiceTimeout = 5000;
        }
        public class Layer : ILayer
        {
            private int ID = -1;
            public Dictionary<string, Action> Selectables = new Dictionary<string, Action>();

            public LayerProperties Properties = new LayerProperties();
            public LayerLock Lock = new LayerLock();

            public Layer(LayerProperties Props, int ID)
            {
                this.Properties = Props;
                this.ID = ID;
            }
            public Layer(int ID)
            { this.ID = ID; }

            public void Draw()
            {
                /// Check Lock
                if (this.Lock.Locked)
                {
                    throw new LayerLockedException();
                }

                /// Draw Title
                Write(this.Properties.Title, this.Properties.TitleForeColor, this.Properties.TitleBackColor);
                Write();
                for (int i = 0; i < this.Properties.Title.Length; i++)
                {
                    Write(this.Properties.Underline.ToString());
                }
                Write("\n\n");

                /// Invoke Content Drawer
                this.Properties.DrawContent.Invoke();

                /// Draw Selectables
                Dictionary<string, Action>.Enumerator E = this.Selectables.GetEnumerator();
                E.MoveNext();
                for (int i = 0; i < this.Selectables.Count; i++)
                {
                    /// Draw Activator
                    Write("[" + i + "]", this.Properties.ActivatorForeColor, this.Properties.ActivatorBackColor);
                    Write(": " + E.Current.Key + "\n", this.Properties.SelectableForeColor, this.Properties.SelectableBackColor);
                }
            }

            public bool Input(int Choice)
            {
                Dictionary<string, Action>.Enumerator E = this.Selectables.GetEnumerator();
                for (int i = 0; i < this.Selectables.Count; i++)
                {
                    E.MoveNext();
                    if (i == Choice)
                    {
                        E.Current.Value.Invoke();
                        return true;
                    }
                }
                return false;
            }
        }
        public class LayerManager
        {
            public Dictionary<string, Action> Commands = new Dictionary<string, Action>();
            public List<Layer> Layers = new List<Layer>();
            public int CurrentLayerIndex = 0;
            public bool ShouldActivate()
            {
                return this.Layers[this.CurrentLayerIndex] != null;
            }

            public void Activate()
            {
                Layer CurrentLayer = Layers[this.CurrentLayerIndex];
                try
                {
                    CurrentLayer.Draw();
                    if (CurrentLayer.Properties.QueryChoice)
                    {
                        string Input = Console.ReadLine();
                        try
                        {
                            int Choice = int.Parse(Input);
                            CurrentLayer.Input(Choice);
                        }
                        catch (FormatException)
                        {
                            try
                            {
                                Commands[Input].Invoke();
                            }
                            catch (KeyNotFoundException)
                            { Write("\nInvalid: "+ Input +"\n\n", ConsoleColor.Red); }
                        }                        
                    }
                }
                catch (LayerLockedException)
                {
                    // TODO: Handle LayerLocked Exception
                    throw;
                }
            }
        }

        /// <summary>
        /// Displays Text to the Console, using the specified Colors.
        /// </summary>
        /// <param name="Text">Text to display</param>
        /// <param name="ForeColor">Foreground Color</param>
        /// <param name="BackColor">Background Color</param>
        public static void Write(string Text = "\n", ConsoleColor ForeColor = ConsoleColor.White, ConsoleColor BackColor = ConsoleColor.Black)
        {
            /// Store current ForegroundColor, and set ForegroundColor to ForeColor.
            ConsoleColor OldForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ForeColor;

            /// Store current BackgroundColor, and set BackgroundColor to BackColor.
            ConsoleColor OldBackColor = Console.BackgroundColor;
            Console.BackgroundColor = BackColor;

            /// Print Text
            Print(Text);

            /// Set ForegroundColor to OldForeColor.
            Console.ForegroundColor = OldForeColor;

            /// Set BackgroundColor to OldBackColor.
            Console.BackgroundColor = OldBackColor;
        }

        public static string GetInput()
        {
            return Console.ReadLine();
        }

        public static void Wait(int Miliseconds)
        {
            Thread.Sleep(Miliseconds);
        }

        /// <summary>
        /// Displays Text to the Console.
        /// </summary>
        /// <param name="Text">Text to display</param>
        private static void Print(string Text)
        {
            Console.Write(Text);
        }
    }
}
