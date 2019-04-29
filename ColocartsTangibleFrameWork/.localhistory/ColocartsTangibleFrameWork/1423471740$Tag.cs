using System;
using System.Text;

namespace ColocartsTangibleFrameWork
{/// <summary> 
    /// This class used for a Tangible Tag witch contain ID and Position.
    /// </summary>
    /// <author>Bilel Mnasser</author>
   public class Tag
   {/// <summary> 
    /// Identification of a Tangible Tag
    /// </summary>
        public String ID="";
        /// <summary> 
        /// Position of a Tangible Tag 
        /// </summary>
        public HistoryPoint Position;
        /// <summary> 
       ///parametered constructor 
        ///<param name="ID" />
        ///Identificator of Tag
        ///<param name="Position" />
        ///Position of tag
       ///</summary>
        public Tag(string ID, Point Position)
        {
            this.Position = new HistoryPoint();
            this.ID = ID;
            this.Position.Value = Position;
        
        
        }
        /// <summary> 
        /// default constructor
        /// </summary>
        public Tag() { 
         this.Position=new HistoryPoint();
        
        
        }

    }
}
