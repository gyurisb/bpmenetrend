using CityTransitApp;
using CityTransitApp.Common;
using CityTransitServices.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.Common.Processes
{
    public struct DatabaseDownloadProgress
    {
        public string Message;
        public int Percentage;
    }
    public enum DatabaseDownloadResult { NoAccess, Success, Initialized };

    public class DatabaseDownloader : Process<DatabaseDownloader, DatabaseDownloadProgress, DatabaseDownloadResult>
    {
        protected override async Task<DatabaseDownloadResult> StartAsync()
        {
            try
            {
                //ha kényszerített (első telepítés), akkor a rendelkezésre álló db-t töltöm be, egyébként letöltöm a szerverről
                Stream fileStream;
                int updatesVersion = CommonComponent.Current.Config.DBNumber;
                bool downloadDisabled = !CommonComponent.Current.DatabaseExists || AppFields.ForceUpdate;
                if (downloadDisabled)
                {
                    fileStream = getResourceDatabase();
                }
                else
                {
                    //updates.txt letöltése és mentése
                    updatesVersion =    await getUpdatesVersion();
                    fileStream =        await downloadDatabase(updatesVersion);
                }
                //Az adatbázis kicsomagolása, előtte a használt fájlokat lezárja
                await decompressDatabase(fileStream);
                fileStream.Dispose();
                //Az adatbázis betöltése, az előző adatbázis alapján a backup elvégzése
                await completeDatabase();
                UpdateMonitor.UpdateDone(updatesVersion);
                return downloadDisabled ? DatabaseDownloadResult.Initialized : DatabaseDownloadResult.Success;
            }
            catch (WebException)
            {
                return DatabaseDownloadResult.NoAccess;
            }
        }

        private Stream getResourceDatabase()
        {
            var file = Services.FileSystem.GetFileFromApplicationUri(CommonComponent.Current.Config.DefaultDatabase);
            return file.OpenForRead();
        }

        private async Task<int> getUpdatesVersion()
        {
            Stream resultStream = await Services.Http.GetAsync(CommonComponent.Current.Config.GetUpdatesUrl());
            if (resultStream == null)
                throw new WebException("Updates file unavailable", WebExceptionStatus.ConnectFailure);
            return int.Parse(new StreamReader(resultStream).ReadLine());
        }

        private async Task<Stream> downloadDatabase(int updatesVersion)
        {
            Stream resultStream = await Services.Http.GetAsync(CommonComponent.Current.Config.GetDownloadSource(updatesVersion), downloadProgressChanged);
            if (resultStream == null)
                throw new WebException("No database", WebExceptionStatus.ConnectFailure);
            return resultStream;
        }


        private void downloadProgressChanged(NetPortableServices.HttpProgress e)
        {
            string percentText = String.Format("{0} {1:0.00} MB / {2:0.00} MB",
                //Services.Localization.StringOf("DownloadProgress"),
                "",
                e.BytesReceived / (1024 * 1024.0), 
                e.TotalBytesToReceive / (1024 * 1024.0)
                );
            PerformProgressChanged(percentText, 100.0 * e.BytesReceived / (e.TotalBytesToReceive != 0 ? e.TotalBytesToReceive : double.MaxValue));
        }

        private async Task decompressDatabase(Stream fileStream)
        {
            if (CommonComponent.Current.TB.ContainsData)
            {
                PerformProgressChanged(Services.Resources.LocalizedStringOf("DownloadWaitDB"));
                await CommonComponent.Current.TB.UsingFiles;
                CommonComponent.Current.TB.Dispose();
            }

            PerformProgressChanged(Services.Resources.LocalizedStringOf("DownloadDecompress"));
            await Services.Compression.UnzipAsync(fileStream, Services.FileSystem.GetAppStorageRoot());
        }

        private async Task completeDatabase()
        {
            PerformProgressChanged(Services.Resources.LocalizedStringOf("DownloadLoadDB"));

            if (CommonComponent.Current.UB.HasAnyData())
            {
                if (CommonComponent.Current.TB.ContainsData)
                {
                    var backup = CommonComponent.Current.UB.CreateBackup();
                    CommonComponent.Current.LoadTransitBase();
                    backup.ExportToContext();
                }
                else
                {
                    CommonComponent.Current.DeleteUserBase();
                }
            }
            else
            {
                CommonComponent.Current.LoadTransitBase();
            }

            PerformProgressChanged(Services.Resources.LocalizedStringOf("DownloadReady"));

            AppFields.ForceUpdate = false;
        }

        public void PerformProgressChanged(string message, double percentage = 0)
        {
            PerformProgressChanged(new DatabaseDownloadProgress { Message = message, Percentage = (int)Math.Round(percentage) });
        }
    }
}
