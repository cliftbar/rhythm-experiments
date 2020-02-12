using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows.Media;

using ConsoleGameEngine;

namespace RhythmExperiments2
{
	class RhythmGame : ConsoleGame
	{
		TimingMaster _master;
		public int _sleeptime;
		DateTime _starttime;
		int referenceBeat;
		Random randomGen;

		private static void Main(string[] args)
		{

			new RhythmGame().Construct(24, 30, 16, 16, FramerateMode.MaxFps);
			
		}

		public override void Create()
		{
			referenceBeat = 0;
			randomGen = new Random();
			Engine.SetPalette(Palettes.Pico8);

			//SimpleTimingSource testSource = new SimpleTimingSource(60, 100);
			AudioTimingSource testAudioSource = new AudioTimingSource(148, "sample.mp3");

			Debug.WriteLine($"Total Length: {testAudioSource.duration_sec}");

			this._master = new TimingMaster(testAudioSource, 100);

			//SimpleTimingWorker worker = new SimpleTimingWorker(master, "Worker 1");
			SimpleTimingWorker worker2 = new SimpleTimingWorker(this._master, "Worker 2", this.Engine);
			//AsyncSimpleTimingWorker worker3 = new AsyncSimpleTimingWorker(this._master, "Async Worker 3", this.Engine);
			//AsyncSimpleTimingWorker worker4 = new AsyncSimpleTimingWorker(master, "Async Worker 4");

			//this._master.RunLoop(this);
			//System.Threading.Thread masterThread = new System.Threading.Thread(this._master.AIORunLoop);
			this._master._timingSource.start();

			// thread out the Timing Loop
			Task.Run(this._master.AIORunLoop);
			//Engine.Frame(new Point(1, 0), new Point(10, 10), 7);
			//masterThread.Start();
			//this._master.AIORunLoop();

			//this._sleeptime = this._master.refreshRate_ms;
			//this._starttime = this._master.LoopStart();
		}

		public override void Render()
		{
			//Engine.ClearBuffer();


			//Engine.DisplayBuffer();
		}

		public override void Update()
		{
			//Engine.ClearBuffer();
			if (Engine.GetKeyDown(ConsoleKey.A))
			{
				double position = this._master.position_beat - 0.1; // keypress/update loop lag offset?  up to the full time the loop takes? not sure thats right

				double offset = position - Math.Round(position);
				Engine.WriteText(new Point(2, 20), $"Timing Check:", 1);
				Engine.WriteText(new Point(2, 21), $"{Math.Round(offset, 3)}", 1);
			}

			if (this.referenceBeat + 1 <= this._master.position_beat)
			{
				//double position = this._master.position_beat;

				//double offset = position - Math.Round(position);
				//double timing = (this._master.position_beat - (this.referenceBeat + 1));
				//Engine.WriteText(new Point(2, 20), $"Timing Check:", 1);
				//Engine.WriteText(new Point(2, 21), $"{offset}", 1);

				//Engine.ClearBuffer();
				////randomGen = new Random();

				//for (int i = 0; i < 5; i++)
				//{
				//	int posx = randomGen.Next(0, 15);
				//	int posy = randomGen.Next(0, 15);
				//	int color = randomGen.Next(1, 16);

				//	Engine.SetPixel(new Point(posx, posy), color);
				//}

				//Engine.ClearBuffer();
				//randomGen = new Random();

				int posx = 3; //randomGen.Next(0, 15);
				int posy = 3; //randomGen.Next(0, 15);
				int color = randomGen.Next(1, 16);

				Engine.SetPixel(new Point(posx, posy), color);

				this.referenceBeat += 1;
			}
			
			//Point p = new Point(10, 20);
			//Engine.WriteText(p, "test", 8);

			//bool doLoopUpdate = true;

			//try
			//{
			//	if (doLoopUpdate) {
			//		this._sleeptime = this._master.RunLoopUpdate(this._starttime, this._sleeptime);
			//	}
			//}
			//catch (ExecutionEngineException)
			//{
			//	doLoopUpdate = false;
			//}

			Engine.DisplayBuffer();
			

			//System.Threading.Thread.Sleep(this._sleeptime);
			//Console.WriteLine("test");
		}
	}

