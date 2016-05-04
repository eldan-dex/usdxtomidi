using System;
using System.Collections.Generic;
using System.IO;
using CannedBytes.Midi.IO;
using CannedBytes.Midi.Message;

namespace MLKtoMIDI {
    static class USDXtoMIDI {
        [STAThread]
        static void Main() {
            //Config
            int noteLengthMultipler = 51;
            int midi_C4 = 60;


            List<MidiFileEvent> events = new List<MidiFileEvent>();
            MidiMessageFactory factory = new MidiMessageFactory();
            MidiFileEvent evnt;
            string sourceFile;
            string destinationFile;
            try {
                string[] args = Environment.GetCommandLineArgs();

                if (args.Length < 3) {
                    Console.WriteLine("USDX2MIDI tool by Dex\nUsage: " + System.Diagnostics.Process.GetCurrentProcess().ProcessName + " notesFile.txt outputFile.mid [-mul X] [-C4 X]");
                    return;
                }
                if (args.Length >= 5) {
					if(args[3].ToLower() == "-mul")
						noteLengthMultipler = Convert.ToInt32(args[4]);

					if(args[3].ToLower() == "-c4")
						midi_C4 = Convert.ToInt32(args[4]);
                }

                Console.WriteLine("[INFO]: C4=" + midi_C4 + ", noteLengthMultiplier=" + noteLengthMultipler);

                sourceFile = args[1];
                destinationFile = args[2];

                if (!File.Exists(sourceFile)) {
                    Console.WriteLine("[ERROR]: " + sourceFile + " does not exist!");
                }

                string[] lines = File.ReadAllLines(sourceFile);
                foreach (string line in lines) {
                    if (line.StartsWith("#") || line.StartsWith("-")) //Ignore comments and silence
                        continue;

                    else if (line.StartsWith("E")) //End when end is found
                        break;

                    else if (line.StartsWith(":") || line.StartsWith("*")/* || line.StartsWith("F")*/) { //Normal note, golden note, (spoken note?)
                        string[] values = line.Contains(" ") ? line.Split(' ') : null; //Skip if no space is found (incorrect data)
                        if (values == null || values.Length < 4)
                            continue;

                        long noteTime = Convert.ToInt64(values[1]) * noteLengthMultipler;
                        var val = midi_C4 + Convert.ToInt32(values[3]);
                        byte noteValue = Convert.ToByte(val);

                        evnt = new MidiFileEvent();
                        evnt.AbsoluteTime = noteTime;
                        evnt.Message = factory.CreateChannelMessage(MidiChannelCommand.NoteOn, 1, noteValue, 0x40);
                        events.Add(evnt);
                        evnt = new MidiFileEvent();
                        evnt.AbsoluteTime = noteTime + Convert.ToInt64(values[2]) * noteLengthMultipler;
                        evnt.Message = factory.CreateChannelMessage(MidiChannelCommand.NoteOff, 1, noteValue, 0x00);
                    }
                }
                var serializer = new MidiFileSerializer(destinationFile); //Write results
                serializer.Serialize(events);
            }
            catch (Exception ex) {
                Console.WriteLine("[ERROR]: Something went wrong, sorry :/\n " + ex.Message);
                return;
            }
        }
    }
}
