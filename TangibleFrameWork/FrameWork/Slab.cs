using System;
using System.Collections.Generic;
using System.Text;
namespace TangibleFramework
{ 
    
   
    /// <summary> 
    ///This Class Represent a Slab into the Table witch contain a Buffer for tags detected by each Slab (read more about This Framework Design).
    /// </summary>
    /// <author>Bilel Mnasser</author>
    public class Slab
    {

        /// <summary> 
        ///This buffer represent the objects Tags detected for each Slab
        /// </summary>
        public List<Tag> SlabTagBuffer;

        float GetIndexOf(string rfid)
        {
            for (int i = 0; i < SlabTagBuffer.Count; i++)
            {
                if (SlabTagBuffer[i].ID == rfid)
                    return i;
                 
            }
            return -1f;




        }
        /// <summary> 
        ///Default Constructor who only intialize the buffer
        /// </summary>
        public Slab(){

            SlabTagBuffer = new List<Tag>();
        }
        /// <summary> 
        ///Clean Buffer 
        /// </summary>
        public void ClearBuffer()
        {


            this.SlabTagBuffer.Clear();

        }
        /// <summary> 
        ///this function is used to update slab Buffer values
        ///<param name="FrameValue">the string value of the Slab Frame data</param>
       ///<return> this function return type is void </return>
       ///</summary>
        public void UpdateBuffer(String FrameValue)
        {
           // Console.WriteLine(FrameValue);
            String[] Parts = FrameValue.Split('_');
            List<string> RFIDLastFrame = new List<string>();
           // Console.WriteLine(Parts);
            for (int i = 0; i < Parts.Length - 1; i++)
            {
                String[] RFIDPArt = Parts[i].Split('.');
                
                string ValueRF = RFIDPArt[0];
                string Slabcoord = RFIDPArt[1];
                int X = Convert.ToInt32(Slabcoord[0]) - 48;
                int Y = Convert.ToInt32(Slabcoord[1]) - 48;
                int rssi;
                RFIDLastFrame.Add(ValueRF);
                if (TableServer.RSSI_Mode_Is_Activated)
                {
                    rssi = Convert.ToInt32(Slabcoord[2]) - 48;
                    if (TableServer.RSSIFilter)
                    {
                        if (TableServer.Minimum_Rssi_Value <= rssi)
                        {

                            this.SlabTagBuffer.Add(new Tag(ValueRF, new Point(X, Y, rssi)));



                        }
                    }
                    else { this.SlabTagBuffer.Add(new Tag(ValueRF, new Point(X, Y, rssi))); }
                   

                   
                }
                else
                {
                    if (GetIndexOf(ValueRF) < 0)
                        this.SlabTagBuffer.Add(new Tag(ValueRF, new Point(X, Y)));
                    else
                        this.SlabTagBuffer[(int)GetIndexOf(ValueRF)].Position.Value = new Point(X, Y);
              }
 }

            for (int i = 0; i < SlabTagBuffer.Count; i++)
            {
                if (!RFIDLastFrame.Contains(SlabTagBuffer[i].ID))
                    SlabTagBuffer.RemoveAt(i);


            }


   }







        /// <summary> 
        ///this function is used to initialize Buffer values
        ///<param name="FrameValue">the string value of Slab Frame data</param>
        ///<return> this function return type is void </return>
        ///</summary>

        internal void initialize(string FrameValue)
        {




            this.SlabTagBuffer.Clear();
            String[] Parts = FrameValue.Split('_');
            for (int i = 0; i < Parts.Length - 1; i++)
            {
                String[] RFIDPArt = Parts[i].Split('.');

                string ValueRF = RFIDPArt[0];
                string Slabcoord = RFIDPArt[1];
                int X = Convert.ToInt32(Slabcoord[0]) - 48;
                int Y = Convert.ToInt32(Slabcoord[1]) - 48;
                int rssi;
                if (TableServer.RSSI_Mode_Is_Activated)
                {
                    rssi = Convert.ToInt32(Slabcoord[2]) - 48;

                    if (TableServer.Minimum_Rssi_Value <= rssi)
                    {

                        this.SlabTagBuffer.Add(new Tag(ValueRF, new Point(X, Y)));

                    }
                    else continue;


                }
                else
                {

                    this.SlabTagBuffer.Add(new Tag(ValueRF, new Point(X, Y)));



                }

              /*  EventTag = new Tags();
                EventTag.RFIDVAlue = ValueRF;
                EventTag.Position = TableServer.Get_Tag_Position(ValueRF).Position;
                NewTagsEntered(EventArgs.Empty);
                EventTag = null;
                /// <summary> /// for (int k = 0; k < 100;k++ )
                Console.WriteLine("X :" + X + " Y: " + Y + " Value :" + ValueRF + "\n");
                */
            }
          


        }

      
    }
}

