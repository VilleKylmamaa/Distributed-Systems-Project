import React from "react"

function MessageLine({sender, text}) {

  return (
    <>
      {sender 
        ? 
          <><span className="sender">{sender}:</span> {text}</>
        : 
          <span className="has-joined-chat-message">{text}</span>
      }
    </>
  )
}

export default MessageLine
