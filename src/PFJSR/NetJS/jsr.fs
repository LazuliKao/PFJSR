namespace PFJSR
open Jint.Native
open Jint

module JSR=
    let CreateEngine(js:string,core:NativeFunc.Core.Instance):Engine=
        let eng=new Engine()
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
        (new JsString(nameof NativeFunc.Basic.setTimeout),
                                           NativeFunc.Basic.setTimeout)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.request),
                                           NativeFunc.Basic.request)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.mkdir),
                                           NativeFunc.Basic.mkdir)|>eng.SetValue|>ignore
        (new JsString(nameof NativeFunc.Basic.getWorkingPath),
                                           NativeFunc.Basic.getWorkingPath)|>eng.SetValue|>ignore

        //核心玩法
        (new JsString(nameof core.addBeforeActListener),
                                           core.addBeforeActListener)|>eng.SetValue|>ignore
        (new JsString(nameof core.addAfterActListener),
                                           core.addAfterActListener)|>eng.SetValue|>ignore
        (new JsString(nameof core.removeBeforeActListener),
                                           core.removeBeforeActListener)|>eng.SetValue|>ignore
        (new JsString(nameof core.removeAfterActListener),
                                           core.removeAfterActListener)|>eng.SetValue|>ignore

        js|>eng.Execute