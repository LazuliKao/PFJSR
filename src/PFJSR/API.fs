namespace PFJSR
open CSR
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
    let mutable LoadedScripts:list<ScriptItemModel>=[] 

