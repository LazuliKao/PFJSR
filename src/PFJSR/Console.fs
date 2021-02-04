namespace PFJSR
open Colorful
open System.Drawing
module Console=
    let WriteLine(content :string)=
        Colorful.Console.WriteLineFormatted("{0}{2}{1}{3}", Color.LightGoldenrodYellow,
            new Formatter("[", Color.LightSteelBlue),
            new Formatter("]", Color.LightSteelBlue),
            new Formatter("PFJSR", Color.LightGreen),
            new Formatter(content, Color.LightGoldenrodYellow)
        )
    let WriteLineErr(content :string,ex:exn)=
        Colorful.Console.WriteLineFormatted("{0}{2} {3}{1}{4}\n\t{5}\n{6}", Color.LightGoldenrodYellow,
            new Formatter("[", Color.LightGoldenrodYellow),
            new Formatter("]", Color.LightGoldenrodYellow),
            new Formatter("PFJSR", Color.Cyan),
            new Formatter("ERROR", Color.MediumVioletRed),
            new Formatter(content, Color.OrangeRed),
            new Formatter(ex.Message, Color.Orange),
            new Formatter(ex.StackTrace, Color.Gray)
        )