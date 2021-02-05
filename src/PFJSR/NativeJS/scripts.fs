namespace PFJSR
open CSR
module NativeScripts=
    let SetupEngine=()
    let AppendScript(scriptName:string,scriptContent:string)=
        (scriptContent,fun result->
            if result then
               $"\"{scriptName}\"载入成功！(返回值{result})"|>Console.WriteLine
            else
               $"\"{scriptName}\"载入失败！(返回值{result})"|>Console.WriteLine
        )|>API.api.JSErunScript
    let ApplyAcripts=()

