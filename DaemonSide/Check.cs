using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DaemonSide
{
    class Check
    {
        PcSettings pcSettings = new PcSettings();
        Http http = new Http();
        BackupSettings backupSettings = new BackupSettings();

        public bool Login()
        {
            Console.Write("Username: ");
            User.Instance.Username = Console.ReadLine();
            Console.Write("Password: ");
            User.Instance.Password = Console.ReadLine();
            string api = "/api/sessions/";
            string result = http.PostAsync(api, User.Instance).Result;
            if(!result.Contains("invalid")) { return true; }
            return false;
        }
        public void Id()
        {
            string idPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\id.sad";
            Pc.Instance.Name = pcSettings.GetName();
            Pc.Instance.IpAddress = pcSettings.GetIpAddress().Result;
            Pc.Instance.MacAddress = pcSettings.GetMacAddress();
            Pc.Instance.OS = pcSettings.GetOs();
            if (File.Exists(idPath))
            {
                Pc.Instance.Id = int.Parse(File.ReadAllText(idPath));
            }
            else
            {
                string api = "/api/pc/";
                Pc.Instance.State = "offline";
                string result = http.PostAsync(api, Pc.Instance).Result;
                Pc.Instance = JsonConvert.DeserializeObject<Pc>(result);
                using (StreamWriter writer = File.CreateText(idPath))
                {
                    writer.WriteLine(Pc.Instance.Id);
                }
            }
        }
        public void UpdatePc()
        {
            string api = "/api/pc/";
            string result = http.GetAsyncID(api, Pc.Instance.Id).Result;
            Pc.Instance = JsonConvert.DeserializeObject<Pc>(result);
            if (String.IsNullOrEmpty(result)) { string idPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\id.sad"; File.Delete(idPath); Pc.Instance.Id = new int(); Id(); }
            if (Pc.Instance.State == "blocked") { return; }
            Pc.Instance.IpAddress = pcSettings.GetIpAddress().Result;
            Pc.Instance.MacAddress = pcSettings.GetMacAddress();
            Pc.Instance.OS = pcSettings.GetOs();
            string result2 = http.PutAsync(api, Pc.Instance.Id, Pc.Instance).Result;
        }
        public void DoBackup()
        {
            foreach (var pcBackup in backupSettings.GetConfigs())
            {
                foreach (var time in backupSettings.GetTimes(pcBackup.IdConfig))
                {
                    if ((time.Date == null) || (DateTime.Parse(time.Date) > DateTime.Now)) { continue; }
                    if (BackupTime(time.Frequency, time.Date, time.Days))
                    {
                        foreach (var storage in backupSettings.GetStorages(pcBackup.IdConfig))
                        {
                            BackupConfig backupConfig = backupSettings.GetBackupConfig(pcBackup.IdConfig);
                            PcBackup pcBackupId = backupSettings.GetPcBackudId(Pc.Instance.Id, pcBackup.IdConfig);
                            List<Backup> backups = backupSettings.GetBackupById(pcBackupId.Id);
                            string backupDirectoryName = String.Format("/{0:dd-MM-yyyy}_{1:HH-mm}_{2}_{3}/", DateTime.Now.Date, DateTime.Now, backupConfig.Type, pcBackupId.Id);
                            string storagePath = String.Format(storage.Path + backupDirectoryName);
                            string log = ""; int fileCount = 0; int fileCountFailed = 0; int fileCountSuccess = 0; string spliter; string backupOperations = "";
                            switch (storage.Place)
                            {
                                case "Local":
                                    {
                                        spliter = BackupLocal(storagePath, storage, backupConfig, pcBackup, backups);
                                        log = spliter.Split('|')[0];
                                        fileCount = int.Parse(spliter.Split('|')[1]);
                                        fileCountFailed = int.Parse(spliter.Split('|')[2]);
                                        fileCountSuccess = int.Parse(spliter.Split('|')[3]);
                                        backupOperations = spliter.Split('|')[4];
                                        break;
                                    }
                                case "FTP":
                                    {
                                        /*Only Local backups are suported now.*/
                                        break;
                                    }
                            }
                            backupSettings.BackupDone(log, pcBackupId, fileCount, fileCountFailed, fileCountSuccess, backupOperations);
                        }
                    }
                }
            }
        }
        public bool BackupTime(string frequency, string date, string day)
        {
            var dateT = DateTime.Parse(date);
            var dateTNow = DateTime.Now;
            var time = dateT.TimeOfDay;
            var timeNow = dateTNow.TimeOfDay;
            var timeRange = TimeSpan.FromSeconds(599);
            if (frequency == "week")
            {
                if (day == dateTNow.DayOfWeek.ToString() && time.Add(timeRange) >= timeNow && time <= timeNow) { return true; }
                else if (dateT.DayOfWeek == dateTNow.DayOfWeek && time.Add(timeRange) >= timeNow && time <= timeNow) { return true; }
            }
            else if (frequency == "day" && time.Add(timeRange) >= timeNow && time <= timeNow) { return true; }
            else if (frequency == "non" && dateT.Add(timeRange) >= dateTNow && dateT <= dateTNow) { return true; }
            else if (frequency == "year" && dateT.AddYears(dateTNow.Year - dateT.Year).ToShortDateString() == dateTNow.ToShortDateString() && time.Add(timeRange) >= timeNow && time <= timeNow) { return true; }
            else if (frequency == "month" && dateT.AddYears(dateTNow.Year - dateT.Year).AddMonths(dateTNow.Month - dateT.Month).ToShortDateString() == dateTNow.ToShortDateString() && time.Add(timeRange) >= timeNow && time <= timeNow) { return true; }
            return false;
        }
        public string BackupLocal(string storagePath, Storage storage, BackupConfig backupConfig, PcBackup pcBackup, List<Backup> backups)
        {
            string log = ""; int fileCount = 0; int fileCountFailed = 0; int fileCountSuccess = 0; int check = 0; string backupOperations = "";
            if (!Directory.Exists(storage.Path)) { log += String.Format("\nLocal storage path does not exist. (\"{0}\")", storage.Path); string cantLocal = String.Format("{0}|{1}|{2}|{3}", log, fileCount, fileCountFailed, fileCountSuccess); return cantLocal; }
            Directory.CreateDirectory(storagePath);
            List<string> operations = new List<string>();
            List<string> firstoperations = new List<string>();
            if (backupConfig.Type == "IB" || backupConfig.Type == "DB")
            {
                foreach (var backup in backups)
                {
                    try { operations.AddRange(backup.Operations.Split("\n")); } catch (Exception e) { }
                    if (check == 0) { try { firstoperations.AddRange(backup.Operations.Split("\n")); } catch (Exception e) { } check++; }
                }
                foreach (string operation in operations)
                {
                    if (backupConfig.Type == "IB" && !File.Exists(operation) && operation != "" && !operation.Contains("REMOVE")) { backupOperations += "REMOVE " + operation + "\n"; }
                    if (backupConfig.Type == "DB" && !firstoperations.Contains(operation)) { backupOperations += operation + "\n"; }
                }
                foreach (string first in firstoperations)
                {
                    if (backupConfig.Type == "DB" && !File.Exists(first) && first != "" && !first.Contains("REMOVE")) { backupOperations += "REMOVE " + first + "\n"; }
                }
            }
            foreach (var backupFiles in backupSettings.GetPaths(pcBackup.IdConfig))
            {
                if (Directory.Exists(backupFiles.Path))
                {
                    DirectoryInfo di = new DirectoryInfo(backupFiles.Path);
                    Directory.CreateDirectory(storagePath + di.Name + '/');
                    foreach (var dirPath in Directory.GetDirectories(di.FullName, "*", SearchOption.AllDirectories))
                        Directory.CreateDirectory(dirPath.Replace(di.FullName, storagePath + di.Name + '/'));
                    foreach (var newPath in Directory.GetFiles(di.FullName, "*.*", SearchOption.AllDirectories))
                    {
                        fileCount++;
                        if ((backupConfig.Type == "IB" && !operations.Contains(newPath)) || (backupConfig.Type == "DB" && !firstoperations.Contains(newPath)) || backupConfig.Type == "FB")
                        {
                            try
                            {
                                File.Copy(newPath, newPath.Replace(di.FullName, storagePath + di.Name + '/'), true); fileCountSuccess++;
                                if (backupConfig.Type == "IB" || backupConfig.Type == "DB") { backupOperations += newPath + "\n"; }
                            }
                            catch (Exception e) { log += String.Format("\n{0} (\"{1}\")", e.ToString(), newPath); fileCountFailed++; }
                        }
                    }
                }
                else if (File.Exists(backupFiles.Path))
                {
                    fileCount++;
                    if ((backupConfig.Type == "IB" && !operations.Contains(backupFiles.Path)) || (backupConfig.Type == "DB" && !firstoperations.Contains(backupFiles.Path)) || backupConfig.Type == "FB")
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(backupFiles.Path); File.Copy(fi.FullName, storagePath + fi.Name, true); fileCountSuccess++;
                            if (backupConfig.Type == "IB" || backupConfig.Type == "DB") { backupOperations += backupFiles.Path + "\n"; }
                        }
                        catch (Exception e) { log += String.Format("\n{0} (\"{1}\")", e.ToString(), backupFiles.Path); fileCountFailed++; }
                    }
                }
                else { log += String.Format("\nDirectory or file does not exists or has been deleted. (\"{0}\")", backupFiles.Path); fileCount++; fileCountFailed++; }
            }
            string combiner = String.Format("{0}|{1}|{2}|{3}|{4}", log, fileCount, fileCountFailed, fileCountSuccess, backupOperations);
            return combiner;
        }
    }
}
