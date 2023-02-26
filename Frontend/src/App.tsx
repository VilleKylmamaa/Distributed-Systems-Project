import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import axios from 'axios';
import { useState } from 'react';
import './App.css';
import ChatBox from './Components/ChatBox';
import Message from './Entities/Message';

const loadBalancerUrls = [
  "http://localhost:7001/LoadBalancer",
  "http://localhost:7002/LoadBalancer",
  "http://localhost:7003/LoadBalancer"
]

/* const appServerUrls = [
  "http://localhost:5001/SignalrHub",
  "http://localhost:5002/SignalrHub",
  "http://localhost:5003/SignalrHub"
] */

function App() {
  const [ connection, setConnection ] = useState<HubConnection | null>(null);
  const [ isRoomJoined, setRoomJoined ] = useState<boolean>(false);
  const [ messages, setMessages ] = useState<Array<Message>>([]);
  const [ username, setUsername ] = useState<string>("");
  const [ roomName, setRoomName ] = useState<string>("");
  const [ isConnecting, setIsConnecting ] = useState<boolean>(false);
  const [ connectingMessage, setConnectingMessage ] = useState<string>("");
  const [ errorMessage, setErrorMessage ] = useState<string>("");

  const fetchAppServerUrlFromLoadBalancer = async(urlIndex: number) : Promise<string> => {
    try {
      console.log("Fetching application server url from load balancer:", loadBalancerUrls[urlIndex])
      const loadBalancerResponse = await axios.get(loadBalancerUrls[urlIndex])
      const signalrUrl: string = loadBalancerResponse.data;
      console.log("Url fetched from load balancer:", signalrUrl)
      return signalrUrl
    }
    catch {
      console.log("Failed to fetch from " + loadBalancerUrls[urlIndex] + " Retrying...")
    }
    return ""
  }

  const joinRoom = async () : Promise<void> => {
    setErrorMessage("")
    setIsConnecting(true)
    let signalrUrl = "";
    let urlIndex = 0;
    let connectingMessageTimeout = 0

    try {
      while (signalrUrl === "" && urlIndex < 9) {
        const urlIndexModulo = urlIndex % 3;
        // Delay first connection message to avoid flashing in the case of immediate connection
        if (urlIndex === 0) {
          connectingMessageTimeout = setTimeout(() => {
            setConnectingMessage(`Fetching application server url from ${loadBalancerUrls[urlIndexModulo]}`);
          }, 400)
        } else{
          setConnectingMessage(`Fetching application server url from ${loadBalancerUrls[urlIndexModulo]}`);
        }

        signalrUrl = await fetchAppServerUrlFromLoadBalancer(urlIndexModulo);
        urlIndex++;

        if (urlIndex === 9 || signalrUrl === "about:blank") {
          console.log("Attempted to fetch application server url from load balancer "
            + urlIndex + " times. Giving up.")
          clearTimeout(connectingMessageTimeout)
          setIsConnecting(false)
          setErrorMessage("Error: Failed to fetch application server url from load balancer.")
          setConnectingMessage("")
          return
        }
      }
      clearTimeout(connectingMessageTimeout)

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
      setIsConnecting(false)
      setConnectingMessage("")
    }
    catch (error) {
      console.error(error)
      setIsConnecting(false)
      setConnectingMessage("")
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
            disabled={!roomName || isConnecting}
          >
            Join room
          </button>

          <div className="status-container">
            {isConnecting && errorMessage === "" && <div className="connecting-message">{connectingMessage}</div>}
            {errorMessage !== "" && <div className="error-message">{errorMessage}</div>}
          </div>
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
