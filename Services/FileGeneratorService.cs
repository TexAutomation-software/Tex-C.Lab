using GZSoft.Tex.Controller.Interface;
using System;
using System.IO;
using System.Security.Cryptography;

namespace PCRA.Services
{
    internal class FileGeneratorService {
        private readonly AppSettingsService _settings;
        
        public FileGeneratorService(AppSettingsService settings) {
            _settings = settings;
        }

        private string BuildOutputPath(string filename) {
            Directory.CreateDirectory(_settings.OutputFolder);
            return Path.Combine(_settings.OutputFolder,filename);
        }

        public void GenerateLoginFile(IController controller, string password) {
            if (password == null || password == "") {
                return;
            }
            ICryptService service = (ICryptService)controller.GetControllerService(typeof(ICryptService));

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            byte[] encrypted = service.AesEncrypt(password, iv);

            string path = BuildOutputPath("LOGIN.ENC");
            using (FileStream fs = File.Create(path)) {
                fs.Write(iv, 0, iv.Length);
                fs.Write(encrypted, 0, encrypted.Length);
            }
        }

        public void GenerateChangePasswordFile(IController controller, string password) {
            if (password == null || password == "") {
                return;
            }
            ICryptService service = (ICryptService)controller.GetControllerService(typeof(ICryptService));

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            byte[] encrypted = service.AesEncrypt(password, iv);

            string path = BuildOutputPath("CHANGEPWD.ENC");
            using (FileStream fs = File.Create(path)) {
                fs.Write(iv, 0, iv.Length);
                fs.Write(encrypted, 0, encrypted.Length);
            }
        }

        public void GenerateResetPasswordFile(IController controller, byte[] mac, long utcTime) {
            if (mac == null || mac.Length == 0 || mac.Length != 6) {
                return;
            }
            ICryptService service = (ICryptService)controller.GetControllerService(typeof(ICryptService));

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            byte[] dati = new byte[32];

            for (int i = 0; i < 6; i++)
                dati[i] = mac[i];

            dati[6] = (byte)(utcTime >> 24);
            dati[7] = (byte)(utcTime >> 16);
            dati[8] = (byte)(utcTime >> 8);
            dati[9] = (byte)utcTime;

            byte[] encrypted = service.AesEncrypt(dati, iv);

            string path = BuildOutputPath("RESET.ENC");
            using (FileStream fs = File.Create(path))
            {
                fs.Write(iv, 0, iv.Length);
                fs.Write(encrypted, 0, encrypted.Length);
            }
        }

        public void GenerateCardParsFile(IController controller, string crdFileName) {
            if (crdFileName == null || crdFileName == "") {
                return;
            }
            ICryptService service = (ICryptService)controller.GetControllerService(typeof(ICryptService));

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            FileStream input = File.OpenRead(crdFileName);
            string path = BuildOutputPath("CARDPARS.ENC");
            FileStream output = File.Create(path);

            // Scrivo l'IV nei primi 16 byte del file
            output.Write(iv, 0, iv.Length);

            service.AesEncrypt(input, output, iv);
        }

        public void GenerateChecksumFile(IController controller, string binFileName) {
            if (binFileName == null || binFileName == "") {
                return;
            }
            ICryptService service = (ICryptService)controller.GetControllerService(typeof(ICryptService));

            byte[] iv = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv);

            byte[] dati = new byte[32];

            using (FileStream fs = File.OpenRead(binFileName)) {
                fs.Seek(16, SeekOrigin.Begin); // byte 16-19
                fs.Read(dati, 0, 4);
            }

            byte[] encrypted = service.AesEncrypt(dati, iv);
            string path = BuildOutputPath("CHECKSUM.ENC");
            using (FileStream fs = File.Create(path)) {
                fs.Write(iv, 0, iv.Length);
                fs.Write(encrypted, 0, encrypted.Length);
            }
        }
    }
}
