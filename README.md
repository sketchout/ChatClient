ChatClient
==========


ChatClient is connect to the StringBasedServer in Java.
This is oriented from Win Form client.
It has Socket Handle, UI Thread, Json Parser.

1. Send message

    string message="Hello안녕"; 
    
    byte[] bMessage = Encoding.UTF8.GetBytes(message); 
    
    

2. Recv message


    byte[] bMessage = new byte[iReadLen]; 
    
    try { 
    
      clientSocket.Receive(bMessage);
      
    } catch ( Exception e ) {
    
      throw e;
      
    }
    
    string messsage = Encoding.GetEncoding(myEncoding).GetString(bMessage);
    
    
    
