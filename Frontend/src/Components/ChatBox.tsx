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
  
  const [messageText, setMessageText] = useState<string>("")

  const sendMessage = async (messageText : string) => {
    try {
      await connection?.invoke(
        "MessageToRoom",
        {text: messageText, username, roomName}
      )
      setMessageText("")

    } catch (error) {
      console.error(error)
    }
  }

  const spamMessage = async () : Promise<void> => {
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
              messageText={message.text}
            />
          </p>
        ))}
      </div>
      
      <div className="input-section">
        <input 
          onChange={event => {setMessageText(event.target.value)}}
          value={messageText}
          className="message-input"
        />
        <button 
          onClick={() => sendMessage(messageText)}
          disabled={!messageText}
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
    </div>
  )
}

export default ChatBox
