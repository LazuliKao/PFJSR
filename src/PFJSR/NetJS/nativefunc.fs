namespace PFJSR

open System.IO
open System.Threading
open System.Threading.Tasks
open API
open CSR
open type Newtonsoft.Json.JsonConvert

module NativeFunc=
    module Basic=
        let shares  = new System.Collections.Generic.Dictionary<string,obj>()
        type log_delegate = delegate of string -> unit
        let log=
            log_delegate(fun e-> Console.WriteLine(e))
        type fileReadAllText_delegate = delegate of string -> string
        let fileReadAllText=
            fileReadAllText_delegate (fun e->  
                try
                    e|>File.ReadAllText 
                with _ -> null
                )
        type fileWriteAllText_delegate = delegate of string*string -> bool
        let fileWriteAllText=
            fileWriteAllText_delegate(fun f c ->
                try
                    (f,c)|>File.WriteAllText
                    true
                with _ -> false
                )
        type fileWriteLine_delegate = delegate of string*string-> bool
        let fileWriteLine=
            fileWriteLine_delegate(fun f c ->
                try
                    (f,[c])|>File.AppendAllLines
                    true
                with _ -> false
                )
        type TimeNow_delegate = delegate of unit -> string
        let TimeNow=
            TimeNow_delegate(fun _ ->
                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                )
        type setShareData_delegate = delegate of string*obj-> unit
        let setShareData=
            setShareData_delegate(fun k o ->
                    if shares.ContainsKey(k) then shares.[k] <- o else shares.Add(k,o)
                )
        type getShareData_delegate = delegate of string -> obj
        let getShareData=
            getShareData_delegate(fun k ->
                    if shares.ContainsKey(k) then shares.[k] else Jint.Native.Undefined.Instance:>obj
                    )
        type removeShareData_delegate = delegate of string -> obj
        let removeShareData=
            removeShareData_delegate(fun k->
                    if shares.ContainsKey(k) then 
                        let o=shares.[k]
                        k|> shares.Remove|>ignore
                        o
                    else
                        Jint.Native.Undefined.Instance:>obj
                )
        type request_delegate = delegate of string*string*string*System.Action<bool,obj> -> unit
        let request=
            request_delegate(fun u m p f->
                    System.Threading.Tasks.Task.Run(fun ()-> 
                        (
                            try
                                Console.WriteLine("")
                                //new Thread(() =>
                                //{
                                //    string ret = null;
                                //    try
                                //    {
                                //        ret = localrequest(JSString(u), JSString(m), JSString(p));
                                //    }
                                //    catch { }
                                //    if (f != null)
                                //    {
                                //        try
                                //        {
                                //            f.Invoke(false, new string[] { ret });
                                //        }
                                //        catch
                                //        {
                                //            Console.WriteLine("[JS] File " + jsengines[f.Engine] + " Script err by call [request].");
                                //        }
                                //    }
                                //}).Start();
                            with _->()
                        )
                        )|>ignore
                )
        type setTimeout_delegate = delegate of System.Action*int -> unit
        let setTimeout=
            setTimeout_delegate(fun o ms->
                   if not (o|>isNull) then
                        Task.Run(fun _->
                        (
                            ms|>Thread.Sleep
                            o.Invoke()
                        ))|>ignore
                )
        type mkdir_delegate = delegate of string-> bool
        let mkdir=
            mkdir_delegate(fun dirname ->
                    let mutable dir :DirectoryInfo= null;
                    if not (dirname|>isNull) then
                        try
                            dir <- Directory.CreateDirectory(dirname)
                        with _->()
                    not (dir|>isNull)
                )
        type getWorkingPath_delegate = delegate of unit-> string
        let getWorkingPath=
            getWorkingPath_delegate(fun _ ->
                    System.AppDomain.CurrentDomain.BaseDirectory
                )
    module Core =
        type addBeforeActListener_delegate = delegate of string*System.Func<string,obj> -> int
        type addAfterActListener_delegate = delegate of string*System.Func<string,obj> -> int
        type removeBeforeActListener_delegate = delegate of string*int -> unit
        type removeAfterActListener_delegate = delegate of string*int -> unit
        type Instance(scriptName:string) =
            let BeforeActListeners =new System.Collections.Generic.Dictionary<int,MCCSAPI.EventCab>()
            let AfterActListeners =new System.Collections.Generic.Dictionary<int,MCCSAPI.EventCab>()
            member this.addBeforeActListener=
                addBeforeActListener_delegate(fun k f-> 
                (
                    let fullFunc=MCCSAPI.EventCab(fun e->
                        (
                            try
                                e|>BaseEvent.getFrom|>SerializeObject|>f.Invoke|>false.Equals|>not
                            with ex->
                            (
                                try
                                "在脚本\""+scriptName+"\"执行\""+(int e.``type``|>enum<EventType>).ToString()+"\"BeforeAct回调时遇到错误："+ex.ToString()|>Console.WriteLineErr
                                with _->()
                                true
                            )
                        ))
                    let funcHash=f.Method.GetHashCode()
                    BeforeActListeners.Add(funcHash,fullFunc)
                    (k,fullFunc)|>api.addBeforeActListener|>ignore
                    funcHash
                ))      
            member this.addAfterActListener=
                addAfterActListener_delegate(fun k f-> 
                (
                    let fullFunc=MCCSAPI.EventCab(fun e->
                        (
                            try
                                e|>BaseEvent.getFrom|>SerializeObject|>f.Invoke|>false.Equals|>not
                            with ex->
                            (
                                try
                                "在脚本\""+scriptName+"\"执行\""+(int e.``type``|>enum<EventType>).ToString()+"\"AfterAct回调时遇到错误："+ex.ToString()|>Console.WriteLineErr
                                with _->()
                                true
                            )
                        ))
                    let funcHash=f.Method.GetHashCode()
                    AfterActListeners.Add(funcHash,fullFunc)
                    (k,fullFunc)|>api.addAfterActListener|>ignore
                    funcHash
                ))  
            member private this.InvokeRemoveFailed=fun (a1:string ,a2:string)->
                (
                     "在脚本\""+scriptName+"\"执行\""+a1+"\"无效，参数2的值仅可以通过\""+a2+"\"结果获得"|>Console.WriteLineErr
                )
            member this.removeBeforeActListener=
                removeBeforeActListener_delegate(fun k fhash-> 
                (   
                    try
                        if BeforeActListeners.ContainsKey(fhash) then
                            let getFunc=BeforeActListeners.[fhash]
                            (k, getFunc )|>api.removeBeforeActListener|>ignore
                            BeforeActListeners.Remove(fhash)|>ignore
                        else
                            this.InvokeRemoveFailed(nameof(this.removeBeforeActListener),nameof(this.addBeforeActListener))
                    with _-> this.InvokeRemoveFailed(nameof(this.removeBeforeActListener),nameof(this.addBeforeActListener))
              ))   
            member this.removeAfterActListener=
                removeAfterActListener_delegate(fun k fhash-> 
                (   
                    try
                        if AfterActListeners.ContainsKey(fhash) then
                            let getFunc=AfterActListeners.[fhash]
                            (k, getFunc )|>api.removeAfterActListener|>ignore
                            AfterActListeners.Remove(fhash)|>ignore
                        else
                            this.InvokeRemoveFailed(nameof(this.removeAfterActListener),nameof(this.addAfterActListener))
                    with _->this.InvokeRemoveFailed(nameof(this.removeAfterActListener),nameof(this.addAfterActListener))
                ))