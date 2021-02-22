namespace PFJSR
open CSR
module API=
    let mutable api:MCCSAPI = null
    let mutable csrAssemblyList:list<System.Reflection.Assembly>=[]
