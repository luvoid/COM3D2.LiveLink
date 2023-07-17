using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Concurrent
{
	internal class Concurrent<T>
		where T : struct
	{
		public T Value
		{
			get { lock (m_ValueLock) return _value;  }
			set { lock (m_ValueLock) _value = value; }
		}

		private object m_ValueLock;
		private T _value;

		public Concurrent(T value)
		{
			m_ValueLock = new object();
			_value = value;
		}

		public static implicit operator T(Concurrent<T> x) => x.Value;

		public override string ToString() => Value.ToString();
		public override bool Equals(object obj) => Value.Equals(obj);
		public override int GetHashCode() => Value.GetHashCode();
	}
}
