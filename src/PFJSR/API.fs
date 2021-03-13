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
    let mutable lock=false
    let rec WaitForLock()=
        while lock do
            System.Threading.Thread.Sleep(3)
        if lock then WaitForLock()
        if lock then WaitForLock()
    let RunFun(e:unit->unit)=
        WaitForLock()
        WaitForLock()
        lock<-true
        e()
        lock<-false
        //System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(e,System.Windows.Threading.DispatcherPriority.ApplicationIdle)