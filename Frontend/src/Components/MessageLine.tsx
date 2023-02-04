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
          <span>{messageText}</span>
      }
    </>
  )
}

export default MessageLine
