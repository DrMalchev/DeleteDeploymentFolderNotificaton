using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace DeleteDeploymentFolder
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid("12e8cc71-83c8-4e8d-8dc1-c8fafae01a92")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class ImplementDeletion : AsyncPackage
    {


        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            string pathToDelete = GetPathOfDeploymentFolder();
            await Task.Run(() => 
            { 
                UseOutputWindowAsync(1, 1).ConfigureAwait(false); 

            });
            bool directoryExists = Directory.Exists(pathToDelete);
            //if (directoryExists)
            //{
               await Task.Run(()=> DeleteDeploymentFolder("Delete App_Data/Sitefinity/Deployment folder!"));
            //};

        }

        private static string GetPathOfDeploymentFolder()
        {
            var path = Directory.GetCurrentDirectory();
            var pathStripped = path.Split('\\');
            pathStripped = pathStripped.Take(pathStripped.Length - 2).ToArray();
            string pathToDelete = String.Join("/", pathStripped) + "/App_Data/Sitefinity/Deployment";
            return pathToDelete;
        }

        private void DeleteDeploymentFolder(string message)
        {
            UseVsMessageBoxAsync(message)
                .ConfigureAwait(false);

            
        }


        #region VS Message box
        private Task UseVsMessageBoxAsync(string message)
        {
            var result = VsShellUtilities.ShowMessageBox(
               this,
               $"{message}",
               "R E M I N D E R",
               OLEMSGICON.OLEMSGICON_INFO,
               OLEMSGBUTTON.OLEMSGBUTTON_OK,
               OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            if (result == 6)
            // 6 = yes button, 7 = no button
            {
                Directory.Delete(GetPathOfDeploymentFolder());
                return Task.CompletedTask;
            }

            return Task.CompletedTask;


        }
        #endregion

        private IVsOutputWindowPane _pane;
        private async Task UseOutputWindowAsync(int currentSteps, int numberOfSteps)
        {
            if (_pane == null)
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
                var ow = await GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
                Assumes.Present(ow);

                var guid = Guid.NewGuid();
                ow.CreatePane(ref guid, "My output", 1, 1);
                ow.GetPane(ref guid, out _pane);

                _pane.Activate();
                _pane.OutputStringThreadSafe(GetPathOfDeploymentFolder());
            }
            else
            {
                _pane.OutputStringThreadSafe(GetPathOfDeploymentFolder());
            }
        }
    }
}
