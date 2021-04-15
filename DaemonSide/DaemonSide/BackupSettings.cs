using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DaemonSide
{
    class BackupSettings
    {
        PcSettings ps = new PcSettings();
        Http http = new Http();
        public List<PcBackup> GetConfigs()
        {
            string api = "/api/pcbackup/idPc?id=";
            string result = http.GetAsyncID(api, Pc.Instance.Id).Result;
            List<PcBackup> configIds = JsonConvert.DeserializeObject<List<PcBackup>>(result);
            return configIds;
        }
        public List<BackupFiles> GetPaths(int id)
        {
            string api = "/api/file/idConfig?id=";
            string result = http.GetAsyncID(api, id).Result;
            List<BackupFiles> backupFiles = JsonConvert.DeserializeObject<List<BackupFiles>>(result);
            return backupFiles;
        }
        public List<Time> GetTimes(int id)
        {
            string api = "/api/time/idConfig?id=";
            string result = http.GetAsyncID(api, id).Result;
            List<Time> times = JsonConvert.DeserializeObject<List<Time>>(result);
            return times;
        }
        public List<Storage> GetStorages(int id)
        {
            string api = "/api/storage/idConfig?id=";
            string result = http.GetAsyncID(api, id).Result;
            List<Storage> storages = JsonConvert.DeserializeObject<List<Storage>>(result);
            return storages;
        }
        public BackupConfig GetBackupConfig(int id)
        {
            string api = "/api/backupconfig/";
            string result = http.GetAsyncID(api, id).Result;
            BackupConfig backupConfig = JsonConvert.DeserializeObject<BackupConfig>(result);
            return backupConfig;
        }
        public PcBackup GetPcBackudId(int idPc, int idConfig)
        {
            string api = "/api/pcbackup/idPcAndConfig?";
            string ids = String.Format("idPc={0}&idConfig={1}", idPc, idConfig);
            string result = http.GetAsyncIDMulti(api, ids).Result;
            PcBackup pcBackupId = JsonConvert.DeserializeObject<PcBackup>(result);
            return pcBackupId;
        }
        public void PostBackup(Object obj)
        {
            string api = "/api/backup/";
            string result = http.PostAsync(api, obj).Result;
            Backup backup = JsonConvert.DeserializeObject<Backup>(result);
        }
        public List<Backup> GetBackupById(int id)
        {
            string api = "/api/backup/idPcBackup?id=";
            string result = http.GetAsyncID(api, id).Result;
            List<Backup> backups = JsonConvert.DeserializeObject<List<Backup>>(result);
            return backups;
        }
        public void BackupDone(string log, PcBackup pcBackupId, int fileCount, int fileCountFailed, int fileCountSuccess, string backupOperations)
        {
            Backup backup = new Backup();
            backup.BackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            backup.FileCount = fileCount;
            backup.FileCountFailed = fileCountFailed;
            backup.FileCountSuccess = fileCountSuccess;
            backup.Errors = log;
            backup.IdPcBackUp = pcBackupId.Id;
            backup.Operations = backupOperations;
            PostBackup(backup);
        }
    }
}
