using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace D3DLab.Plugin {
    public interface IPluginWindow {
        event EventHandler Closed;
        object DataContext { get; set; }
        Dispatcher Dispatcher { get; }
        void Close();
        void Show();
    }
    public interface IPluginViewModel {
        void Closed();
        void Init();
    }

    public class PluginComponent {
        public PluginComponent(IPluginViewModel viewModel, UserControl view) {
            ViewModel = viewModel;
            View = view;
        }

        public IPluginViewModel ViewModel { get; }
        public UserControl View { get; }
    }

    public abstract class APluginRunner : ID3DLabPlugin {
        public string Name { get; }
        public string Description { get; }

        IPluginWindow win;
        protected APluginRunner(string name, string description) {
            Name = name;
            Description = description;
        }


        public Task CloseAsync() {
            win?.Close();
            return Task.CompletedTask;
        }


        public virtual Task ExecuteAsWindowAsync(IPluginContext context) {
            var task = Task.CompletedTask;
            try {
                win = CreateWindow();
                // w.Owner = context.Window;
                var vm = CreateViewModel(context);
                win.DataContext = vm;
                win.Closed += (o, e) => vm.Closed();

                win.Show();

                task = win.Dispatcher.InvokeAsync(() => {
                    vm.Init();
                }).Task;
            } catch (Exception ex) {
                ex.ToString();
                throw ex;
            }

            return task;
        }

        protected abstract IPluginViewModel CreateViewModel(IPluginContext context);
        protected abstract IPluginWindow CreateWindow();

        public virtual void LoadResources(IPluginContext context) {

        }

        public IPluginViewModel ExecuteAsComponent(IPluginContext context) => CreateViewModel(context);
    }
}
