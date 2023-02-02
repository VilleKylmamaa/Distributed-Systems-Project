import React from 'react'
import uniqueId from 'lodash/uniqueId'
import { useState } from 'react'
import { randomMessages  } from '../RandomAssets/randomMessages.js'
import MessageLine from './MessageLine.js'
import Message from '../Entities/Message.js'
import { HubConnection } from '@microsoft/signalr'

type Props = {
  connection: HubConnection | null,
  messages: Array<Message>, 
  username: string,
  roomName: string,
}

function ChatBox({connection, messages, username, roomName}: Props) {
  
  const [message, setMessage] = useState("")

  const sendMessage = async (message) => {
    try {
      await connection?.invoke(
        "MessageToRoom",
        {text: message, username, roomName}
      )
      setMessage("")

    } catch (error) {
      console.error(error)
    }
  }

  const spamMessage = async () => {
    try {
      const randomMessage = randomMessages[Math.floor(Math.random() * randomMessages.length)]
      await connection?.invoke(
        "MessageToRoom", 
        {text: randomMessage, username, roomName}
      )

    } catch (error) {
      console.error(error)
    }
  }

  return (
    <div>
      <div className="card chat-box">
        {messages.map(message => (
          <p 
            key={uniqueId("msg")}
            style={{color: message.color}}
          >
            <MessageLine
              sender={message.sender}
              text={message.text}
            />
          </p>
        ))}
      </div>

      <input 
        onChange={event => {setMessage(event.target.value)}}
        value={message}
      />
      <button 
        onClick={() => sendMessage(message)}
        disabled={!message}
        className="send-button"
      >
        Send
      </button>
      
      <button 
        onClick={spamMessage}
        className="spam-button"
      >
        Spam :)
      </button>
    </div>
  )
}

export default ChatBox
