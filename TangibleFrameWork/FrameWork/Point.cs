using System;
using System.Collections.Generic;

using System.Text;

namespace TangibleFramework
{/// <summary> 
    /// This class represent a Position inside the table wich is represented into 2D dimension (X and Y).
    /// </summary>
    /// <author>Bilel Mnasser</author>
   [Serializable]
  public  class Point
    {
        /// <summary>
        /// X axis Value
       /// </summary>
     
    public float X;
    /// <summary> 
      ///Y axis Value
      ///</summary>
  
    public float Y;
    /// <summary>
    /// Connstructeur par défaut 
    /// </summary>
    /// 
    public int rssi;
    public Point()
    {

        

    }
    /// <summary> 
    ///parametered constructor 
    ///<param name="x" />
    ///X axis value
    ///<param name="y" />
    ///Y axis value
    ///</summary>
  
       public Point(float x, float y) 
    {

        this.X = x;
        this.Y = y;
    
    
    }
       public Point(float x, float y,int rssi)
       {

           this.X = x;
           this.Y = y;
           this.rssi = rssi;

       }
    }
}
