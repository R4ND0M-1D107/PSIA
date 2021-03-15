using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DamienG.Security.Cryptography;

namespace udp_random
{
	class UnitCrc
	{
		public static bool CrcCheker (byte[] data)
		{
			UInt32 crc = BitConverter.ToUInt32(data, 0);
			UInt32 crc_data = Crc32.Compute(data.Skip(5).ToArray());
			if (crc != crc_data)
				return false;
			else
				return true;
		}
	}
}
