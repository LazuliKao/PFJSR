namespace PFJSR
module Console=
    let WriteLine(content :string)=
        ("[PFJSR]",System.Drawing.Color.Yellow)|>Colorful.Console.Write
        (content,System.Drawing.Color.AliceBlue)|>Colorful.Console.WriteLine
    let WriteLineErr(content :string)=
        ("[PFJSR]",System.Drawing.Color.Yellow)|>Colorful.Console.Write
        ("[ERROR]",System.Drawing.Color.Red)|>Colorful.Console.Write
        printfn "%s" content
    let log(content:string)=
        (System.Console.WriteLine(content))
