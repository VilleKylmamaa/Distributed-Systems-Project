import React from "react"

type Props = {
  sender: string,
  messageText: string,
}

function MessageLine({sender, messageText} : Props) {

  return (
    <>
      {sender 
        ? 
          <><span className="sender">{sender}:</span> {messageText}</>
        : 
          <span className="has-joined-chat-message">{messageText}</span>
      }
    </>
  )
}

export default MessageLine
