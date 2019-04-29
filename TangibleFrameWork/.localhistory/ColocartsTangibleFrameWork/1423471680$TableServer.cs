using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.Configuration;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
 using System.ComponentModel;

namespace TangibleFramework
{public struct  UdpState{
    public IPEndPoint e;
    public UdpClient u;
                }
    /// <summary> 
    /// Represent a Event handler for changes on Tags Objects.
    ///</summary>
    ///<author>Bilel Mnasser</author>
    public delegate void TagEventHandler(Tag sender, EventArgs e);

    public delegate void SenserDetectingEventHandler(KeyValuePair<string, List<Point>> element, EventArgs e);
    /// <summary> 
    ///This is the main Class in this framework, wich contain all the mecanism of network listening, setting of tangible table (read more about Framework design).
    /// </summary>
    /// <author>Bilel Mnasser</author>
    public class TableServer 
    {
        /// <summary> 
        /// this static Object represent a table and inside it all tags and all slabs and their tags (needed for add/insert/delete tags).
        /// </summary>
        public volatile static Table table;

        // public List<SlabPosition> SlabPositions;
        /// <summary> 
        ///Network Port number for communication with the Tangible Table (65000 default value).
        /// </summary>
        public volatile static int Port_Number = 65000;
        /// <summary> 
        ///Network Socket for network communication with the table .
        /// </summary>
        // private Socket Commanding_Socket;
        /// <summary> 
        ///This thread will be running for listening for network table tangible frames.
        /// </summary>
        public static Thread Server_Listenening_Thread;
        Dictionary<string, bool> FrameIsGeted = new Dictionary<string, bool>();
        /// <summary>
        /// Listening Statut ( ON / OFF default). 
        /// </summary>
        public volatile bool Server_Is_Running = false;
        /// <summary>
        /// time of sending event of tags from table. 
        /// </summary>
        public volatile static int TimeStep = 70;
        /// <summary>
        /// Byte Buffer for table command or Reply (1024 Byte default size).
        /// </summary>
        public volatile static Byte[] Main_Buffer = new Byte[1024];
        /// <summary> UDP Listener for reading all table frames Broadcast.
        /// </summary>
        public volatile UdpClient UDP_Listner;
        /// <summary> 
        /// This Object represent a IP Address.
        /// </summary>
        IPEndPoint Ip_Address;
        /// <summary> 
        /// static Frames Time Out integer (default value 100 milliseconds).
        /// </summary>
        public volatile static Int32 Time_Out = 1000;
        /// <summary> 
        /// XML File Path of Slabs IP List.
        /// </summary>
        public volatile static string Slabs_IP_File = @"C:\TableFiles\table.xml";
        /// <summary> 
        /// List of All Slabs IP address String.
        /// </summary>
        List<String> Connected_Slab_List;
        /// <summary> 
        /// Minimum Address Ip for the automatic Network Search (default value 100).
        /// </summary>
        static  volatile public int IP_Minimum = 111;
        /// <summary> 
        /// Maximum Address Ip for the automatic Network Search (default value 255).
        /// </summary>
        static public volatile int IP_Maximum = 113;
        /// <summary> 
        /// Table RFID Sensor Width (default value 16).
        /// </summary>
        static public volatile int Table_Sensors_WIdth = 16;
        /// <summary> 
        /// Table RFID Sensor Height (default value 16).
        /// </summary>
        static public volatile int Table_Sensors_height = 16;
        /// <summary> 
        /// Slab RFID Sensor Width (default value 4).
        /// </summary>
        static public int Slab_Sensors_WIdth = 4;
        /// <summary> 
        /// Slab RFID Sensor Height (default value 4).
        /// </summary>
        static public volatile int Slab_Sensors_Height = 4;
        /// <summary> 
        /// Minimum RSSSI Value for RSSI Detection Mode (default value is 0).
        /// </summary>
        static public volatile int Minimum_Rssi_Value = 0;
        /// <summary> 
        /// RSSSI Detection Mode Statut ( ON / OFF default).
        /// </summary>
        static public volatile bool RSSI_Mode_Is_Activated = false;
        /// <summary> 
        /// List Of All Tags ID that will be concerned.
        /// </summary>
        static public volatile List<string> List_Tags_ID = new List<string>();
        /// <summary> 
        /// Event Handler for Tags leaving the table.
        /// </summary>
        public  event  TagEventHandler Tags_Leaving;
        /// <summary> 
        /// Event Handler for Tags moving on the table.
        /// </summary>
        public  event TagEventHandler Tags_Updates;

