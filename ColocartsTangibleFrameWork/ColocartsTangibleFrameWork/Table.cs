using System;
using System.Collections.Generic;
using System.Text;

namespace TangibleFramework
{  /// <summary> 
    ///This Class represent the table wich contain a list of Slabs and a tags Buffer.
    /// </summary>
    /// <author>Bilel Mnasser</author>
    public class Table {
        /// <summary> Frame Buffer </summary>
        public Dictionary<String, String> FrameBuffer;
        /// <summary> Slabs View Buffer </summary>
        public Dictionary<String, Slab> SlabsTagBuffer;

        /// <summary> default constructor wich initialize all buffers. </summary>
        public Table() 
        {
           
            FrameBuffer = new Dictionary<string, string>();

            SlabsTagBuffer = new Dictionary<string, Slab>();

        }
        /// <summary> 
        ///this function is used to update Frame Buffer values.
        ///<param name="IpKey">the string value of Slab IP address.</param>
        ///<param name="FrameValue">the string value of Slab Frame data.</param>
        ///<return> this function return type is void. </return>
        ///</summary>
        public void UpdateFrameBuffer(String IpKey,String FrameValue) 
        {


            if (this.FrameBuffer.ContainsKey(IpKey))
            {
                if (this.FrameBuffer[IpKey] != FrameValue)
                {
                  
                    this.FrameBuffer.Remove(IpKey);
                    this.FrameBuffer.Add(IpKey, FrameValue);
                    SlabsTagBuffer[IpKey] = new Slab();
                    UpdateSlabsTagBuffer(IpKey, FrameValue);

                }
                else
                {
               

                }

            }
            else
            {


             
                this.FrameBuffer.Add(IpKey, FrameValue);
                AddToFrameBuffer( IpKey,  FrameValue);


            }


        
        
        }
        /// <summary> 
        ///this function is used to add value to Frame Buffer values.
        ///<param name="IpKey">the string value of Slab IP address.</param>
        ///<param name="FrameValue">the string value of Slab Frame data.</param>
        ///<return> this function return type is void </return>
        ///</summary>
        private void AddToFrameBuffer(string IpKey, string FrameValue)
        {
            SlabsTagBuffer.Add(IpKey, new Slab());
            if (FrameValue != "")
            {

                SlabsTagBuffer[IpKey].UpdateBuffer(FrameValue);

            }

        }

        /// <summary> 
        ///this function is used to update Slabs Tag Buffer values.
        ///<param name="IpKey">the string value of Slab IP address.</param>
        ///<param name="FrameValue">the string value of Slab Frame data.</param>
        ///<return> this function return type is void .</return>
        ///</summary>
        public void UpdateSlabsTagBuffer(String IpKey, String FrameValue)
        {
            if (this.SlabsTagBuffer.ContainsKey(IpKey))
            {
                if (FrameValue == "")
                {

                    SlabsTagBuffer[IpKey].SlabTagBuffer.Clear();

                }
                else 
                {
                    SlabsTagBuffer[IpKey].SlabTagBuffer.Clear();

                    SlabsTagBuffer[IpKey].UpdateBuffer(FrameValue);
                
                }

            }
            



        }

 



    }
   }
