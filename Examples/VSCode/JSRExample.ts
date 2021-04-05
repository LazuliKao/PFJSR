/// <reference types="jsr"/>
declare class onChatEvent {
    /** 发言人名字（可能为服务器或命令方块发言） */
    declare playername: string
    /** 接收者名字 */
    declare target: string
    /** 接收到的信息 */
    declare msg: string
    /** 聊天类型 */
    declare chatstyle: string
}
function ParseEvent<T>(e: string): T {
    return JSON.parse(e);
}
addAfterActListener(EventKey.onChat, (eventArgStr) => {
    const e = ParseEvent<onChatEvent>(eventArgStr)
    log(e.chatstyle)



})