        public event SenserDetectingEventHandler Sensors_detecting_Event_Handler;
       public  event SenserDetectingEventHandler Sensors_Not_detecting_Event_Handler;
        /// <summary> 
        ///Virtual Function for  Tags leaving (User must write his own function). 
        ///<param name="sender" >the tag object that rise the event.</param>
        ///<param name="e">the event rised by the sender.</param>
        ///<return> this function return type is void. </return>
        ///</summary>
        protected virtual void On_Tags_Leaving(Tag sender, EventArgs e)
        {
            if (Tags_Leaving != null)

                Tags_Leaving(sender, e);

        }
        /// <summary> 
        ///Virtual Function for  Tags Moving (User must write his own function). 
        ///<param name="sender">the tag object that rise the event. </param>
        ///<param name="e">the event rised by the sender.</param>
        ///<return> this function return type is void. </return>
        ///</summary>
        protected virtual void On_Tags_Moving(Tag sender, EventArgs e)
        {
            if (Tags_Updates != null)

                Tags_Updates(sender, e);

        }

        protected virtual  void Tag_Position_List_Update(KeyValuePair<string, List<Point>> element, EventArgs e)
        {


            if (Sensors_detecting_Event_Handler != null)

                Sensors_detecting_Event_Handler(element, e);



        }
        private void Tag_Position_List_leave(KeyValuePair<string, List<Point>> element, EventArgs e)
        {
            if (Sensors_Not_detecting_Event_Handler != null)

                Sensors_Not_detecting_Event_Handler(element, e);
        }


        /// <summary> 
        ///Default Constructor, Wich will initialize all class members.
        ///</summary>
        public TableServer()
        {
           
            initilize_Class_Members();

            Configure_Slabs_XML();


            Configure_Slabs_Positions();

            Configure_Slabs_Paratmers();

        }


