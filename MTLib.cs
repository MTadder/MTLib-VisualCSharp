using System;
using System.Speech.Synthesis;

namespace MTLib {
    public static class Display {
        public static void Write(string Text, ConsoleColor Color = ConsoleColor.White) {
            ConsoleColor Old = Console.ForegroundColor;
            Console.ForegroundColor=Color;

            Console.Write(Text);

            Console.ForegroundColor=Old;
        }
    }

    public static class Info {
        public static readonly bool Debug = true;
        public static readonly string Author = "MTadder / Ayden G.W.";
        public static readonly string Version = "nonal";
        public static readonly ConsoleColor VersionColorCode = ConsoleColor.Magenta;
    }

    public static class Speech {
        public static void Say(string Text) {
            SpeechSynthesizer Synth = new SpeechSynthesizer();
            Synth.SetOutputToDefaultAudioDevice();
            Synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            Synth.Speak(Text);
        }
    }
}
