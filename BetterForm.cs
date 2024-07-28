public class BetterForm : System.Windows.Forms.Form
{
	#region Public Variables
	public int RequestQueInterval
	{
		get
		{
			return _requestPump.Interval;
		}
		set
		{
			RunLambda(() =>
			{
				_requestPump.Stop();
				_requestPump.Interval = value;
				_requestPump.Start();
			});
		}
	}
	#endregion
	#region Private Variables
	private object _requestQueLock = new object();
	private System.Collections.Generic.List<Request> _requestQue = new System.Collections.Generic.List<Request>();
	private System.Windows.Forms.Timer _requestPump = new System.Windows.Forms.Timer();

	private object _clearingQueLock = new object();
	private bool _clearingQue = false;

	private bool _runningLambda = false;
	private object _runningLambdaLock = new object();
	#endregion
	#region Public Constructors
	public BetterForm()
	{
		FormClosing += (object sender, System.Windows.Forms.FormClosingEventArgs e) => { ClearRequestQue(); };
		_requestPump.Interval = 100;
		_requestPump.Tick += (object sender, System.EventArgs e) => { ClearRequestQue(); };
	}
	#endregion
	#region Public Methods
	public void ShowOnSubThread(int statusQueryInterval = 50, System.Threading.ThreadPriority subThreadPriority = System.Threading.ThreadPriority.Normal)
	{
		if (statusQueryInterval < 0)
		{
			throw new System.Exception("statusQueryInterval must be greater than or equal to 0.");
		}

		bool loaded = false;

		Load += (object sender, System.EventArgs e) => { loaded = true; };

		System.Threading.Thread subThread = new System.Threading.Thread(() =>
		{
			_requestPump.Start();
			ShowDialog();
		});

		subThread.IsBackground = false;
		subThread.Name = "BetterForm Thread";
		subThread.Priority = subThreadPriority;

		subThread.Start();

		if (statusQueryInterval is 0)
		{
			while (!loaded)
			{

			}
		}
		else
		{
			while (!loaded)
			{
				System.Threading.Thread.Sleep(statusQueryInterval);
			}
		}
	}
	public object RunLambda(AsyncReturnParamLambda lambda, object parameter, int statusQueryInterval = 25)
	{
		lock (_runningLambdaLock)
		{
			if (_runningLambda)
			{
				throw new System.Exception("Recursive lambda execution is not supported and will cause a deadlock.");
			}
			_runningLambda = true;
		}

		if (lambda is null)
		{
			throw new System.Exception("lambda cannot be null.");
		}

		if (statusQueryInterval < 0)
		{
			throw new System.Exception("statusQueryInterval must be greater than or equal to 0.");
		}

		Request request = new Request();

		request._lambda = lambda;
		request._parameter = parameter;

		lock (_requestQueLock)
		{
			_requestQue.Add(request);
		}

		if (statusQueryInterval is 0)
		{
			while (!request._completed)
			{

			}
		}
		else
		{
			while (!request._completed)
			{
				System.Threading.Thread.Sleep(statusQueryInterval);
			}
		}

		if (!request._succeeded)
		{
			lock (_runningLambdaLock)
			{
				_runningLambda = false;
			}

			throw request._exception;
		}
		else
		{
			lock (_runningLambdaLock)
			{
				_runningLambda = false;
			}

			return request._output;
		}
	}
	public void RunLambda(AsyncParamLambda lambda, object parameter, int statusQueryInterval = 25)
	{
		RunLambda(async (object _) => { await lambda.Invoke(parameter); return null; }, null, statusQueryInterval);
	}
	public object RunLambda(AsyncReturnLambda lambda, int statusQueryInterval = 25)
	{
		return RunLambda(async (object _) => { return await lambda.Invoke(); }, null, statusQueryInterval);
	}
	public void RunLambda(AsyncLambda lambda, int statusQueryInterval = 25)
	{
		RunLambda(async (object _) => { await lambda.Invoke(); return null; }, null, statusQueryInterval);
	}
	public object RunLambda(ReturnParamLambda lambda, object parameter, int statusQueryInterval = 25)
	{
		return RunLambda(async (object _) => { return lambda.Invoke(parameter); }, null, statusQueryInterval);
	}
	public void RunLambda(ParamLambda lambda, object parameter, int statusQueryInterval = 25)
	{
		RunLambda(async (object _) => { lambda.Invoke(parameter); return null; }, null, statusQueryInterval);
	}
	public object RunLambda(ReturnLambda lambda, int statusQueryInterval = 25)
	{
		return RunLambda(async (object _) => { return lambda.Invoke(); }, null, statusQueryInterval);
	}
	public void RunLambda(Lambda lambda, int statusQueryInterval = 25)
	{
		RunLambda(async (object _) => { lambda.Invoke(); return null; }, null, statusQueryInterval);
	}
	#endregion
	#region Private Methods
	private async void ClearRequestQue()
	{
		lock (_clearingQueLock)
		{
			if (_clearingQue)
			{
				return;
			}
			_clearingQue = true;
		}

		System.Collections.Generic.List<Request> localRequestQue = null;

		lock (_requestQueLock)
		{
			localRequestQue = new System.Collections.Generic.List<Request>(_requestQue);
			_requestQue.Clear();
		}

		for (int i = 0; i < localRequestQue.Count; i++)
		{
			Request request = localRequestQue[i];

			try
			{
				request._output = await request._lambda.Invoke(request._parameter);
				request._succeeded = true;
			}
			catch (System.Exception exception)
			{
				request._succeeded = false;
				request._exception = exception;
			}

			request._completed = true;
		}

		lock (_clearingQueLock)
		{
			_clearingQue = false;
		}
	}
	#endregion
	#region Private Sub Classes
	private class Request
	{
		#region Internal Variables
		internal bool _completed = false;
		internal bool _succeeded = false;
		internal AsyncReturnParamLambda _lambda = null;
		internal object _parameter = null;
		internal object _output = null;
		internal System.Exception _exception = null;
		#endregion
	}
	#endregion
	#region Private Delegates
	public delegate System.Threading.Tasks.Task<object> AsyncReturnParamLambda(object parameter);
	public delegate System.Threading.Tasks.Task AsyncParamLambda(object parameter);
	public delegate System.Threading.Tasks.Task<object> AsyncReturnLambda();
	public delegate System.Threading.Tasks.Task AsyncLambda();
	public delegate object ReturnParamLambda(object parameter);
	public delegate void ParamLambda(object parameter);
	public delegate object ReturnLambda();
	public delegate void Lambda();
	#endregion
}