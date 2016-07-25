using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace AOI_VTWIN_RNS.Aoicollector.Core
{
    public class Worker
    {
        public AoiController aoi;

        public bool Detener = false;
        public Timer timer;
        public int timerInterval = 10;
        public ProgressBar progressBar;
        public BackgroundWorker bgWorker = new BackgroundWorker();
        public int timesExcecuted = 0;

        public DoWorkEventHandler WorkerStart;

        public Worker(AoiController aoiController)
        {
            aoi = aoiController;
        }

        #region Timer
        /// <summary>
        /// Inicializa el timer para ejecutar las operaciones del worker
        /// </summary>
        public void StartTimer()
        {
            int milisegundos = (int)TimeSpan.FromSeconds(timerInterval).TotalMilliseconds;

            timer = new Timer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = milisegundos;
            timer.Enabled = true;
            timer.Start();

            aoi.aoiLog.Area("Timer iniciado en: " + aoi.aoiConfig.intervalo + " seg");
            Core.Log.Sys("Timer de " + aoi.aoiConfig.machineType + " fue iniciado. Intervalo: " + aoi.aoiConfig.intervalo + " seg ");
        }

        public void StopTimer()
        {
            Detener = true;
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
                Core.Log.Sys("Se detuvo la ejecucion del timer de la maquina: " + aoi.aoiConfig.machineType, "atencion");
                aoi.aoiLog.Area("Timer de ejecucion detenido con exito. ", "atencion");
            }
            else
            {
                aoi.aoiLog.Area("El timer no se encuentra en ejecucion. ", "atencion");
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            StartOperation();
        }
        #endregion

        #region Worker
        public void StartOperation(bool forceStart = false)
        {
            if (forceStart) { Detener = false; }
            if (!Detener)
            {
                if (timer == null)
                {
                    StartTimer();
                }

                if (!bgWorker.IsBusy)
                {
                    ExecuteWorker();
                }
                else
                {
                    aoi.aoiLog.Area("*** Espere, el proceso se encuentra aun en ejecucion");
                }
            }
        }

        public BackgroundWorker ExecuteWorker()
        {
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new DoWorkEventHandler(WorkerStart);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(WorkerProgressChanged);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerRunCompleted);
            bgWorker.RunWorkerAsync();

            return bgWorker;
        }

        private void WorkerRunCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            aoi.aoiLog.Area("----------------------- Operacion completa ---------------------------");

            if (timesExcecuted >= 100)
            {
                aoi.aoiLog.AreaReset();
                timesExcecuted = 0;
            }
        }

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        public void SetProgressTotal(int count)
        {
            progressBar.Invoke((MethodInvoker)(() => progressBar.Maximum = count));
        }

        public void SetProgressWorking(int count)
        {
            bgWorker.ReportProgress(count);
        }
        #endregion
    }
}
