namespace PFJSR
open Colorful
open System.Drawing
module Console=
    let WriteLine(content :string)=
        Colorful.Console.WriteLineFormatted("{0}{3}{1}{0}{2}{1}{4}", Console.ForegroundColor,
            new Formatter("[", Color.LightSteelBlue),
            new Formatter("]", Color.LightSteelBlue),
            new Formatter("PFJSR", Color.LightGreen),
            new Formatter("INFO", Color.Gray),
            new Formatter(content, Color.LightGoldenrodYellow)
        )
        Console.ForegroundColor<-Color.White
    let WriteLineErr(content :string,ex:exn)=
        Colorful.Console.WriteLineFormatted("{0}{2} {3}{1}{4}\n\t{5}\n{6}", Console.ForegroundColor,
            new Formatter("[", Color.LightGoldenrodYellow),
            new Formatter("]", Color.LightGoldenrodYellow),
            new Formatter("PFJSR", Color.Cyan),
            new Formatter("ERROR", Color.MediumVioletRed),
            new Formatter(content, Color.OrangeRed),
            new Formatter(ex.Message, Color.Orange),
            new Formatter(ex.StackTrace, Color.Gray)
        )
        Console.ForegroundColor<-Color.White
    let log(content:string)=
        (content,Color.LightYellow)|>Colorful.Console.WriteLine