	class TimingMaster
	{
		public TimingSource _timingSource;

		public double beatsPerMinute { get { return this._timingSource.beatsPerMinute; } }
		public double secondsPerBeat { get { return 60d / this._timingSource.beatsPerMinute; } }

		public double position_sec { get; private set; }

		public double position_beat { get { return this.position_sec / this.secondsPerBeat; } }

		public double totalDuration_sec { get { return this._timingSource.duration_sec; } }
		// public float elapsedTime_sec { get { return this. } }

		public int referenceBeat { get; protected set; }
		public int refreshRate_ms { get; protected set; }


		public TimingMaster(TimingSource timingSource, int refreshRate_ms)
		{
			this._timingSource = timingSource;
			this.refreshRate_ms = refreshRate_ms;
		}

		public void AIORunLoop()
		{
			int sleeptime = this.refreshRate_ms;
			DateTime starttime = DateTime.Now;

			//this._timingSource.start();

			while (true)
			{
				DateTime current_time = DateTime.Now;

				this.position_sec = (current_time - starttime).TotalSeconds;

				if (this.position_sec > this.totalDuration_sec)
				{
					Debug.WriteLine("Source Done");
					break;
				}

				//System.Threading.Tasks.Task.Run(this.DoBeat);
				this.DoBeat();

				// Adjust the sleep time to account for the time spend calculating, to prevent drift
				sleeptime = ((int)this.position_sec * 1000) % this.refreshRate_ms;
			}
		}

		public void RunLoop(RhythmGame game)
		{
			int sleeptime = this.refreshRate_ms;
			//DateTime starttime = DateTime.Now;

			//this._timingSource.start();

			DateTime starttime = this.LoopStart();

			while (true)
			{
				//try
				//{
				//	sleeptime = this.RunLoopUpdate(starttime, sleeptime);
				//}
				//catch (ExecutionEngineException)
				//{
				//	break;
				//}

				game.Update();
				sleeptime = game._sleeptime;

				System.Threading.Thread.Sleep(sleeptime);
			}
		}

		public DateTime LoopStart()
		{
			DateTime starttime = DateTime.Now;

			this._timingSource.start();
			return starttime;
		}

		public int RunLoopUpdate(DateTime starttime, int sleeptime)
		{
			DateTime current_time = DateTime.Now;

			this.position_sec = (current_time - starttime).TotalSeconds;

			if (this.position_sec > this.totalDuration_sec)
			{
				Debug.WriteLine("Source Done");
				throw new ExecutionEngineException("Source Done");
			}

			//System.Threading.Tasks.Task.Run(this.DoBeat);
			this.DoBeat();

			// Adjust the sleep time to account for the time spend calculating, to prevent drift
			return ((int)this.position_sec * 1000) % this.refreshRate_ms;
		}

		public event EventHandler Beat;

		protected virtual void OnBeat(EventArgs e)
		{
			this.Beat?.Invoke(this, e);
		}

		private void DoBeat()
		{
			int newWholeBeat = (int)this.position_beat;

			if (newWholeBeat <= this.referenceBeat) { return; }

			ExecuteBeat(newWholeBeat);
		}

		private void ExecuteBeat(int newWholeBeat)
		{
			this.referenceBeat = newWholeBeat;

			//Debug.WriteLine($"Master Beat Done.  Position: {this.position_sec}, Beats: {this.position_beat}");
			this.OnBeat(EventArgs.Empty);
		}
	}

	/******************/
	/* Timing Sources */
	/******************/
	abstract class TimingSource
	{
		public double beatsPerMinute { get; protected set; }
		public double numberOfBeats { get; protected set; }

		//public double startTimeOffset { get { return this.numberOfBeats; } }
		public double duration_sec { get; protected set; }

		abstract public void start();
	}

	class SimpleTimingSource : TimingSource
	{
		public SimpleTimingSource(double beatsPerMinute, double numberOfBeats)
		{
			this.beatsPerMinute = beatsPerMinute;
			this.numberOfBeats = numberOfBeats;
			this.duration_sec = numberOfBeats / beatsPerMinute * 60d;

		}