        /// <summary> 
        /// this function will inisialize all class members (buffers, objects, Network listeners ... etc).
        ///<return> this function return type is void </return>
        ///</summary>
        static public string ServerIPAddress = "192.168.1.100";
        public void initilize_Class_Members()
        {


         
            //initialisation de toutes les variables nécessaire pour notre Server UDP
            Console.WriteLine("The SERVER IS STARTED INITIALISATION !!");
            //   SlabPositions = new List<SlabPosition>();
            table = new Table();
            // 
            Ip_Address = new IPEndPoint(IPAddress.Any, 0);
            IPAddress ip = IPAddress.Parse(ServerIPAddress);
            _UDPListenerLocalAddress = new IPEndPoint(ip, Port_Number);
            Connected_Slab_List = new List<string>();
            List_Tags_ID = new List<string>();
        }
        /// <summary> 
        ///private Function for reading Slabs Ip from XML file.
        ///<return> this function return type is void .</return>
        ///</summary>
        IPEndPoint _UDPListenerLocalAddress;
        private void Configure_Slabs_XML()
        {/*
          try
          {
              Console.WriteLine("Reading Xml Slabs IPs File");

              Serialization.DeserializeObject(ref this.Connected_Slab_List, Slabs_IP_File);

              if (this.Connected_Slab_List.Count == 0)
              {
                  Console.WriteLine("Xml has zero Slab :( Auto Configuration Of Table !" + Connected_Slab_List.Count);
                  Connected_Slab_List = Automatic_Network_Search();

              }

          }
          catch
          {

              Console.WriteLine("Error : Reading Xml Slabs IPs File Failed");
              Console.WriteLine("Will Take few Minutes Auto Configuration Of Table !");
              Connected_Slab_List = Automatic_Network_Search();
              FileInfo inf = new FileInfo(Slabs_IP_File);
              if (!Directory.Exists(inf.DirectoryName))
              {
                  Directory.CreateDirectory(inf.DirectoryName);

              }
              if (inf.Exists)
              {
                  inf.Delete();

              }
              Serialization.SerializeObject(ref Connected_Slab_List, Slabs_IP_File);


          }*/
        }
        ///<summary>
        ///Private Function for automatic setting by sending comands for each Slab on the tangible table (IP server, detection Mode... etc).
        ///<return> this function return type is void. </return>
        ///
        ///</summary>
        private void Configure_Slabs_Paratmers()
        {
            Connected_Slab_List.Add("192.168.1.121");
            Connected_Slab_List.Add("192.168.1.122");
            Connected_Slab_List.Add("192.168.1.123");
            Connected_Slab_List.Add("192.168.1.124");

            Connected_Slab_List.Add("192.168.1.131");
            Connected_Slab_List.Add("192.168.1.132");
            Connected_Slab_List.Add("192.168.1.133");
            Connected_Slab_List.Add("192.168.1.134");


            Connected_Slab_List.Add("192.168.1.141");
            Connected_Slab_List.Add("192.168.1.142");
            Connected_Slab_List.Add("192.168.1.143");
            Connected_Slab_List.Add("192.168.1.144");

            Connected_Slab_List.Add("192.168.1.151");
            Connected_Slab_List.Add("192.168.1.152");
            Connected_Slab_List.Add("192.168.1.153");
            Connected_Slab_List.Add("192.168.1.154");


            foreach (String Slabip in Connected_Slab_List)
            {
                DalleTimeLastFrame.Add(Slabip, "0");
                FrameIsGeted.Add(Slabip, false);
                if (NewConfigurationNeeded)
                {
                    Console.WriteLine("Confuguring Slab :" + Slabip);
                    Send_Command(Slabip, @"setserverip:192.168.1.100");

                    Send_Command(Slabip, @"detection:1");
                    Send_Command(Slabip, @"optimise:0");

                    if (RSSI_Mode_Is_Activated)
                    {
                        Send_Command(Slabip, @"RSSI:7");

                        Send_Command(Slabip, @"mode:2");

                        Send_Command(Slabip, @"getcfg");

                    }
                    else { Send_Command(Slabip, @"mode:1"); }

                }



            }

         

        }
        /// <summary> 
        ///Private Function for initializing Slabs positions on the tangible table.
        ///<return> this function return type is void. </return>
        ///</summary>
        ///
        static public bool NewConfigurationNeeded = false;
        private void Configure_Slabs_Positions()
        {
            /*  Console.WriteLine("Configuration 4*4 Slab Matrix Position  !");
              int d = 0;
              for (int i = 0; i < Slab_Sensors_Height + 1; i += Slab_Sensors_Height)
              {
                  if (d == Connected_Slab_List.Count || Connected_Slab_List.Count == 0)
                  { Console.WriteLine(" D Finished " + d); break; }
                  for (int j = 0; j < Slab_Sensors_WIdth + 1; j += Slab_Sensors_WIdth)
                  {
                      if (d == Connected_Slab_List.Count || Connected_Slab_List.Count == 0)
                      { break; }

                      Console.WriteLine("ip   " + Connected_Slab_List[d] + " X:" + i + " Y:" + j);

                      SlabPositions.Add(Connected_Slab_List[d], new Point(j, i));
                      Console.WriteLine(Connected_Slab_List[d]+" at X="+j+" and Y ="+ i);

                      d++;


                  }}
              */




        }
        /// <summary> 
        ///Public Function for Start Listening on Network for Broadcast tangible frames.
        ///<return> this function return type is void. </return>
        ///<example>TableServer tab=new tableServer(); tab.start(); </example>
        ///</summary>
        public void Start()
        {
            if (!Server_Is_Running)
            {
                
                Server_Is_Running = true;
                Server_Listenening_Thread = new Thread(new ThreadStart(Run));
                Server_Listenening_Thread.IsBackground = false;
              
                Server_Listenening_Thread.Start();

            }
            else
            {
                Console.WriteLine("Server is Already Running !");
            }


        }
        /// <summary> 
        ///Public Function for Stop Listening on Network for Broadcast tangible frames.
        ///<return> this function return type is void. </return>
        ///<example> TableServer tab=new tableServer(); tab.start(); tab.Stop() </example>
        ///</summary>
        public void Stop()
        {
            if (Server_Is_Running)
            {
                Server_Is_Running = false;


            }
           

        }
        /// <summary> 
        ///private Function for performing the listening on Network Broadcast tangible frames.
        ///<return> this function return type is void .</return>
        ///</summary>
        
        void print(string STRING) { Console.WriteLine(STRING); }

        Dictionary<string, string> DalleTimeLastFrame = new Dictionary<string, string>();


