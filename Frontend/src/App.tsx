import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import axios from 'axios';
import { useState } from 'react';
import './App.css';
import ChatBox from './Components/ChatBox';
import Message from './Entities/Message';

const loadBalancerUrls = [
  "http://localhost:7000/LoadBalancer",
  "http://localhost:7001/LoadBalancer",
  "http://localhost:7002/LoadBalancer"
]

/* const appServerUrls = [
  "http://localhost:5000/SignalrHub",
  "http://localhost:5001/SignalrHub",
  "http://localhost:5002/SignalrHub"
] */

function App() {
  const [ connection, setConnection ] = useState<HubConnection | null>(null);
  const [ isRoomJoined, setRoomJoined ] = useState<boolean>(false);
  const [ messages, setMessages ] = useState<Array<Message>>([]);
  const [ username, setUsername ] = useState<string>("");
  const [ roomName, setRoomName ] = useState<string>("");
  const [ isConnecting, setIsConnecting ] = useState<boolean>(false);

  const fetchAppServerUrlFromLoadBalancer = async(urlIndex: number) : Promise<string> => {
    try {
      console.log("Fetching application server url from load balancer:", loadBalancerUrls[urlIndex])
      const loadBalancerResponse = await axios.get(loadBalancerUrls[urlIndex])
      const signalrUrl: string = loadBalancerResponse.data;
      console.log("Url fetched from load balancer:", signalrUrl)
      return signalrUrl;
    }
    catch {
      console.log("Failed to fetch from " + loadBalancerUrls[urlIndex] + " Retrying...")
    }
    return ""
  }

  const joinRoom = async () : Promise<void> => {
    setIsConnecting(true);
    try {
      let signalrUrl = "";
      let urlIndex = 0;
      while (signalrUrl === "" && urlIndex < 6) {
        signalrUrl = await fetchAppServerUrlFromLoadBalancer(urlIndex % 3);
        urlIndex++;
        if (urlIndex === 9) {
          console.log("Attempted to fetch application server url from load balancer "
            + urlIndex + " times. Giving up.")
          setIsConnecting(false);
          return
        }
      }

      const hubConnection = new HubConnectionBuilder()
        .withUrl(signalrUrl)
        .configureLogging(LogLevel.Information)
        .build()

      hubConnection.on("ReceiveMessage", (message) => {
        console.log("Message received:", message)
        const newMessage = { text: message.text, sender: message.username, color: message.color }
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
    }
    catch (error) {
      console.error(error)
    }
  }

  const leaveChat = async () : Promise<void> => {
    try {
      await connection?.invoke("LeaveRoom", {username, roomName})
      await connection?.stop()
    }
    catch (error) {
      console.error(error)
    }
  }

  return (
    <div className="App">
      <h1 className='title'>Distr Chat</h1>

      {!isRoomJoined && (
        <div className="card">
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

          {isConnecting && (
            <div>Connecting...</div>
          )}
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
