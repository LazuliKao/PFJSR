namespace PFJSR
open PFJSR
open System.Collections
open System.IO
open System.Threading.Tasks
open System.Threading
module Loader=
    let LoadJSRScript(filePath:string)=
        let scriptName=filePath|>Path.GetFileNameWithoutExtension
        try
            let options=new Jint.Options()
            options.AllowClr()|>ignore
            options.AllowClr(typeof<FSharp.Reflection.FSharpType>.Assembly)|>ignore
            //options.AllowClr(typeof<Colorful.Console>.Assembly)|>ignore
            options.AllowClr(typeof<Jint.Engine>.Assembly)|>ignore
            options.AllowClr(typeof<Newtonsoft.Json.JsonConvert>.Assembly)|>ignore
            for x in API.csrAssemblyList do
                options.AllowClr(x)|>ignore
                //#if DEBUG
                //Console.WriteLine("加入程序集"+x.FullName)
                //#endif
            let jsContent=filePath|>File.ReadAllText
            API.LoadedScripts<-new API.ScriptItemModel(API.ScriptType.JSR,scriptName, filePath,jsContent)::API.LoadedScripts
            let engine=new Jint.Engine(options)
            (
                engine,
                jsContent,
                new NativeFunc.Core.Model(scriptName,engine)
            )|>JSR.CreateEngine|>ignore
            scriptName+"加载完成！"|>Console.WriteLine
        with ex->Console.WriteLineErrEx
                            $"\"{scriptName}\"加载失败！"
                            ex scriptName
    let LoadVanillaScript(filePath:string)=
        let scriptName=filePath|>Path.GetFileNameWithoutExtension
        try
            let scriptContent=filePath|>File.ReadAllText
            (scriptName,scriptContent)|>VanillaScripts.AppendScript
            API.LoadedScripts<-new API.ScriptItemModel(API.ScriptType.VJS,scriptName,filePath,scriptContent)::API.LoadedScripts
        with ex->($"\"{scriptName}\"导入失败！",ex)|>Console.WriteLineErr