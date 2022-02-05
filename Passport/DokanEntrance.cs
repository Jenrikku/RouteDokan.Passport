using System;
using System.IO;
using System.Text;

using static RouteDokan.Passport.DokanGuidance;
using static RouteDokan.Passport.DokanConfiguration;

namespace RouteDokan.Passport {
    /// <summary>
    /// <see cref="DokanEntrance"/> is the class where the program begins (both console and GUI).
    /// </summary>
    class DokanEntrance {
        static void Main(string[] args) {
            // Enables Shift-JIS encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.GetEncoding("Shift-JIS");

            Console.WriteLine('\n' + MessageHeader);

            // No GUI
            if(args.Length > 0)
                switch(args[0].ToLowerInvariant()) {
                    case "-s":
                    case "--set":
                        if(args.Length != 3) {
                            Console.WriteLine(SetArgument);
                            break;
                        }

                        if(!AvailableKeys.Contains(args[1])) {
                            Console.WriteLine(
                                "The parameter \"" + args[1] + "\" doesn't exist." +
                                ParametersList);
                            break;
                        }

                        SetValue(args[1], args[2]);
                        Console.WriteLine("Parameter \"" + args[1] + "\" saved.");

                        break;
                    case "-p":
                    case "--port":
                        DokanProcessor.CurrentProgressChanged += 
                            (ProgressInfo info, string header) => {
                                if(info is null) Console.WriteLine(header);
                                else Console.WriteLine(header + info.FullName);
                            };
                        string inputDir;
                        string outputDir;

                        switch(args.Length) {
                            case 0:
                            case 1:
                                inputDir = Environment.CurrentDirectory;
                                outputDir = Path.Join(Environment.CurrentDirectory, "out");
                                break;
                            case 2:
                                inputDir = args[1];
                                outputDir = Path.Join(args[1], "out");
                                break;
                            default:
                                inputDir = args[1];
                                outputDir = args[2];
                                break;
                        }

                        bool? platform = DokanProcessor.IdentificatePlatform(inputDir);
                        if(!platform.HasValue) {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The folder specified can't be processed: at least one .szs must be inside the desidered folder.");
                        } else DokanProcessor.StartWork(inputDir, outputDir, platform.Value);

                        break;
                    case "/help":
                    case "--help":
                        if(args.Length <= 1) {
                            Console.WriteLine("List of available arguments:" + DefaultHelp);
                            break;
                        }

                        switch(args[1]) {
                            case "set":
                            case "-s":
                            case "--set":
                                Console.WriteLine(SetArgument);
                                break;
                            default:
                                Console.WriteLine("Invalid argument." + DefaultHelp);
                                break;
                        }

                        break;
                    default:
                        goto case "--help";
                }
            // GUI
            else
                throw new NotImplementedException();
        }
    }
}
