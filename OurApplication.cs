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

using BACnetLibrary;

namespace BACnetInteropApp
{
    public class OurApplication
    {
        AppManager _apm;
        BACnetManager _bnm;

        public OurApplication(AppManager apm, BACnetManager bnm)
        {
            _apm = apm;
            _bnm = bnm;
        }

        public void OurApplicationThread()
        {
            // order of business
            //   send who-is-router
            //   send who-is
            //   for each discovered device
            //      get services supported
            //      get object list
            //      for each object
            //          get object type, name, pv etc.
            // repeat

            BACnetLibrary.BACnetUtil.SendWhoIsRouter(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST);
            System.Threading.Thread.Sleep(500);

            BACnetLibrary.BACnetUtil.SendWhoIs(_apm, _bnm, BACnetPacket.ADDRESS_TYPE.GLOBAL_BROADCAST);
            System.Threading.Thread.Sleep(1000);
        }
    }
}
