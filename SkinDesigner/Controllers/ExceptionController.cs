using System;
using System.Collections.Generic;
using System.IO;
using SkinDesigner.Windows;

namespace SkinDesigner.Controllers {
    public class ExceptionController {
        public int ExceptionCounter { get; set; }
        public List<Exception> ExceptionInfos { get; set; }
        public ExceptionWindow ExceptionWindow { get; protected set; }
        public Exception LastException { get; set; }

        public bool ShowException(Exception exception) {
            LastException = exception;
            WriteLog();
            ExceptionWindow = new ExceptionWindow();
            ExceptionWindow.SetException(LastException);
            var showDialog = ExceptionWindow.ShowDialog();
            return showDialog != null && (bool) showDialog;
        }

        public void WriteLog() {
            if (LastException == null) return;
            File.AppendAllText("error.log", string.Format(@"{0} {1} {2}{4}{3}{4}{4}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), LastException, LastException.Message, LastException.StackTrace, Environment.NewLine));
        }
    }
}
