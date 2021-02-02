namespace PFJSR
module Console=
    let WriteLine(content :string)=
        ("[PFJSR]",System.Drawing.Color.Yellow)|>Colorful.Console.Write
        (content,System.Drawing.Color.AliceBlue)|>Colorful.Console.WriteLine
    let WriteLineErr(content :string)=
        printfn "[ERROR]%s" content