         //buffer de type chaine de charactere pour récupérer les données du tram
                string received_data;
                //buffer de type Byte pour récupérer tram du Listner
                byte[] receive_byte_array;
                // un buffer pour recuperer les données sous forme, donnes + temps d'envoi de tram
                Response reply ;
             
                DateTime EventStartTimer ;
                DateTime StartTimerFrame;
        
                public void Run()
                 {

                    receive_byte_array = new byte[2048];

                foreach (string tag in List_Tags_ID)
                {
                    tagTablePosition.Add(new Tag(tag, null));
                    ListOfSensors.Add(tag, null);
                }

                reply = new Response();
                EventStartTimer = DateTime.Now;
                StartTimerFrame = EventStartTimer;


                // un buffer pour recuperer les données sous forme, donnes + temps d'envoi de tram



                Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                receiver.Bind(_UDPListenerLocalAddress);

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Ip_Address = (EndPoint)sender;
                    
                        while (Server_Is_Running)
                        {

                           int receivedByteCount  = receiver.ReceiveFrom(receive_byte_array,ref Ip_Address);
                          
                           byte[] message = new byte[receivedByteCount];
                          
                           Buffer.BlockCopy(receive_byte_array, 0, message, 0, receivedByteCount);
                           received_data = Encoding.ASCII.GetString(message, 0, message.Length);
                           //Console.WriteLine(received_data);
                             string Tempsender=Ip_Address.ToString().Split(':')[0];

                            string[] replyvec = (received_data.Replace(Char.ConvertFromUtf32(Convert.ToInt32("02", 16)), "")).Split((char)Convert.ToInt32("03", 16));
                            reply.FrameValue = replyvec[0];
                            reply.FrameTime = replyvec[1];
                            char c = (char)Convert.ToInt32("03", 16);

                            //  print(DalleTimeLastFrame[Ip_Address.Address.ToString()]);
                            throw new Exception();

                            if (reply.FrameValue != null && reply.FrameValue.Contains(":"))
                            {

                                if (!FrameIsGeted[Tempsender.ToString()] && (Int64.Parse(DalleTimeLastFrame[Tempsender.ToString()]) < Int64.Parse(reply.FrameTime)))
                                {
                                    DalleTimeLastFrame[Tempsender.ToString()] = reply.FrameTime;
                                    reply.FrameValue = reply.FrameValue.Split(':')[1];
                                    table.UpdateFrameBuffer(Tempsender.ToString(), reply.FrameValue);
                                    FrameIsGeted[Tempsender.ToString()] = true;
                                    updateTags();
                                }
                                //run every frame from the table
                            }
                            TimeSpan TimePeriodForEvent = DateTime.Now.Subtract(EventStartTimer);
                            TimeSpan TimePeriodForFrame = DateTime.Now.Subtract(StartTimerFrame);
                            if (TimePeriodForFrame.TotalMilliseconds > FrameFrequency && !FrameIsGeted.ContainsValue(false))
                            {


                                FrameIsGeted["192.168.1.121"] = false;
                                FrameIsGeted["192.168.1.122"] = false;
                                FrameIsGeted["192.168.1.123"] = false;
                                FrameIsGeted["192.168.1.124"] = false;

                                FrameIsGeted["192.168.1.131"] = false;
                                FrameIsGeted["192.168.1.132"] = false;
                                FrameIsGeted["192.168.1.133"] = false;
                                FrameIsGeted["192.168.1.134"] = false;


                                FrameIsGeted["192.168.1.141"] = false;
                                FrameIsGeted["192.168.1.142"] = false;
                                FrameIsGeted["192.168.1.143"] = false;
                                FrameIsGeted["192.168.1.144"] = false;

                                FrameIsGeted["192.168.1.151"] = false;
                                FrameIsGeted["192.168.1.152"] = false;
                                FrameIsGeted["192.168.1.153"] = false;
                                FrameIsGeted["192.168.1.154"] = false;
                                StartTimerFrame = DateTime.Now;
                                updateTags();
                                updateSensorsDetectingList();
                            }
                            if (TimePeriodForEvent.TotalMilliseconds > TimeStep)
                            {
                                RunEventHandler();
                                RunEventHandlerSensorsDetecting();
                                EventStartTimer = DateTime.Now;
                            }


                        


                        }

                        receiver.Shutdown(SocketShutdown.Receive);
                        receiver.Close();
                   

                    
                
                   
                

              
           


          

                


               

           
            
        }


