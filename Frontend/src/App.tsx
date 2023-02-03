import { useState } from 'react'
import './App.css'
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr'
import { randomColors } from './RandomAssets/randomColors.js'
import ChatBox from './Components/ChatBox'
import Message from './Entities/Message'
import React from 'react'

const SIGNALR_URL = "http://localhost:5000/SignalrHub"
// const SIGNALR_URL = "http://localhost:5001/SignalrHub"

function App() {
  const [ connection, setConnection ] = useState<HubConnection | null>(null)
  const [ isRoomJoined, setRoomJoined ] = useState<boolean>(false)
  const [ messages, setMessages ] = useState<Array<Message>>([])
  const [ username, setUsername ] = useState<string>("")
  const [ roomName, setRoomName ] = useState<string>("")
  
  const joinRoom = async () : Promise<void> => {
    try {
      const hubConnection = new HubConnectionBuilder()
        .withUrl(SIGNALR_URL)
        .configureLogging(LogLevel.Information)
        .build()
      
      hubConnection.on("ReceiveMessage", (message) => {
        console.log("Message received:", message)
        const randomColor = randomColors[Math.floor(Math.random() * 5)]
        const newMessage = { text: message.text, color: randomColor, sender: message.username }
        setMessages(messages => [newMessage, ...messages])
      })

      hubConnection.onclose(() => {
        setConnection(null)
        setRoomJoined(false)
        setMessages([])
      })

      await hubConnection.start()
      await hubConnection.invoke("JoinRoom", {username, roomName})

      setConnection(hubConnection)
      setRoomJoined(true)

    } catch (error) {
      console.error(error)
    }
  }
  
  const leaveChat = async () : Promise<void> => {
    try {
      await connection?.invoke("LeaveRoom", {username, roomName})
      await connection?.stop()
    } catch (error) {
      console.error(error)
    }
  }

  return (
    <div className="App">
      <h1 className='title'>Distr Chat</h1>

      {!isRoomJoined && (
        <div className="card">
          <>
            <input 
              onChange={event => {setUsername(event.target.value)}}
              value={username}
              placeholder="Username"
            />
            <input 
              onChange={event => {setRoomName(event.target.value)}}
              value={roomName}
              placeholder="Room name"
              className="room-input"
            />
            <button 
              onClick={joinRoom}
              disabled={!roomName}
            >
              Join room
            </button>
          </>
        </div>
      )}

      {isRoomJoined && (
        <>
          <h2 className="room-name">Room: {roomName}</h2>
          <ChatBox
            connection={connection}
            messages={messages}
            username={username}
            roomName={roomName}
          />
          <button 
            onClick={leaveChat}
            className="leave-room-button"
          >
            Leave room
          </button>
        </>
      )}

    </div>
  )
}

export default App
