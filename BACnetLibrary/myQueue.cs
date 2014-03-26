/*
 * The MIT License
 * 
 * Copyright (c) 2010 BACnet Iteroperability Testing Services, Inc.
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * 
 *  BACnet Interoperability Testing Services, Inc.
 *      http://www.bac-test.com
 * 
 * BACnet Wiki
 *      http://www.bacnetwiki.com
 * 
 * MIT License - OSI (Open Source Initiative) Approved License
 *      http://www.opensource.org/licenses/mit-license.php
 * 
*/

/*
 * 28 Nov 10    EKH Releasing under MIT license
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetLibrary
{

    // This wrapper provides thread-safe queues for communicating between threads.

    public class myQueue<T> : Queue<T>
    {
        public void myEnqueue ( T obed )
        {
            lock (this)
            {
                base.Enqueue(obed);
            }
        }


        public T myDequeue()
        {
            lock (this)
            {
                return base.Dequeue();
            }
        }

        public new int Count
        {
            get
            {
                int count;
                lock (this)
                {
                    count = base.Count;
                }
                return count;
            }
        }
    }
}