                public static bool RSSIFilter = true;

        List<Tag> tagTablePosition = new List<Tag>();
        public static int FrameFrequency = 70;


        void updateTags()
        {


            foreach (Tag t in tagTablePosition)
            {

                if (Get_Tag_Position(t.ID) != null)
                {
                    t.Position.Value = Get_Tag_Position(t.ID).Position.Value;
                }
                else
                {
                    t.Position.Value = calcul_Medium_point(Get_Tag_Position(t.ID).Position._history.ToArray());

                }


                }



        }

        Dictionary<string ,List<Point>> ListOfSensors =new Dictionary<string,List<Point>>()  ;
        void updateSensorsDetectingList()
        {
            foreach (Tag t in tagTablePosition)
            {
                

                  
                        ListOfSensors[t.ID] = Get_Tag_Position_List(t.ID);
                   
                
                
            
            
            
            }

         



        }


        private void RunEventHandler()
        {


            foreach (Tag tag in tagTablePosition)
            {
                if (tag.Position.Value == null)
                {
                    On_Tags_Leaving(tag, EventArgs.Empty);

                }
                else
                {
                    On_Tags_Moving(tag, EventArgs.Empty);
                }

            }


        }

        private void RunEventHandlerSensorsDetecting()
        {


            foreach (KeyValuePair<string, List<Point>> element in ListOfSensors)
            {
                if (element.Value != null)
                    Tag_Position_List_Update(element, EventArgs.Empty);
                else { Tag_Position_List_leave(element, EventArgs.Empty); }

            }


        }

      

       


        /// summary> 
        ///Static Public Function for Automatic Network Search for Slab Connected(Execution time 5 to 10 minutes and it's depend on network speed).
        ///<return> this function return type is List of Ip address String. </return>
        ///<example>Foreach(string s in TableServer.AutomaticNetworkSearch()) {Console.WriteLine(s); }; </example>
        ///</summary>
        public static List<String> Automatic_Network_Search()
        {
            List<String> Result = new List<String>();
            Ping ping = new Ping();
            for (int i = IP_Minimum; i < IP_Maximum; i++)
            {
                IPAddress address = IPAddress.Parse("192.168.1." + i);
                PingReply reply = ping.Send(address);

                if (reply.Status == IPStatus.Success)
                {
                    Result.Add(address.ToString());

                }
                else
                {
                    Console.WriteLine(address.ToString() + "is NOT connected Slab");

                }
            }
            return Result;

        }
        /// <summary> 
        /// Public Function to send a simple Tangible Command to a Single Slab.
        ///<param name="IPSlab">the string value of the Slab IP address.</param>
        ///<param name="Cmd">the string value of the Tangible Command.</param>
        ///<return> this function return type is Response Object Type.</return>
        ///<example>TableServer tab= new TableServer(); tab.SendCommand("192.186.1.101","mode:1"); </example>
        ///</summary>
        
        public void Send_Command(string IPSlab, String Cmd)
        {//object qui va contenir la reponse a la commande
            Response reply = new Response();
            //on initialize un erreur
            reply.FrameValue = "Command Error : No Data received From Slab.";
            reply.FrameTime = "0";
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(IPSlab), Port_Number);
           // new IPEndPoint(IPAddress.Parse(ServerIPAddress), Port_Number)
           
            
            Socket sender = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

            sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
            sender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
            sender.Bind(new IPEndPoint(IPAddress.Parse(ServerIPAddress), 0));
            sender.Connect(remoteEP);
             //convertion de IP de chaine de caractere en IPAddress
              //  IPAddress SlabIP = IPAddress.Parse(IPSlab);
                //    initilisation d'objet IPEndPoint      
              //  IPEndPoint endPoint = new IPEndPoint(SlabIP, Port_Number);
                //ajout de l'entete 02,03 en hexa a la commande en ascii 
                Cmd = Char.ConvertFromUtf32(Convert.ToInt32("02", 16)) + Cmd + Char.ConvertFromUtf32(Convert.ToInt32("03", 16));
                //converstion la commande en Byte dans  send_buffer
                byte[] send_buffer = Encoding.ASCII.GetBytes(Cmd);
                //initialisation de Buffer de Reponse de commande
                byte[] reply_buffer = new byte[1024];
               // UDP_Listner.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
               // UDP_Listner.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);
                //Envoie de la commande
              //  UDP_Listner.Send(send_buffer, send_buffer.Length, endPoint);
          

