using System.IO;
using System.Text;

namespace WowTools.Core
{
	/// <summary>
	///  Reads WoW specific data types as binary values in a specific encoding.
	/// </summary>
	public class GenericReader : BinaryReader
	{
		#region GenericReader_stream

		/// <summary>
		///  Not yet.
		/// </summary>
		/// <param name="input">Input stream.</param>
		public GenericReader(Stream input)
			: base(input)
		{
		}

		#endregion

		#region GenericReader_stream_encoding

		/// <summary>
		///  Not yet.
		/// </summary>
		/// <param name="input">Input stream.</param>
		/// <param name="encoding">Input encoding.</param>
		public GenericReader(Stream input, Encoding encoding)
			: base(input, encoding)
		{
		}

		#endregion

		#region GenericReader_filestream

		/// <summary>
		///  Not yet.
		/// </summary>
		/// <param name="fname">Input file name.</param>
		public GenericReader(string fname)
			: base(new FileStream(fname, FileMode.Open, FileAccess.Read))
		{
		}

		#endregion

		#region GenericReader_filestream_encoding

		/// <summary>
		///  Not yet.
		/// </summary>
		/// <param name="fname">Input file name.</param>
		/// <param name="encoding">Input encoding.</param>
		public GenericReader(string fname, Encoding encoding)
			: base(new FileStream(fname, FileMode.Open, FileAccess.Read), encoding)
		{
		}

		#endregion
	}
}