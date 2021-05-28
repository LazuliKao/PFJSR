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
        (js,Esprima.ParserOptions(source=js))|>eng.Execute|>ignore 
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
        for x in System.AppDomain.CurrentDomain.GetAssemblies() do
            #if DEBUG
            Console.WriteLine(x.GetName())
            #endif
            !~x
        for x in API.csrAssemblyList do 
            !~x
            #if DEBUG
            Console.WriteLine("加入程序集"+x.FullName)
            #endif
        let eng=Jint.Engine(options)
        let core=NativeFunc.Core.Model(scriptName,eng)
        RunnerList<-new JSRunner(eng,core) :: RunnerList
        //注入各种方法
        let ( <+< ) (name:string)(value:obj) = //设置公共对象（方法、常量）
            //(JsString(name),value)|>eng.SetValue|>ignore
            eng.Global.Set(JsString(name),JsValue.FromObject(eng,value))|>ignore
        //基础功能
        for item in NativeFunc.Basic.Instance.GetType().GetProperties() do
              item.Name <+< (item.GetValue(NativeFunc.Basic.Instance))
        //核心玩法
        for item in core.GetType().GetProperties() do
             item.Name <+< item.GetValue(core)
        //功能同步于：https://github.com/cngege/BDSJSR2/blob/d56f8a1ebdd73c70bdc0b392c0fd233547135dce/BDSJSR2/Program.cs#L1348
        //api
        "api" <+< API.api
        //lib
        let libObj=Jint.Native.Object.ObjectInstance(eng)//创建JsObject
        (JsString("System"),"System"|>JsString|>eng.Global.Get)
            |>libObj.Set|>ignore//设置JsObject对象的System属性
        "lib" <+< libObj//添加全局常量
        //ram
        "ram" <+< Jint.Runtime.Interop.TypeReference.CreateTypeReference(eng, typeof<PFJSRBDSAPI.Ram>)
        eng