    // Send the message to the connected TcpServer. 
    //stream.Write(send_buffer, 0, send_buffer.Length);
                int bytesSent = sender.Send(send_buffer);
    

               // if (UDP_Listner.Available>0)
                
                    //int bytes = stream.Read(reply_buffer, 0, reply_buffer.Length);
            int bytesRec=0;
         
                bytesRec = sender.Receive(reply_buffer);
                        //UDP_Listner.Receive(ref endPoint);


                    if (bytesRec > 0)
                {//recevoir la reponse a notre commande et convertion en Ascii avec l'éliminisation des entete (02,03)
                    string[] replyvec = (ASCIIEncoding.ASCII.GetString(reply_buffer).Replace(Char.ConvertFromUtf32(Convert.ToInt32("02", 16)), "")).Trim().Split((char)Convert.ToInt32("03", 16));
                    //remplir la reponse
                    reply.FrameValue = replyvec[0];
                    //remplir le temps de la reponse
                    reply.FrameTime = replyvec[1];
                    //renvoie la reponse
                    print(reply.FrameValue.Trim() + "    " + reply.FrameTime.Trim());

                }
                else
                {//renvoie la reponse
                   
                }


                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    
        }
        /// <summary> 
        /// Private Function to get a position of tag from his ID Value.
        ///<param name="IDRFID">the string value Tag RFID Identificator.</param>
        ///<return> this function return type is tag Object Type.</return>
        ///</summary>
        private Tag Get_Tag_Position(String IDRFID)
        {
            List<Point> result = new List<Point>();



            foreach (string Ip in table.SlabsTagBuffer.Keys)
            {
                Ip.ToCharArray().ToString();
                foreach (Tag tg in table.SlabsTagBuffer[Ip].SlabTagBuffer)
                {
                    if (tg.ID == IDRFID)
                    {
                        Point position = new Point();
                        if (Ip == "192.168.1.121")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 12, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.122")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 12, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.123")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 12, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.124")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 12, tg.Position.Value.rssi);



                        else if (Ip == "192.168.1.131")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 8, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.132")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 8, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.133")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 8, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.134")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 8, tg.Position.Value.rssi);

                        else if (Ip == "192.168.1.141")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 4, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.142")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 4, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.143")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 4, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.144")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 4, tg.Position.Value.rssi);


                        else if (Ip == "192.168.1.151")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.152")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.153")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.154")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else Console.WriteLine("IP convesion Problem" + position);


