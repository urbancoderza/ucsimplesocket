﻿using System;
using System.Threading;

namespace SimpleSocket
{
	public sealed partial class Connection
	{
		private volatile int _disposed;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
			{
				if (_receiveWorker != null)
				{
					if (_cancelTokenSource != null)
						_cancelTokenSource.Cancel(true);
					_receiveWorker.Dispose();
					if (_cancelTokenSource != null)
						_cancelTokenSource.Dispose();
				}

				if (_packer != null)
					_packer.Dispose();
				if (_unpacker != null)
					_unpacker.Dispose();
			}

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Default destructor.
		/// </summary>
		~Connection()
		{
			Dispose();
		}
	}
}