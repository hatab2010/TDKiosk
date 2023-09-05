using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TDKiosk
{
    public class Loader : BindableObject
    {
        public event Action Relesed;
        public event Action Updated;

        public bool IsActive { private set; get; }
        object LoaderLock = new object();
        public float Duration { private set; get; }
        public float Progress { private set; get; } = 1;

        private Task _processTask = null;
        private Task _incraseTask;
        private float _incraseStep;
        private bool _isStop = false;
        private float _interval = 100;
        private float _currentDuration;
        int _updateInterval = 100;

        public Loader(float duration, float incarseStep)
        {            
            _incraseStep = incarseStep;
            _currentDuration = duration;
            Duration = duration;
            Progress = 1;

            if (_incraseStep > 1)
                throw new ArgumentException();
        }

        public void Start()
        {
            if (_processTask != null)            
                return;

            _isStop = false;

            _processTask = Task.Run(() =>
            {
                while (true)
                {                    
                    //bool isBreak;
                    lock (LoaderLock)
                    {
                        if (_isStop)
                            break;
                    }

                    Thread.Sleep(_updateInterval);

                    lock (LoaderLock)
                    {
                        if (_isIncrase == false)                        
                            _currentDuration -= _updateInterval;
                        
                        if (_currentDuration < 0) _currentDuration = 0;
                        var progress = _currentDuration / Duration;
                        Progress = progress > 0 ? progress : 0;

                        Updated?.Invoke();

                        if (progress <= 0)
                        {
                            lock (LoaderLock)
                            {
                                Relesed?.Invoke();
                            }

                            _isStop = true;
                            _processTask = null;
                            break;
                        }
                    }
                }
            });
        }

        public void Restart()
        {
            Stop();

            lock (LoaderLock)
            {
                Progress = 1;
                _currentDuration = Duration;
                Updated?.Invoke();
            }

            Start();
        }

        public void Stop()
        {
            lock (LoaderLock)
                _isStop = true;

            try { _processTask.Wait(); }
            catch (Exception ex) { }

            lock (LoaderLock)
            {
                _isStop = false;
                _processTask = null;
            }
        }

        bool _isIncrase;
        public void StartIncrase()
        {
            lock(LoaderLock)
                if (_isStop)
                    return;

            Stop();
            var stopwatch = new Stopwatch();
            var interval = 100;
            stopwatch.Start();
            
            float _incraseDuration = _incraseStep * Duration;           

            if (_incraseTask != null)
                return;

            _isIncrase = true;
            _incraseTask = Task.Run(() =>
            {
                while (true)
                {
                    lock (LoaderLock)
                    {
                        if (_isIncrase == false)
                        {
                            stopwatch.Stop();
                            stopwatch = null;
                            break;
                        }

                        var incrase = stopwatch.ElapsedMilliseconds;
                        float duration = _currentDuration + incrase / 2;
                        if (duration > Duration) duration = Duration;

                        Progress = duration / Duration;
                        _currentDuration = duration;
                        Updated?.Invoke();
                    }

                    Thread.Sleep(interval);
                }
            });           
        }

        public void StopIncrase()
        {
            lock (LoaderLock)
            {
                lock (LoaderLock)
                    _isIncrase = false;
            }

            try { _incraseTask.Wait(); } catch (Exception ex) { }

            lock (LoaderLock)
            {
                _incraseTask = null;
            }

            Start();
        }
    }
}