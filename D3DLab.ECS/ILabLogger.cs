using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS {
    public interface ILabLogger {
        void Debug(string message);
        void Error(Exception exception);
        void Error(Exception exception, string message);
        void Error(string message);
        void Info(string message);
        void Warn(string message);
    }
}