		public override void start()
		{
			
			Debug.WriteLine("Simple Timing Start, nothing to see here");
		}
	}

	class AudioTimingSource : TimingSource
	{
		private MediaPlayer _audioPlayer;
		//private Boolean _mediaOpen = false;

		public AudioTimingSource(double beatsPerMinute, string audioSourceRelativePath)
		{
			this.beatsPerMinute = beatsPerMinute;

			this._audioPlayer = new MediaPlayer();

			this._audioPlayer.Open(new Uri(Path.Combine(@"C:\Users\cwbarclift\MyFiles\Code\CSharp\RhythmExperiments2\RhythmExperiments2", audioSourceRelativePath)));
			//this._audioPlayer.MediaOpened += this.blockUntilOpen;

			//while (!this._mediaOpen)
			//{
			//	System.Threading.Thread.Sleep(100);
			//}

			while (true) {
				try
				{
					this.duration_sec = this._audioPlayer.NaturalDuration.TimeSpan.TotalSeconds;
					break;
				}
				catch (InvalidOperationException)
				{
				}
			}
			this.numberOfBeats = this.duration_sec / 60d * this.beatsPerMinute;
			
		}

		//private void blockUntilOpen(object sender, EventArgs e)
		//{
		//	this._mediaOpen = true;
		//}

		public override void start()
		{
			this._audioPlayer.Play();
		}
	}

	/***********************/
	/* Synchronous Workers */
	/***********************/
	abstract class TimingWorker
	{
		protected TimingMaster _timingMaster;

		public TimingWorker(TimingMaster timingMaster)
		{
			this._timingMaster = timingMaster;
			this._timingMaster.Beat += this.Beat;
		}

		protected virtual void Beat(object sender, EventArgs e)
		{
			Debug.WriteLine($"Worker Beat Done.  Position: {this._timingMaster.position_sec}, Beats: {this._timingMaster.position_beat}");
		}
	}

	class SimpleTimingWorker : TimingWorker
	{
		public string workerName { get; protected set; }
		private int _modder = 0;
		ConsoleGameEngine.ConsoleEngine engine;

		public SimpleTimingWorker(TimingMaster timingMaster, string workerName, ConsoleGameEngine.ConsoleEngine engine) : base(timingMaster)
		{
			this.workerName = workerName;
			this.engine = engine;
		}

		protected override void Beat(object sender, EventArgs e)
		{
			
			this._modder = (_modder + 1) % 4;
			//this.engine.SetPixel(new Point(8, 8), this._modder, ConsoleCharacter.Light);  //WriteText(new Point(1, 1), $"{this._modder}", 6);
			//Console.WriteLine($"{this._modder}");
			
			//Debug.WriteLine($"Worker {this.workerName} Beat Done.  Position: {this._timingMaster.position_sec}, Beats: {this._timingMaster.position_beat}");
		}
	}

	/*****************/
	/* Async Workers */
	/*****************/
	class AsyncTimingWorker : TimingWorker
	{
		public AsyncTimingWorker(TimingMaster timingMaster) : base(timingMaster) { }

		protected override async void Beat(object sender, EventArgs e)
		{
			Debug.WriteLine($"Async Worker Beat Done.  Position: {this._timingMaster.position_sec}, Beats: {this._timingMaster.position_beat}");
		}
	}

	class AsyncSimpleTimingWorker : AsyncTimingWorker
	{
		public string workerName { get; protected set; }
		private int _modder = 0;
		public AsyncSimpleTimingWorker(TimingMaster timingMaster, string workerName) : base(timingMaster)
		{
			this.workerName = workerName;

		}

		protected override async void Beat(object sender, EventArgs e)
		{
			//await Task.Delay(500);
			this._modder = (this._modder + 1) % 4;
			Console.WriteLine($"{this._modder}");
			Debug.WriteLine($"Async Worker {this.workerName} Beat Done.  Position: {this._timingMaster.position_sec}, Beats: {this._timingMaster.position_beat}");
		}
	}

}
