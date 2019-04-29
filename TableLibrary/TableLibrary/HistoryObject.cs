using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TangibleFramework
{
    public class HistoryPoint : IEnumerable<Point>
    {
       public static int HistoryBufferCount = 1000;
        public HistoryPoint()
        {
            _history = new Queue<Point>(HistoryBufferCount); // Initiates the history stack.
        }

        public Point Value
        {
            get // Returns the top value from the history if there is one. Otherwise null.
            {
                if (_history.Count > 0)
                    return _history.Peek();
                return null;
            }
            set {

                if (_history.Count < HistoryBufferCount)
                _history.Enqueue(value);
                else
                    Undo();
                    _history.Enqueue(value);
            
            
            
            } // Adds the specified value to the history.
        }

        public void Undo()
        {
            if (_history.Count > 0)
                _history.Dequeue(); // Removes the current value from the history.
        }
       
        // History stack that will hold all previous values of the object.
        public Queue<Point> _history;
        
        public IEnumerator<Point> GetEnumerator()
        {
            return _history.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