                        // Console.WriteLine(tg.ID+" has been detected by "+Ip+" position : x "+tg.Position.X+" y: "+tg.Position.Y );
                        result.Add(position);



                    }



                }
            }



            if (result.Count == 0)
            {
                return new Tag(IDRFID, null); ;

            }
            else
            {

                return new Tag(IDRFID, calcul_Medium_point(result.ToArray()));
            }




        }
        private List<Point> Get_Tag_Position_List(String IDRFID)
        {
            List<Point> result = new List<Point>();



            foreach (string Ip in table.SlabsTagBuffer.Keys)
            {
                Ip.ToCharArray().ToString();
                foreach (Tag tg in table.SlabsTagBuffer[Ip].SlabTagBuffer)
                {
                    if (tg.ID == IDRFID)
                    {
                        Point position = new Point();
                        if (Ip == "192.168.1.121")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 12, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.122")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 12, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.123")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 12, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.124")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 12, tg.Position.Value.rssi);



                        else if (Ip == "192.168.1.131")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 8, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.132")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 8, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.133")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 8, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.134")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 8, tg.Position.Value.rssi);

                        else if (Ip == "192.168.1.141")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 4, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.142")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 4, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.143")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 4, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.144")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 4, tg.Position.Value.rssi);


                        else if (Ip == "192.168.1.151")
                            position = new Point(tg.Position.Value.X + 0, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.152")
                            position = new Point(tg.Position.Value.X + 4, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.153")
                            position = new Point(tg.Position.Value.X + 8, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else if (Ip == "192.168.1.154")
                            position = new Point(tg.Position.Value.X + 12, tg.Position.Value.Y + 0, tg.Position.Value.rssi);
                        else Console.WriteLine("IP convesion Problem" + position);


                        // Console.WriteLine(tg.ID+" has been detected by "+Ip+" position : x "+tg.Position.X+" y: "+tg.Position.Y );
                        result.Add(position);



                    }



                }
            }



            if (result.Count == 0)
            {
                return null;

            }
            else
            {

                return result;
            }




        }


        /// <summary> 
        /// public Function to calculate the medium point of list of points.
        ///<param name="ListPoint">the List of points objects.</param>
        ///<return> this function return type is Point Object Type.</return>
        ///</summary>
        public static Point calcul_Medium_point(Point[] ListPoint)
        {


            float x = 0.0000f;
            float y = 0.0000f;
            int HistoryCount = 0;
            for (int i = 0; i < ListPoint.Length; i++)
            {
                if (ListPoint[i] != null)
                {

                    x += ListPoint[i].X;


                    y += ListPoint[i].Y;
                    //      Console.WriteLine("Somme de Medium Point is X: " + x + "  Y: " + y);
                    HistoryCount++;

                }

            }

            if (HistoryCount > 0)
            {

                x = x / HistoryCount;

                y = y / HistoryCount;

                //    Console.WriteLine("Medium Point is X: "+x+"  Y: "+y);

                return new Point(x, y);

            }

            return null;



        }
        
        /// <summary> 
        /// public Function to Liberate All Resources and clean everything.
        ///<return> this function return type is void.</return>
        ///</summary>
  
        public static void Main()
        {
           //Process p =Process.GetProcessById(2068);
          //  p.Kill();
            TableServer.RSSI_Mode_Is_Activated = true;
            TableServer.RSSIFilter = false;
            // TableServer.Minimum_Rssi_Value = 7;
            //  on crée un objet table server
            TableServer.FrameFrequency = 200;
            TableServer.TimeStep = 100;
            HistoryPoint.HistoryBufferCount = 15;
            TableServer Tab = new TableServer();
            List_Tags_ID.Add("6697860FED2002E0");
            //  definir la fonction onPositionchanged sur le event handler "  Tab.TagsUpdates"
            Tab.Tags_Updates += new TagEventHandler(Tab.onPositionchanged);

            //  definir la fonction ontagLeavetable sur le event handler "  Tab.TagsNotOnTable "
            //   Tab.Tags_Leaving += new TagEventHandler(Tab.ontagLeavetable);
            Tab.Sensors_detecting_Event_Handler += new SenserDetectingEventHandler(Tab.ontagLeavetable);
            //  on démarre l'écoute sur la table
            Tab.Start();

            Console.ReadLine();
           // Console.WriteLine(Tab.receiver.Available);
            Tab.Stop();
           // Console.WriteLine(Tab.receiver.Available);
            Console.WriteLine(TableServer.Server_Listenening_Thread.IsAlive);
          //  Console.WriteLine(TableServer.UDP_Listner.Client.Connected + "    " + TableServer.UDP_Listner.Available);
            Thread.Sleep(10);
            Console.WriteLine(TableServer.Server_Listenening_Thread.IsAlive);
           // Console.WriteLine(Tab.UDP_Listner.Client.Available);
           /// Console.WriteLine(TableServer.UDP_Listner.Available);
          //  Console.WriteLine(TableServer.UDP_Listner.ExclusiveAddressUse);
          //  Console.WriteLine(TableServer.UDP_Listner.Client.Connected);
            //Console.WriteLine(TableServer.Server_Listenening_Thread.IsAlive);

            //Console.WriteLine(Tab.receiver.Available);
           //
            
         //   Console.WriteLine(Tab.UDP_Listner.Client.Available);
            Console.ReadLine();
        }
        private void onPositionchanged(Tag sender, EventArgs e)
        {



            Console.WriteLine(sender.ID + " X : " + sender.Position.Value.X + " Y : " + sender.Position.Value.Y);


        }
        private void ontagLeavetable(KeyValuePair<string, List<Point>> element, EventArgs e)
        {
            Console.WriteLine(" --------------------------------------------------------------------------");
            if (element.Value != null)
            {
                foreach (Point p in element.Value)
                {
                    Console.WriteLine(" Senser :" + p.X + " " + p.Y + " is detecting something distance "+p.rssi+"!!");
                }

            }
            Console.WriteLine(" --------------------------------------------------------------------------");
        }
        
        


    
    }
}
