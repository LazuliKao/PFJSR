namespace PFJSR
open Jint.Native
open Jint
module JSR=
    type JSRunner(_engine:Engine,_js:string,_core:NativeFunc.Core.Model) = 
        member _this.eng with get() =_engine  
        member _this.js with get() =_js 
        member _this.core with get() =_core  
    let mutable RunnerList:list<JSRunner>=[]
    let CreateEngine(eng:Engine,js:string,core:NativeFunc.Core.Model):Engine=
        RunnerList<-new JSRunner(eng,js,core) :: RunnerList
        //基础功能
        for item in NativeFunc.Basic.Instance.GetType().GetProperties() do
             (new JsString(item.Name),
                  item.GetValue(NativeFunc.Basic.Instance))|>eng.SetValue|>ignore
        //核心玩法
        for item in core.GetType().GetProperties() do
             (new JsString(item.Name),
                  item.GetValue(core))|>eng.SetValue|>ignore
        //for item in core.GetType().GetMethods() do
        //     Console.WriteLine(item.Name)
        js|>eng.Execute