using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using Database;
using WebApp.Utilities.Extensions;
using WebApp.Utilities.MessageBox;

namespace WebApp.Utilities.Helpers.FileController
{
    public class FileController : Controller
    {
        public FileContentResult SurecKanitDosyalariZipFileContent(int vaSurecId, List<int> maddeTurIds)
        {
            maddeTurIds = maddeTurIds ?? new List<int>();
            var mMessage = new MmMessage
            {
                IsSuccess = false,
                Title = "Veri kanıt dosyası indirme işlemi",
                MessageType = Msgtype.Warning
            };
            if (!maddeTurIds.Any())
            {
                mMessage.Messages.Add("İndirme işlemi için en az 1 madde türü seçmeniz gerekmektedir.");
                return null;
            }
            using (var entities = new VysDBEntities())
            {
                var vaSurec = entities.VASurecleris.First(f => f.VASurecID == vaSurecId);

                var surecDosyalaris = vaSurec.VASurecleriMaddes.Where(p => p.IsAktif && maddeTurIds.Contains(p.MaddeTurID)).SelectMany(s => s.VASurecleriMaddeEklenenDosyas)
                       .ToList();

                var documentGroups = surecDosyalaris.GroupBy(g => new { g.Birimler.BirimAdi })
                    .Select(s => new ZipFileDto
                    {
                        CreateFolder = true,
                        FolderName = s.Key.BirimAdi,
                        Files = s.GroupBy(gd => new { gd.VASurecleriMadde.MaddeKod, gd.VASurecleriMadde.Maddeler.MaddeAdi }).Select(sm => new ZipFileDto
                        {
                            CreateFolder = true,
                            FolderName = sm.Key.MaddeKod + "-" + sm.Key.MaddeAdi,
                            Files = sm.Select(smd => new ZipFileDto {   FilePath = smd.DosyaYolu }).ToList()
                        }).Where(p => p.Files.Any()).ToList()

                    }).Where(p => p.Files.Any()).ToList();

                // Zip dosyasını oluştur
                byte[] zipBytes = CreateZip(documentGroups);
                var fileName = vaSurec.Yil + " yılı veri kanıt dosyaları " + vaSurec.BaslangicTarihi.ToFormatDate() + "-" + vaSurec.BitisTarihi.ToFormatDate() + ".zip";
                // Zip dosyasını indirilebilir bir dosya olarak döndür 
                return File(zipBytes, "application/zip", fileName);
            }
        }

        private byte[] CreateZip(List<ZipFileDto> data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Her bir veriyi işle
                    foreach (var item in data)
                    {
                        AddItemToZip(archive, item);
                    }
                }
                return memoryStream.ToArray(); // Oluşturulan zip dosyasını byte dizisi olarak döndür
            }
        }

        private void AddItemToZip(ZipArchive archive, ZipFileDto item, string currentFolderPath = "")
        {
            // Eğer CreateFolder true ise, yeni bir klasör oluştur
            if (item.CreateFolder)
            {
                item.FolderName = item.FolderName.RemoveInvalidFileNameChars();
                currentFolderPath = Path.Combine(currentFolderPath, item.FolderName);
                archive.CreateEntry(currentFolderPath + "/");
                foreach (var file in item.Files)
                {
                    AddItemToZip(archive, file, currentFolderPath);
                }
            }
            else // Eğer CreateFolder false ise, dosyayı ekle
            {
                if (!string.IsNullOrEmpty(currentFolderPath))
                {
                    currentFolderPath += "/";
                }

                string dosyaYolu = HostingEnvironment.MapPath(item.FilePath);
                if (System.IO.File.Exists(dosyaYolu))
                {
                    var fileName = new FileInfo(dosyaYolu).Name;
                    var entry = archive.CreateEntry(currentFolderPath + fileName);
                    using (var fileStream = new FileStream(dosyaYolu, FileMode.Open))
                    using (var entryStream = entry.Open())
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
        }

        public class ZipFileDto
        {
            public bool CreateFolder { get; set; }
            public string FolderName { get; set; } 
            public string FilePath { get; set; }

            public List<ZipFileDto> Files { get; set; } = new List<ZipFileDto>();
        }

    }

}