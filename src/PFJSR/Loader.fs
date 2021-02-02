namespace PFJSR
open System.Collections
open System.IO

module Loader=
    type ScriptItemModel(n: string, p: string) =
        member this.Name :string=n
        member this.Path :string=p
    let mutable LoadedScripts:list<ScriptItemModel>=[] 
    let LoadJSRScript(filePath:string)=
        try
            let scriptName=filePath|>Path.GetFileNameWithoutExtension
            let engine=
                (
                    (filePath|>File.ReadAllText),
                    new PFJSR.NativeFunc.Core.Instance(scriptName)
                )|>JSR.CreateEngine 
            LoadedScripts<-new ScriptItemModel (scriptName, filePath)::LoadedScripts
            scriptName+"加载完成！"|>Console.WriteLine
        with ex->ex.ToString()|>Console.WriteLine
