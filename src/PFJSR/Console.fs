namespace PFJSR
open System.Drawing
open System
open Serilog
open Serilog.Sinks.SystemConsole.Themes

module Console=
    let PluginName="PFJSR"
    let console_already_setup=Log.Logger.GetHashCode()<>46104728
    let SetupConsole()=
        try
            let mutable model=null
            if Version.Parse(API.api.VERSION) >new Version(1, 16, 100, 0) then
                model <- "[{Level:u4}]{Message:lj}{NewLine}{Exception}"
            else
                model <- "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u4}]{Message:lj}{NewLine}{Exception}"
            Log.Logger<-(new LoggerConfiguration()).WriteTo.Console(
                theme=new SystemConsoleTheme(
                    new System.Collections.Generic.Dictionary<ConsoleThemeStyle,SystemConsoleThemeStyle>(dict[
                        ConsoleThemeStyle.Text,new SystemConsoleThemeStyle(Foreground=ConsoleColor.White)
                        ConsoleThemeStyle.SecondaryText,new SystemConsoleThemeStyle(Foreground=ConsoleColor.DarkYellow)
                        ConsoleThemeStyle.TertiaryText,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Green)
                        ConsoleThemeStyle.Invalid,new SystemConsoleThemeStyle(Foreground=ConsoleColor.DarkCyan)
                        ConsoleThemeStyle.Null,new SystemConsoleThemeStyle(Foreground=ConsoleColor.White)
                        ConsoleThemeStyle.Name,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Magenta)
                        ConsoleThemeStyle.String,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Yellow)
                        ConsoleThemeStyle.Number,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Blue)
                        ConsoleThemeStyle.Boolean,new SystemConsoleThemeStyle(Foreground=ConsoleColor.White)
                        ConsoleThemeStyle.Scalar,new SystemConsoleThemeStyle(Foreground=ConsoleColor.DarkGray)
                        ConsoleThemeStyle.LevelVerbose,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Gray,Background=ConsoleColor.DarkGray)
                        ConsoleThemeStyle.LevelDebug,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Cyan,Background=ConsoleColor.DarkMagenta)
                        ConsoleThemeStyle.LevelInformation,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Cyan)
                        ConsoleThemeStyle.LevelWarning,new SystemConsoleThemeStyle(Foreground=ConsoleColor.Red,Background=ConsoleColor.DarkBlue)
                        ConsoleThemeStyle.LevelError,new SystemConsoleThemeStyle(Foreground=ConsoleColor.White,Background=ConsoleColor.DarkRed)
                        ConsoleThemeStyle.LevelFatal,new SystemConsoleThemeStyle(Foreground=ConsoleColor.White,Background=ConsoleColor.DarkRed)
                    ])
            ),outputTemplate=model
            )
            #if DEBUG
                .MinimumLevel.Debug()
            #endif
                .CreateLogger()
            Log.Information("控制台输出已开启")
        with ex->System.Console.WriteLine($"[{PluginName}]控制台输出开启失败：{ex}")
    let Setup()=
        if console_already_setup|>not then
            SetupConsole()
    let WriteLineT(subtype:obj)(content :obj)=
        Log.Information("[{0}]|{1}| " + content.ToString(), PluginName, subtype)
    let WriteLine(content :obj)=
        Log.Information("[{0}]" + content.ToString(), PluginName)
        //Colorful.Console.WriteLineFormatted("{0}{3}{1}{0}{2}{1}{4}\t", Console.ForegroundColor,
        //   new Formatter("[", Color.LightSteelBlue),
        //   new Formatter("]", Color.LightSteelBlue),
        //   new Formatter("PFJSR", Color.LightGreen),
        //   new Formatter("INFO", Color.Gray),
        //   new Formatter(content, Color.LightGoldenrodYellow)
        //)
        //Console.ForegroundColor<-Color.White
    let WriteLineWarn(content:obj,tip:string)=
        Log.Warning("[{0}]"+content.ToString()+" --> "+tip.ToString(), PluginName)
    let WriteLineErr(content:obj,ex:exn)=
        if typeof<API.PFJsrException> = ex.GetType() then
            Log.Error("[{0}]"+content.ToString()+"\n"+ex.Message, PluginName)
        else
            Log.Error(ex,"[{0}]"+content.ToString(), PluginName)
        //Colorful.Console.WriteLineFormatted("{0}{2} {3}{1}{4}\n\t{5}\n{6}\t", Console.ForegroundColor,
        //   new Formatter("[", Color.LightGoldenrodYellow),
        //   new Formatter("]", Color.LightGoldenrodYellow),
        //   new Formatter("PFJSR", Color.Cyan),
        //   new Formatter("ERROR", Color.MediumVioletRed),
        //   new Formatter(content, Color.OrangeRed),
        //   new Formatter(ex.Message, Color.Orange),
        //   new Formatter(ex.StackTrace, Color.Gray)
        //)
        //Console.ForegroundColor<-Color.White
    let log(content:string)=
        printfn "%s" content
        //(content,Color.LightYellow)|>Colorful.Console.WriteLine
