namespace PFJSR
open CSR
open Newtonsoft.Json.Linq
open System.Text.RegularExpressions
open System.Collections.Generic
open System.Threading.Tasks

module API=
    let mutable api:MCCSAPI = null
    let mutable csrAssemblyList:list<System.Reflection.Assembly>=[]
    let PFJsrExceptionStart="PFJsrException:"
    type PFJsrException(ex)=
        inherit System.Exception(PFJsrExceptionStart+ex)
    type ScriptType=
        | JSR
        | VJS
    type ScriptItemModel(t: ScriptType,n: string, p: string, c: string) =
        member _this.Type :ScriptType=t
        member _this.Name :string=n
        member _this.Path :string=p
        member _this.Content :string=c
    type VanillaScriptException(info:JObject) =
        inherit System.Exception()
        let stackraw=info.Value<string>("stack")
        let stackmatch=Regex(@"at(\s.*?)\s\((.*?):(\d+):(\d+)\)").Matches(stackraw)
        member _this.stack=[for x in stackmatch -> (x.Groups.[0].Value,x.Groups.[1].Value.Trim(),x.Groups.[2].Value.Trim(),int x.Groups.[3].Value,int x.Groups.[4].Value)]
        member _this.scriptName=info.Value<string>("script")
        member _this.exname=info.Value<string>("exname")
        member _this.message=info.Value<string>("message")
        //override _this.ToString()=""
    let mutable LoadedScripts:list<ScriptItemModel>=[]
        //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(e,System.Windows.Threading.DispatcherPriority.ApplicationIdle)