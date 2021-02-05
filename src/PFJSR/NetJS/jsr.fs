namespace PFJSR
open Jint.Native
open Jint
module JSR=
    type JSRunner(_engine:Engine,_js:string,_core:NativeFunc.Core.Instance) = 
        member _this.eng with get() =_engine  
        member _this.js with get() =_js 
        member _this.core with get() =_core  
    let mutable RunnerList:list<JSRunner>=[]
    let CreateEngine(eng:Engine,js:string,core:NativeFunc.Core.Instance):Engine=
        RunnerList<-new JSRunner(eng,js,core) :: RunnerList
        (new JsString(nameof NativeFunc.Basic.log),
                                           NativeFunc.Basic.log)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.fileReadAllText),
                                           NativeFunc.Basic.fileReadAllText)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.fileWriteAllText),
                                           NativeFunc.Basic.fileWriteAllText)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.fileWriteLine),
                                           NativeFunc.Basic.fileWriteLine)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.TimeNow),
                                           NativeFunc.Basic.TimeNow)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.setShareData),
                                           NativeFunc.Basic.setShareData)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.getShareData),
                                           NativeFunc.Basic.getShareData)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.removeShareData),
                                           NativeFunc.Basic.removeShareData)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.mkdir),
                                           NativeFunc.Basic.mkdir)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.getWorkingPath),
                                           NativeFunc.Basic.getWorkingPath)|>eng.SetValue|>ignore
        //核心玩法
        for item in core.GetType().GetProperties() do
             (new JsString(item.Name),
                  item.GetValue(core))|>eng.SetValue|>ignore
        //for item in core.GetType().GetMethods() do
        //     Console.WriteLine(item.Name)
        js|>eng.Execute