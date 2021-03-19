namespace PFJSR
open Jint.Native
open Jint
open System.IO

module JSR=
    type JSRunner(_engine:Engine,_core:NativeFunc.Core.Model) = 
        member _this.eng with get() =_engine  
        //member _this.js with get() =_js 
        member _this.core with get() =_core  
    let mutable RunnerList:list<JSRunner>=[]
    let InitScript(eng:Engine,js:string)=
        js|>eng.Execute|>ignore 
    let CreateEngine(scriptName):Engine=
        let options=Jint.Options().AllowClr() 
        #if DEBUG
        options.DebugMode()|>ignore
        #endif
        (
        #if DEBUG
        fun ex->
        #else
        fun _->
        #endif
        #if DEBUG
        Console.WriteLine (ex.ToString())
        #endif
        true
        )|>options.CatchClrExceptions|>ignore
        let ( !~ ) (a: System.Reflection.Assembly) = options.AllowClr(a)|>ignore
        let ( !+ ) (t: System.Type) = !~ t.Assembly
        !+typeof<Jint.Engine>
        !+typeof<Reflection.FSharpType>
        !+typeof<Newtonsoft.Json.JsonConvert>
        for x in API.csrAssemblyList do 
            !~x
            #if DEBUG
            Console.WriteLine("加入程序集"+x.FullName)
            #endif
        let eng=Jint.Engine(options)
        let core=NativeFunc.Core.Model(scriptName,eng)
        //注入各种方法
        RunnerList<-new JSRunner(eng,core) :: RunnerList
        //基础功能
        for item in NativeFunc.Basic.Instance.GetType().GetProperties() do
             (new JsString(item.Name),
                  item.GetValue(NativeFunc.Basic.Instance))|>eng.SetValue|>ignore
        //核心玩法
        for item in core.GetType().GetProperties() do
             (new JsString(item.Name),
                  item.GetValue(core))|>eng.SetValue|>ignore
        eng
         