/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProDataReaderImageMetadata
{

    public class XimgRational
    {
        private Int32 _num;
        private Int32 _denom;

        public XimgRational(byte[] bytes)
        {
            byte[] n = new byte[4];
            byte[] d = new byte[4];
            Array.Copy(bytes, 0, n, 0, 4);
            Array.Copy(bytes, 4, d, 0, 4);
            _num = BitConverter.ToInt32(n, 0);
            _denom = BitConverter.ToInt32(d, 0);
        }

        public double ToDouble()
        {
            return Convert.ToDouble(_num) / Convert.ToDouble(_denom);
        }

        public string ToString(string separator = "/")
        {
            return _num.ToString() + separator + _denom.ToString();
        }
    }

    public class XimgURational
    {
        private UInt32 _num;
        private UInt32 _denom;

        public XimgURational(byte[] bytes)
        {
            byte[] n = new byte[4];
            byte[] d = new byte[4];
            Array.Copy(bytes, 0, n, 0, 4);
            Array.Copy(bytes, 4, d, 0, 4);
            _num = BitConverter.ToUInt32(n, 0);
            _denom = BitConverter.ToUInt32(d, 0);
        }

        public double ToDouble()
        {
            return Math.Round(Convert.ToDouble(_num) / Convert.ToDouble(_denom), 2);
        }

        public override string ToString()
        {
            return this.ToString("/");
        }

        public string ToString(string separator)
        {
            return _num.ToString() + separator + _denom.ToString();
        }
    }

    public class XimgGPSRational
    {
        private XimgRational _hours;
        private XimgRational _minutes;
        private XimgRational _seconds;
        private double _degrees;

        public XimgRational Hours => _hours;

        public XimgRational Minutes => _minutes;

        public XimgRational Seconds => _seconds;
        
        public double Degrees => _degrees;

        public XimgGPSRational(byte[] bytes)
        {
            byte[] h = new byte[8]; byte[] m = new byte[8]; byte[] s = new byte[8];

            Array.Copy(bytes, 0, h, 0, 8); Array.Copy(bytes, 8, m, 0, 8); Array.Copy(bytes, 16, s, 0, 8);

            _hours = new XimgRational(h);
            _minutes = new XimgRational(m);
            _seconds = new XimgRational(s);
            _degrees = _hours.ToDouble() + (_minutes.ToDouble() / 60) + (_seconds.ToDouble() / 3600);
        }

        public override string ToString()
        {
            return _hours.ToDouble() + "° "
                + _minutes.ToDouble() + "\' "
                + _seconds.ToDouble() + "\"";
        }

        public string ToString(string separator)
        {
            return _hours.ToDouble() + separator
                + _minutes.ToDouble() + separator +
                _seconds.ToDouble();
        }
    }
}
