using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using Database;
using WebApp.Utilities.Extensions;

namespace WebApp.Utilities.Helpers.FileController
{
    public class FileController : Controller
    {

        public ActionResult SurecKanitDosyalariIndir(int vaSurecId)
        {
            using (var entities = new VysDBEntities())
            {
                var vaSurec = entities.VASurecleris.First(f => f.VASurecID == vaSurecId);

                var surecDosyalaris = vaSurec.VASurecleriMaddes.Where(p => p.IsAktif).SelectMany(s => s.VASurecleriMaddeEklenenDosyas)
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
                            Files = sm.Select(smd => new ZipFileDto { FileName = smd.DosyaAdi, FilePath = smd.DosyaYolu }).ToList()
                        }).ToList()

                    }).ToList();

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
                var folderEntry = archive.CreateEntry(currentFolderPath + "/");
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
        private void AddFolderToZip(ZipArchive archive, string folderPath, string parentFolderName)
        {
            // Klasör altındaki dosyaları ekler
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                string entryName = parentFolderName + Path.GetFileName(filePath);
                var entry = archive.CreateEntry(entryName);
                using (var entryStream = entry.Open())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }

            // Klasör altındaki alt klasörleri dolaşır ve her birini zip dosyasına ekler
            foreach (string subFolderPath in Directory.GetDirectories(folderPath))
            {
                string folderName = parentFolderName + Path.GetFileName(subFolderPath) + "/";
                AddFolderToZip(archive, subFolderPath, folderName);
            }

            var data = new List<ZipFileDto>();
            data.AddRange(new List<ZipFileDto>
            {
                new ZipFileDto
                {
                    CreateFolder = true,
                    FolderName = "Birim 1",
                    Files = new List<ZipFileDto>
                    {
                        new ZipFileDto
                        {
                            CreateFolder = true,
                            FolderName = "Madde1", Files = new List<ZipFileDto>
                            {
                                new ZipFileDto { FileName = "Dosya1", FilePath = "/dosyalar/dosya1.pdf" },
                                new ZipFileDto { FileName = "Dosya2", FilePath = "/dosyalar/dosya2.pdf" }
                            }
                        },
                        new ZipFileDto
                        {
                            CreateFolder = true,
                            FolderName = "Madde2", Files = new List<ZipFileDto>
                            {
                                new ZipFileDto { FileName = "Dosya5", FilePath = "/dosyalar/dosya5.pdf" },
                                new ZipFileDto { FileName = "Dosya6", FilePath = "/dosyalar/dosya6.pdf" }
                            }
                        }
                    }

                },
                new ZipFileDto
                {
                    CreateFolder = true,
                    FolderName = "Birim 2",
                    Files = new List<ZipFileDto>
                    {
                        new ZipFileDto
                        {
                            CreateFolder = true,
                            FolderName = "Madde10", Files = new List<ZipFileDto>
                            {
                                new ZipFileDto { FileName = "Dosya10", FilePath = "/dosyalar/dosya10.pdf" },
                                new ZipFileDto { FileName = "Dosya20", FilePath = "/dosyalar/dosya20.pdf" }
                            }
                        },
                        new ZipFileDto
                        {
                            CreateFolder = true,
                            FolderName = "Madde20", Files = new List<ZipFileDto>
                            {
                                new ZipFileDto { FileName = "Dosya50", FilePath = "/dosyalar/dosya50.pdf" },
                                new ZipFileDto { FileName = "Dosya60", FilePath = "/dosyalar/dosya60.pdf" }
                            }
                        }
                    }

                }

            });
        }

        public class ZipFileDto
        {
            public bool CreateFolder { get; set; }
            public string FolderName { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }

            public List<ZipFileDto> Files { get; set; } = new List<ZipFileDto>();
        }

    }

}